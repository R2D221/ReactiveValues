# ReactiveValues

ReactiveValues is a C# library to create a system of base and computed values that notify when they're updated (mainly useful in UIs, like INotifyPropertyChanged), largely based on [TC39's (JavaScript) signals proposal](https://github.com/tc39/proposal-signals).

This repository is a work in progress. You're free to explore all projects in this repo, but the only officially released ones are the ones with NuGet packages.

## ReactiveValues [![](https://img.shields.io/nuget/vpre/ReactiveValues)](https://www.nuget.org/packages/ReactiveValues/)

Primitives for creating reactive values.

```csharp
var counter = new ReactiveValue<int>(1);
var isEven = new ReactiveFunc<bool>(() => counter.Value % 2 == 0);
var effect = new Effect(() => label.Text = isEven.Value ? "Even" : "Odd");

var watcher = Watcher.Current;
watcher.Watch(effect);
```

## ReactiveValues.DataTypes [![](https://img.shields.io/nuget/vpre/ReactiveValues.DataTypes)](https://www.nuget.org/packages/ReactiveValues.DataTypes/)

Base class that implements INotifyPropertyChanged automatically.

```csharp
public class PersonViewModel : ReactiveObject
{
	public required string GivenName
	{
		get => Property(() => GivenName).GetRequired();
		set => Property(() => GivenName).Set(value);
	}

	public string? FamilyName
	{
		get => Property(() => FamilyName).Get();
		set => Property(() => FamilyName).Set(value);
	}

	public string FullName =>
		Property(() => FullName)
		.Computed(() => $"{GivenName} {FamilyName}");
}
```

## License

MIT
