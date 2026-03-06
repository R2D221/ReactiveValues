namespace ReactiveValues.Helpers;

public sealed class Invalidator
{
	private readonly struct Invalid : IEquatable<Invalid>
	{
		public bool Equals(Invalid other) => false;

		public override bool Equals(object? obj) => false;

		public override int GetHashCode() => throw new NotSupportedException();
	}

	private readonly ReactiveValue<Invalid> reactive = new(default);

	internal Invalidator() { }

	internal void Register() => _ = reactive.Value;

	public void Invalidate() => reactive.Value = default;
}
