using ReactiveValues.Exceptions;
using System.Diagnostics;

namespace ReactiveValues;

public sealed class ReactiveValue<T> : Reactive<T>
{
	public ReactiveValue(T value) : this(value, EqualityComparer<T?>.Default) { }

	public ReactiveValue(T value, EqualityComparer<T?> equality) : base(() => throw new UnreachableException(), equality)
	{
		using var i = GetInternals(LockAction._);

		i.SetValue(value);
		i.MarkLive();
	}

	public new T Value
	{
		get => base.Value;

		set
		{
			if (frozen.Value) { throw new FrozenReactiveGraphException(); }

			using (Reactive.Defer())
			{
				using var i = GetInternals(LockAction._);

				if (i.ValueIsEqual(value)) { return; }

				i.Invalidate();
				i.SetValue(value);
			}
		}
	}
}
