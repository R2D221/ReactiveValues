using ReactiveValues.Exceptions;
using ReactiveValues.Helpers;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace ReactiveValues;

public abstract partial class Reactive
{
	private protected static readonly ExceptionDispatchInfo defaultException =
		ExceptionDispatchInfo.Capture(new UnreachableException());

	private protected static readonly ThreadLocal<bool> frozen = new();

	public static bool IsWatched(Reactive reactive)
	{
		using var s = reactive.GetInternals(LockAction._);
		return s.IsWatched;
	}

	private protected readonly Dictionary<Reactive, ReactiveLastKnownValue> sources = [];
	private protected readonly Queue<Reactive> modifiedSources = [];

	private protected readonly HashSet<WeakReference<Reactive>> receivers =
		new(comparer: WeakEqualityComparer<Reactive>.Instance);

	private protected readonly HashSet<WeakReference<Reactive>> watchers =
		new(comparer: WeakEqualityComparer<Reactive>.Instance);

	private CountdownEvent? objLock;
	private bool isValid;
	private int version;
	private protected ExceptionDispatchInfo? exception = defaultException;
	private bool isWatched;
	private bool isLive;

	private protected Reactive() { }

	private readonly Event<EventArgs> watched = new();
	private readonly Event<EventArgs> unwatched = new();

	public event EventHandler? Watched
	{
		add
		{
			if (value is null) { return; }
			watched.AddEventHandler(new(value));
		}

		remove
		{
			if (value is null) { return; }
			watched.RemoveEventHandler(new(value));
		}
	}

	public event EventHandler? Unwatched
	{
		add
		{
			if (value is null) { return; }
			unwatched.AddEventHandler(new(value));
		}

		remove
		{
			if (value is null) { return; }
			unwatched.RemoveEventHandler(new(value));
		}
	}

	private void RaiseWatched()
	{
		watched.Raise(this, EventArgs.Empty);
	}

	private void RaiseUnwatched()
	{
		unwatched.Raise(this, EventArgs.Empty);
	}

	internal Internals GetInternals(LockAction lockAction) => GetInternalsImpl(lockAction);
	private protected virtual Internals GetInternalsImpl(LockAction lockAction) => new(this, lockAction);
	internal class Internals(Reactive reactive, LockAction lockAction) : IDisposable
	{
		private readonly Locks.LockScope scope = Locks.AcquireLock(ref reactive.objLock, lockAction);

		public Reactive Reactive => reactive;

		public void Dispose() => scope.Dispose();

		public IEnumerable<ReactiveLastKnownValue> Sources => reactive.sources.Values;

		public IEnumerable<Reactive> ModifiedSources => reactive.modifiedSources;

		public ReactiveLastKnownValue? TryDequeueModifiedSource()
		{
			if (reactive.modifiedSources.TryDequeue(out var source))
			{
				return reactive.sources[source];
			}
			else
			{
				return null;
			}
		}

		public IEnumerable<Reactive> Receivers => reactive.receivers.Select(x => x.TryGetTarget(out var xx) ? xx : null).OfType<Reactive>();
		public bool HasWatchers => reactive.watchers.Any(x => x.TryGetTarget(out _));

		public bool IsValid => reactive.isValid;

		public int Version => reactive.version;

		public bool IsWatched
		{
			get => reactive.isWatched;
			set => reactive.isWatched = value;
		}

		public bool IsLive => reactive.isLive;

		public void RaiseWatched() => reactive.RaiseWatched();

		public void RaiseUnwatched() => reactive.RaiseUnwatched();

		public void MarkInvalid()
		{
			reactive.isValid = false;
			reactive.version++;
			reactive.Notify();
		}

		public void MarkValid()
		{
			reactive.isValid = true;
		}

		public void ClearSources()
		{
			reactive.sources.Clear();
			reactive.modifiedSources.Clear();
		}

		public void AddSource(ReactiveLastKnownValue source)
		{
			reactive.sources.TryAdd(source.Reactive, source);
		}

		public void RemoveSource(Reactive source)
		{
			if (source.isValid is false)
			{
				reactive.modifiedSources.RemoveAll(source);
			}

			reactive.sources.Remove(source);
		}

		public void AddReceiver(Reactive receiver)
		{
			reactive.receivers.Add(new(receiver));

			if (receiver is WatcherNode || receiver.isWatched)
			{
				reactive.watchers.Add(new(receiver));
			}
		}

