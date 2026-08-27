namespace ReactiveValues.DataTypes;

public abstract class ReactiveProperty
{
	private protected ReactiveProperty() { }

	internal abstract Reactive Reactive { get; }
}

public sealed class ReactiveProperty<T>(ReactiveObject owner, string name) : ReactiveProperty
{
	private Reactive<T>? reactive;

	internal override Reactive Reactive => reactive ?? throw new ArgumentNullException(nameof(reactive));

	public void Set(T value)
	{
		if (reactive is null)
		{
			reactive = new ReactiveValue<T>(value);
		}
		else
		{
			((ReactiveValue<T>)reactive).Value = value;
		}
	}

	public T Get(Func<T> initialValue)
	{
		reactive ??= new ReactiveValue<T>(initialValue());
		owner.HookPropertyChanged(name, reactive);
		return reactive.Value;
	}

	public T? Get() => Get(() => default!);

	public T GetRequired() => Get(() => throw new InvalidOperationException($"Property '{name}' was not set."));

	public T Computed(Func<T> valueFunc)
	{
		reactive ??= new ReactiveFunc<T>(valueFunc);
		owner.HookPropertyChanged(name, reactive);
		return reactive.Value;
	}
}
