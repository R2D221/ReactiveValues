using ReactiveValues.DataTypes;

namespace ReactiveSamples.Common;

public sealed class ViewModel : ReactiveObject
{
	public string GivenName
	{
		get => Get(() => GivenName, () => "Arturo");
		set => Set(() => GivenName, value);
	}

	public string FamilyName
	{
		get => Get(() => FamilyName, () => "Torres");
		set => Set(() => FamilyName, value);
	}

	public string FullName => Computed(() => FullName,
		() => $"{GivenName} {FamilyName}");

	public int Age
	{
		get => Get(() => Age, () => 0);
		set => Set(() => Age, value);
	}

	public ReactiveList<string> Items
	{
		get => Get(() => Items, () => ["A", "B", "C", "D"]);
		set => Set(() => Items, value);
	}
}