		public void RemoveReceiver(Reactive receiver)
		{
			reactive.receivers.Remove(new(receiver));

			if (receiver is WatcherNode || receiver.isWatched)
			{
				reactive.watchers.Remove(new(receiver));
			}
		}

		public void AddWatcher(Reactive watcher)
		{
			reactive.watchers.Add(new(watcher));
		}

		public void RemoveWatcher(Reactive watcher)
		{
			reactive.watchers.Remove(new(watcher));
		}

		internal void MarkSourceModified(Reactive source)
		{
			if (reactive.sources.ContainsKey(source))
			{
				reactive.modifiedSources.Enqueue(source);
			}
			else
			{
				throw new UnreachableException();
			}
		}

		internal void MarkLive()
		{
			reactive.isLive = true;
		}

		public void ClearLive()
		{
			reactive.isLive = false;
		}

		public virtual void Recompute() => throw new NotSupportedException();

		public void Invalidate()
		{
			var stack = new Stack<Internals>();
			stack.Push(this);

			while (stack.TryPop(out var current))
			{
				current.MarkInvalid();

				foreach (var nextNode in Enumerable.Reverse(current.Receivers))
				{
					using var next = nextNode.GetInternals(LockAction._);

					var wasValid = next.IsValid;

					next.MarkSourceModified(current.Reactive);

					if (wasValid is false) { continue; }

					stack.Push(next);
				}
			}
		}

		public void EnsureValid()
		{
			if (IsValid) { return; }

			var stack = new Stack<Internals>();
			stack.Push(this);

			var seen = new HashSet<Internals>();

			while (stack.TryPop(out var current))
			{
				if (seen.Add(current)) // not seen yet
				{
					stack.Push(current);

					foreach (var source in Enumerable.Reverse(current.ModifiedSources))
					{
						using var next = source.GetInternals(LockAction.Recompute);

						if (next.IsValid) { continue; }

						stack.Push(next);
					}
				}
				else // already seen
				{
					current.Recompute();
				}
			}
		}

		public void UpdateIsWatched()
		{
			var stack = new Stack<Internals>();
			stack.Push(this);

			while (stack.TryPop(out var current))
			{
				var prevWatched = current.IsWatched;
				var currWatched = current.HasWatchers;

				if (prevWatched == currWatched) { continue; }

				current.IsWatched = currWatched;

				if (prevWatched is false)
				{
					current.RaiseWatched();
				}

				if (currWatched is false)
				{
					current.RaiseUnwatched();
				}

				foreach (var source in Enumerable.Reverse(current.Sources))
				{
					using var next = source.Reactive.GetInternals(LockAction.Recompute);

					stack.Push(next);

					if (prevWatched is false)
					{
						next.AddWatcher(current.Reactive);
					}

					if (currWatched is false)
					{
						next.RemoveWatcher(current.Reactive);
					}
				}
			}
		}
	}

	internal bool IsCurrent(int lastSeenVersion)
	{
		//Debug.Assert(@lock.IsUpgradeableReadLockHeld);

		return version == lastSeenVersion;
	}

	protected virtual void Notify() { }

	public bool IsLive
	{
		get
		{
			using var readerScope = GetInternals(LockAction._);
			return readerScope.IsLive;
		}
	}

	internal virtual Effect EventEffect(Action raiseEvent) => throw new NotSupportedException();

	public override string ToString() =>
		throw new NotSupportedException();
}

