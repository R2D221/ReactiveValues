using ReactiveValues.DataTypes;
using System.Windows.Input;

namespace ReactiveSamples.Common;

public sealed class InputControlsViewModel : ReactiveObject
{
	public bool Checked
	{
		get => Get(() => Checked);
		set => Set(() => Checked, value);
	}

	public double Number
	{
		get => Get(() => Number);
		set => Set(() => Number, value);
	}

	public string Text
	{
		get => Get(() => Text, () => "");
		set => Set(() => Text, value);
	}

	public DateTime? Date
	{
		get => Get(() => Date);
		set => Set(() => Date, value);
	}

	public IReadOnlyList<string> Items => Computed(() => Items,
		() => ["Alfa", "Bravo", "Charlie"]);

	public string? Item
	{
		get => Get(() => Item);
		set => Set(() => Item, value);
	}

	public bool CanSubmit => Computed(() => CanSubmit,
		() =>
		Checked is true
		&& Number > 0
		&& Text.Length > 0
		&& Date is not null
		&& Item is not null
		);

	public string StatusMessage
	{
		get => Get(() => StatusMessage, () => "");
		set => Set(() => StatusMessage, value);
	}

	public ICommand SubmitCommand => field ??= Command(
		_ => CanSubmit,
		_ =>
		{
			StatusMessage = $"{DateTime.Now} - Submitted!";
		});

	public ICommand ClearCommand => field ??= Command(
		_ => true,
		_ =>
		{
			Checked = false;
			Number = 0;
			Text = "";
			Date = null;
			Item = null;

			StatusMessage = "";
		});
}
