using ReactiveValues.DataTypes;
using System.Windows.Input;

namespace ReactiveSamples.Common;

public sealed class InputControlsViewModel : ReactiveObject
{
	public bool Checked
	{
		get => Property(() => Checked).Get();
		set => Property(() => Checked).Set(value);
	}

	public double Number
	{
		get => Property(() => Number).Get();
		set => Property(() => Number).Set(value);
	}

	public string Text
	{
		get => Property(() => Text).Get(() => "");
		set => Property(() => Text).Set(value);
	}

	public DateTime? Date
	{
		get => Property(() => Date).Get();
		set => Property(() => Date).Set(value);
	}

	public IReadOnlyList<string> Items =>
		Property(() => Items)
		.Computed(() => ["Alfa", "Bravo", "Charlie"]);

	public string? Item
	{
		get => Property(() => Item).Get();
		set => Property(() => Item).Set(value);
	}

	public bool CanSubmit =>
		Property(() => CanSubmit)
		.Computed(() =>
		Checked is true
		&& Number > 0
		&& Text.Length > 0
		&& Date is not null
		&& Item is not null
		);

	public string StatusMessage
	{
		get => Property(() => StatusMessage).Get(() => "");
		set => Property(() => StatusMessage).Set(value);
	}

	public ICommand SubmitCommand => field ??= new ReactiveCommand(
		() => CanSubmit,
		() =>
		{
			StatusMessage = $"{DateTime.Now} - Submitted!";
		});

	public ICommand ClearCommand => field ??= new ReactiveCommand(
		() => true,
		() =>
		{
			Checked = false;
			Number = 0;
			Text = "";
			Date = null;
			Item = null;

			StatusMessage = "";
		});
}
