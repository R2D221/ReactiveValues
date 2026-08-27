using ReactiveValues.DataTypes;
using System.Windows.Input;

namespace ReactiveSamples.Common;

public sealed class ListsViewModel : ReactiveObject
{
	public string NewItem
	{
		get => Property(() => NewItem).Get(() => "");
		set => Property(() => NewItem).Set(value);
	}

	public ReactiveList<string> LeftItems =>
		Property(() => LeftItems)
		.Get(() => []);

	public int LeftIndex
	{
		get => Property(() => LeftIndex).Get(() => -1);
		set => Property(() => LeftIndex).Set(value);
	}

	public ReactiveList<string> RightItems =>
		Property(() => RightItems)
		.Get(() => []);

	public int RightIndex
	{
		get => Property(() => RightIndex).Get(() => -1);
		set => Property(() => RightIndex).Set(value);
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
