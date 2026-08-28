using ReactiveValues.DataTypes;

namespace ReactiveSamples.Common;

public sealed class ViewModel : ReactiveObject
{
	public string GivenName
	{
		get => Property(() => GivenName).Get(() => "Arturo");
		set => Property(() => GivenName).Set(value);
	}

	public string FamilyName
	{
		get => Property(() => FamilyName).Get(() => "Torres");
		set => Property(() => FamilyName).Set(value);
	}

	public string FullName =>
		Property(() => FullName)
		.Computed(() => $"{GivenName} {FamilyName}");

	public int Age
	{
		get => Property(() => Age).Get(() => 0);
		set => Property(() => Age).Set(value);
	}

	public ReactiveList<string> Items
	{
		get => Property(() => Items).Get(() => ["A", "B", "C", "D"]);
		set => Property(() => Items).Set(value);
	}
}
