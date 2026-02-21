namespace ReactiveValues;

public sealed class ReactiveFunc<T> : Reactive<T>
{
	public ReactiveFunc(Func<T> valueFunc)
		: base(valueFunc) { }

	public ReactiveFunc(Func<T> valueFunc, EqualityComparer<T?> equality)
		: base(valueFunc, equality) { }
}
