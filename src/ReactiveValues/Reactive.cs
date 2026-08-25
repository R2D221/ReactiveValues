using ReactiveValues.Exceptions;
using ReactiveValues.Helpers;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace ReactiveValues;

public abstract partial class Reactive
{
	private protected static readonly ExceptionDispatchInfo defaultException =
		ExceptionDispatchInfo.Capture(new UnreachableException());

	private protected static readonly ThreadLocal<bool> frozen = new();

	public static bool IsWatched(Reactive reactive)
	{
		using (reactive.@lock.ReadLockScope())
		{
			return reactive.isWatched;
		}
	}

	#region Connections

	internal readonly Dictionary<Reactive, ReactiveLastKnownValue> sources = [];

	private protected readonly Queue<Reactive> modifiedSources = [];

	private protected readonly HashSet<WeakReference<Reactive>> receivers =
		new(comparer: WeakEqualityComparer<Reactive>.Instance);

	private protected readonly HashSet<WeakReference<Reactive>> watchers =
		new(comparer: WeakEqualityComparer<Reactive>.Instance);

	internal void AddSource(ReactiveLastKnownValue source)
	{
		sources.TryAdd(source.Reactive, source);
	}

	internal void RemoveSource(Reactive source)
	{
		if (source.isValid is false)
		{
			modifiedSources.RemoveAll(source);
		}

		sources.Remove(source);
	}

	internal void ClearSources()
	{
		sources.Clear();
		modifiedSources.Clear();
	}

	internal void MarkSourceModified(Reactive source)
	{
		if (sources.ContainsKey(source))
		{
			modifiedSources.Enqueue(source);
		}
		else
		{
			throw new UnreachableException();
		}
	}

	internal ReactiveLastKnownValue? TryDequeueModifiedSource()
	{
		if (modifiedSources.TryDequeue(out var source))
		{
			return sources[source];
		}
		else
		{
			return null;
		}
	}

	internal IEnumerable<Reactive> EnumerateLiveReceivers() =>
		receivers
		.Select(x => x.TryGetTarget(out var xx) ? xx : null)
		.OfType<Reactive>();

	internal void AddReceiver(Reactive receiver)
	{
		receivers.Add(new(receiver));

		if (receiver is WatcherNode || receiver.isWatched)
		{
			watchers.Add(new(receiver));
		}
	}

	internal void RemoveReceiver(Reactive receiver)
	{
		receivers.Remove(new(receiver));

		if (receiver is WatcherNode || receiver.isWatched)
		{
			watchers.Remove(new(receiver));
		}
	}

	internal void AddWatcher(Reactive watcher)
	{
		watchers.Add(new(watcher));
	}

	internal void RemoveWatcher(Reactive watcher)
	{
		watchers.Remove(new(watcher));
	}

	#endregion

	internal readonly ReaderWriterLockSlim @lock = new();
	internal bool isValid;
	private protected int version;
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

	internal void MarkInvalid()
	{
		isValid = false;
		version++;
		Notify();
	}

	internal void MarkValid()
	{
		isValid = true;
	}

	internal void MarkLive()
	{
		isLive = true;
	}

	internal void ClearLive()
	{
		isLive = false;
	}

	internal void Invalidate()
	{
		var stack = new Stack<Reactive>();
		stack.Push(this);

		while (stack.TryPop(out var current))
		{
			IReadOnlyList<Reactive> currentReceivers;

			using (current.@lock.ReadLockScope())
			{
				currentReceivers = [.. Enumerable.Reverse(current.EnumerateLiveReceivers())];
			}

			foreach (var next in currentReceivers)
			{
				using (next.@lock.UpgradeableReadLockScope())
				{
					var wasValid = next.isValid;

					if (wasValid is false)
					{
						using (next.@lock.WriteLockScope())
						{
							next.MarkSourceModified(current);
						}

						continue;
					}

					using (next.@lock.WriteLockScope())
					{
						next.MarkSourceModified(current);
						next.MarkInvalid();
					}

					stack.Push(next);
				}
			}
		}
	}

