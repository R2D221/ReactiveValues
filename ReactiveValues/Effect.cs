namespace ReactiveValues;

public sealed class Effect
{
	private readonly EffectNode node;

	public Effect(Action action)
	{
		node = new(action, this);
	}

	internal EffectNode Node => node;

	public bool IsLive =>
		node.IsLive;

	public void Run() =>
		_ = node.Value;
}

internal sealed class EffectNode(Action action, Effect effect)
	:
	Reactive<ValueTuple>(() => { action(); return default; })
{
	public Effect Effect => effect;
}