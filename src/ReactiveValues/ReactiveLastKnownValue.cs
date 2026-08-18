using System.Runtime.CompilerServices;

namespace ReactiveValues;

internal class ReactiveLastKnownValue(Reactive reactive, int lastSeenVersion)
{
	public static readonly EqualityComparer<ReactiveLastKnownValue> Equality =
		EqualityComparer<ReactiveLastKnownValue>
			.Create(
			(x, y) => ReferenceEquals(x?.Reactive, y?.Reactive),
			x => x.Reactive.GetHashCode());

	protected readonly int lastSeenVersion = lastSeenVersion;

	public Reactive Reactive => reactive;

	public virtual bool ValueIsCurrent_EnsureValid() => reactive.IsCurrent_EnsureValid(lastSeenVersion);
}

internal sealed class ReactiveLastKnownValue<T>(Reactive<T> reactive, int lastSeenVersion, T lastSeenValue)
	: ReactiveLastKnownValue(reactive, lastSeenVersion)
{
	public new Reactive<T> Reactive => Unsafe.As<Reactive<T>>(base.Reactive);

	public override bool ValueIsCurrent_EnsureValid() => Reactive.IsCurrent_EnsureValid(lastSeenVersion, lastSeenValue);
}