	internal void UpdateIsWatched()
	{
		var stack = new Stack<Reactive>();
		stack.Push(this);

		while (stack.TryPop(out var current))
		{
			IReadOnlyList<Reactive> currentSources;
			bool prevWatched;
			bool currWatched;

			using (current.@lock.UpgradeableReadLockScope())
			{
				prevWatched = current.isWatched;
				currWatched = current.watchers.Any(x => x.TryGetTarget(out _));

				if (prevWatched == currWatched) { continue; }

				using (current.@lock.WriteLockScope())
				{
					current.isWatched = currWatched;
				}

				currentSources = [.. Enumerable.Reverse(current.sources.Keys)];
			}

			if (prevWatched is false)
			{
				current.RaiseWatched();
			}

			if (currWatched is false)
			{
				current.RaiseUnwatched();
			}

			foreach (var next in currentSources)
			{
				stack.Push(next);

				if (prevWatched is false)
				{
					using (next.@lock.WriteLockScope())
					{
						next.AddWatcher(current);
					}
				}

				if (currWatched is false)
				{
					using (next.@lock.WriteLockScope())
					{
						next.RemoveWatcher(current);
					}
				}
			}
		}
	}

	internal bool IsCurrent_EnsureValid(int lastSeenVersion)
	{
		using (@lock.ReadLockScope())
		{
			return version == lastSeenVersion;
		}
	}

	protected virtual void Notify() { }

	public bool IsLive
	{
		get
		{
			using (@lock.ReadLockScope())
			{
				return isLive;
			}
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
	private T? value;

	private protected Reactive(Func<T> valueFunc) :
		this(valueFunc, EqualityComparer<T?>.Default)
	{ }

	public T Value
	{
		get
		{
			Debugger.NotifyOfCrossThreadDependency();
			if (frozen.Value) { throw new FrozenReactiveGraphException(); }

			IReadOnlyCollection<Reactive> sourcesUpdateIsWatched = [];
			try
			{
				using (@lock.UpgradeableReadLockScope(() => new CircularReferenceException()))
				{
					if (!isValid)
					{
						Recompute(out sourcesUpdateIsWatched);
					}

					return GetValue();
				}
			}
			finally
			{
				foreach (var source in sourcesUpdateIsWatched)
				{
					source.UpdateIsWatched();
				}
			}
		}
	}

	internal bool ValueIsEqual(T value) =>
		exception is null
		&&
		equality.Equals(this.value, value);

	internal T GetValue()
	{
		try
		{
			exception?.Throw();
			var value = this.value!;
			Reactive.AddSource(this, version, value);
			return value;
		}
		catch when (HandleException()) { throw; }
		bool HandleException()
		{
			Reactive.AddSource(this, version);
			return false;
		}
	}

	internal void SetValue(T value)
	{
		exception = null;
		this.value = value;
		MarkValid();
	}

	internal void SetException(Exception exception)
	{
		this.exception = ExceptionDispatchInfo.Capture(exception);
		value = default;
		MarkValid();
	}

	internal void Recompute(out IReadOnlyCollection<Reactive> sourcesUpdateIsWatched)
	{
		using (@lock.WriteLockScope())
		{
			using (Reactive.Untrack())
			{
				if (sources.Count != 0)
				{
					while (TryDequeueModifiedSource() is { } source)
					{
						if (source.ValueIsCurrent_EnsureValid() is false)
						{
							goto NotCurrent;
						}
					}

					MarkValid();
					sourcesUpdateIsWatched = [];
					return;
				}
			}

		NotCurrent:;
			using (Reactive.Track(this))
			{
				var oldSources = new List<Reactive>(capacity: sources.Count);

				foreach (var source in sources.Keys)
				{
					oldSources.Add(source);

					using (source.@lock.WriteLockScope())
					{
						source.RemoveReceiver(this);
					}
				}

				ClearSources();
				ClearLive();

				try
				{
					var value = valueFunc();

					if (ValueIsEqual(value))
					{
						MarkValid();
					}
					else
					{
						SetValue(value);
					}
				}
				catch (Exception exception)
				{
					SetException(exception);
				}
				finally
				{
					var newSources = sources.Keys;

					sourcesUpdateIsWatched = [
						.. oldSources.Except(newSources),
						.. newSources.Except(oldSources)];
				}
			}
		}
	}

	internal bool IsCurrent_EnsureValid(int lastSeenVersion, T lastSeenValue)
	{
		IReadOnlyCollection<Reactive> sourcesUpdateIsWatched = [];
		try
		{
			using (@lock.UpgradeableReadLockScope())
			{
				if (version == lastSeenVersion) { return true; }

				if (isValid is false)
				{
					Recompute(out sourcesUpdateIsWatched);
				}

				Debug.Assert(isValid);

				return exception is null
					&& equality.Equals(value, lastSeenValue);
			}
		}
		finally
		{
			foreach (var source in sourcesUpdateIsWatched)
			{
				source.UpdateIsWatched();
			}
		}
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
