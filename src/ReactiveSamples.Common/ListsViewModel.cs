using ReactiveValues.DataTypes;
using System.Windows.Input;

namespace ReactiveSamples.Common;

public sealed class ListsViewModel : ReactiveObject
{
	public string NewItem
	{
		get => Get(() => NewItem, () => "");
		set => Set(() => NewItem, value);
	}

	public ReactiveList<string> LeftItems => Get(() => LeftItems, () => []);

	public int LeftIndex
	{
		get => Get(() => LeftIndex, () => -1);
		set => Set(() => LeftIndex, value);
	}

	public ReactiveList<string> RightItems => Get(() => RightItems, () => []);

	public int RightIndex
	{
		get => Get(() => RightIndex, () => -1);
		set => Set(() => RightIndex, value);
	}

	public ICommand AddItemCommand => field ??= Command(
		_ => NewItem.Length > 0,
		_ =>
		{
			LeftItems.Add(NewItem);
			NewItem = "";
		});

	public ICommand MoveRightCommand => field ??= Command(
		_ => LeftIndex > -1,
		_ =>
		{
			var leftItem = LeftItems.GetAt(LeftIndex);

			LeftItems.Remove(leftItem);
			RightItems.AddLast(leftItem);
		});

	public ICommand MoveLeftCommand => field ??= Command(
		_ => RightIndex > -1,
		_ =>
		{
			var rightItem = RightItems.GetAt(RightIndex);

			RightItems.Remove(rightItem);
			LeftItems.AddLast(rightItem);
		});
}
