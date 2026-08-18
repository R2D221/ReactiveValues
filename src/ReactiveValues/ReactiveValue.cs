using ReactiveValues.Exceptions;
using System.Diagnostics;

namespace ReactiveValues;

public sealed class ReactiveValue<T> : Reactive<T>
{
	public ReactiveValue(T value) : this(value, EqualityComparer<T?>.Default) { }

	public ReactiveValue(T value, EqualityComparer<T?> equality) : base(() => throw new UnreachableException(), equality)
	{
		using (@lock.WriteLockScope())
		{
			SetValue(value);
			MarkLive();
		}
	}

	public new T Value
	{
		get => base.Value;

		set
		{
			if (frozen.Value) { throw new FrozenReactiveGraphException(); }

			using (Reactive.Defer())
			{
				using (@lock.UpgradeableReadLockScope())
				{
					if (ValueIsEqual(value))
					{
						return;
					}

					using (@lock.WriteLockScope())
					{
						MarkInvalid();
						SetValue(value);
					}
				}

				Invalidate();
			}
		}
	}
}