public abstract class Reactive<T>(
	Func<T> valueFunc,
	EqualityComparer<T?> equality)
	:
	Reactive, INotifyPropertyChanged
{
	private readonly EqualityComparer<T?> equality = equality;
	private protected readonly Func<T> valueFunc = valueFunc;
	private bool isComputing;
	private T? value;

	private protected Reactive(Func<T> valueFunc) :
		this(valueFunc, EqualityComparer<T?>.Default)
	{ }

	public T Value
	{
		get
		{
			if (frozen.Value) { throw new FrozenReactiveGraphException(); }

			using var i = GetInternals(LockAction.Recompute);

			i.EnsureValid();
			return i.GetValue();
		}
	}

	internal new Internals GetInternals(LockAction lockAction) => Unsafe.As<Internals>(GetInternalsImpl(lockAction));
	private protected override Reactive.Internals GetInternalsImpl(LockAction lockAction) => new Internals(this, lockAction);
	internal new class Internals(Reactive<T> reactive, LockAction lockAction) : Reactive.Internals(reactive, lockAction)
	{
		public new ReactiveFunc<T> Reactive => Unsafe.As<ReactiveFunc<T>>(base.Reactive);

		public bool ValueIsEqual(T value) =>
			Reactive.exception is null
			&&
			Reactive.equality.Equals(Reactive.value, value);

		public T GetValue()
		{
			try
			{
				Reactive.exception?.Throw();
				var value = Reactive.value!;
				ReactiveValues.Reactive.AddSource(Reactive, Version, value);
				return value;
			}
			catch when (HandleException()) { throw; }
			bool HandleException()
			{
				ReactiveValues.Reactive.AddSource(Reactive, Version);
				return false;
			}
		}

		public void SetValue(T value)
		{
			Reactive.exception = null;
			Reactive.value = value;
			MarkValid();
		}

		public void SetException(Exception exception)
		{
			Reactive.exception = ExceptionDispatchInfo.Capture(exception);
			Reactive.value = default;
			MarkValid();
		}

		public T InvokeValueFunc()
		{
			if (Reactive.isComputing)
			{
				throw new CircularReferenceException();
			}

			Reactive.isComputing = true;

			try
			{
				return Reactive.valueFunc();
			}
			finally
			{
				Reactive.isComputing = false;
			}
		}

		public override void Recompute()
		{
			using (ReactiveValues.Reactive.Untrack())
			{
				if (Sources.Any())
				{
					while (TryDequeueModifiedSource() is { } source)
					{
						if (source.ValueIsCurrent is false)
						{
							goto NotCurrent;
						}
					}

					MarkValid();
					return;
				}
			}

		NotCurrent:;
			using (ReactiveValues.Reactive.Track(this))
			{
				var oldSources = new List<Reactive>(Sources.Count());

				foreach (var sourceWithValue in Sources)
				{
					using var source = sourceWithValue.Reactive.GetInternals(LockAction.Recompute);
					oldSources.Add(source.Reactive);
					source.RemoveReceiver(Reactive);
				}

				ClearSources();
				ClearLive();

				try
				{
					var value = InvokeValueFunc();

					if (ValueIsEqual(value)) { return; }

					SetValue(value);
				}
				catch (Exception exception)
				{
					SetException(exception);
				}
				finally
				{
					MarkValid();

					var newSources = Sources.Select(x => x.Reactive).ToList();

					foreach (var source in oldSources.Except(newSources))
					{
						using var sourceI = source.GetInternals(LockAction.Recompute);
						sourceI.UpdateIsWatched();
					}

					foreach (var source in newSources.Except(oldSources))
					{
						using var sourceI = source.GetInternals(LockAction.Recompute);
						sourceI.UpdateIsWatched();
					}
				}
			}
		}
	}

	internal bool IsCurrent(int lastSeenVersion, T lastSeenValue)
	{
		using var i = GetInternals(LockAction.Recompute);

		if (i.Version == lastSeenVersion) { return true; }

		if (i.IsValid is false)
		{
			throw new UnreachableException();
		}

		return exception is null
			&& equality.Equals(value, lastSeenValue);
	}

	internal override Effect EventEffect(Action raiseEvent)
	{
		Action @event = () => { };

		var effect = new Effect(() =>
		{
			_ = Value;

			using (Reactive.Untrack())
			{
				@event();
			}
		});

		effect.Run();
		@event = raiseEvent;

		return effect;
	}

	private readonly ConcurrentDictionary<PropertyChangedEventHandler, (Watcher watcher, Effect effect)>
		propertyChanged = new();

	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			if (value is null) { return; }

			_ = propertyChanged.AddOrUpdate(
				value,
				value =>
				{
					var watcher = EventHandlerWatcher.Current;

					var args = new PropertyChangedEventArgs(nameof(Value));

					var effect = Reactive.EventEffect(this, () => value(this, args));

					watcher.Watch(effect);

					return (watcher, effect);
				},
				(value, _) => throw new InvalidOperationException()
				);
		}

		remove
		{
			if (value is null) { return; }

			if (propertyChanged.TryRemove(value, out var result) is false)
			{
				throw new InvalidOperationException();
			}

			result.watcher.Unwatch(result.effect);
		}
	}
}
