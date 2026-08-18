using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using WindowsFormsTimer = System.Windows.Forms.Timer;

namespace ReactiveValues.WindowsForms;

public sealed partial class WindowsFormsWatcher : Watcher
{
	//// We use a WinForms timer because we want to run the watcher callbacks as low
	//// priority. In WinForms, SyncContext.Post registers the callback as high priority,
	//// and can make the UI unresponsive if sent too frequently. The WinForms timer tho
	//// registers its tick event as a WM_TIMER message which has the lowest priority.
	//// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessage#remarks
	//private static readonly ThreadLocal<WindowsFormsTimer> threadLocalTimer = new(InitializeTimer);

	//private static readonly ConcurrentDictionary<WindowsFormsTimer, ConcurrentDictionary<WindowsFormsWatcher, ValueTuple>> sets = new();

	private static readonly ConditionalWeakTable<Control, WindowsFormsWatcher> watchers = new();

	public static WindowsFormsWatcher ForControl(Control control) => watchers.GetValue(control, control => new(control));

	//private static WindowsFormsTimer InitializeTimer()
	//{
	//	var timer = new WindowsFormsTimer();
	//	timer.Tick += (s, __) =>
	//	{
	//		var timer = (WindowsFormsTimer)(s ?? throw new UnreachableException());
	//		timer.Enabled = false;

	//		ConcurrentDictionary<WindowsFormsWatcher, ValueTuple> set;
	//		while (true)
	//		{
	//			set = sets[timer];
	//			if (sets.TryUpdate(timer, [], set)) { break; }
	//		}

	//		foreach (var watcher in set.Keys)
	//		{
	//			watcher.UpdatePending();
	//		}
	//	};
	//	timer.Interval = 1;
	//	return timer;
	//}

	private readonly Control control;
	//private readonly WindowsFormsTimer timer;

	private const int FALSE = 0;
	private const int TRUE = 1;

	private int pending = FALSE;

	private WindowsFormsWatcher(Control control)
	{
		this.control = control;

		//if (control.InvokeRequired)
		//{
		//	throw new InvalidOperationException();
		//}

		//timer = threadLocalTimer.Value ?? throw new UnreachableException();
	}

	protected override void OnNotified()
	{
		if (Interlocked.Exchange(ref pending, TRUE) is FALSE)
		{
			if (control.IsHandleCreated is false)
			{
				control.HandleCreated += (_, _) => RunPending();
			}
			else
			{
				_ = control.BeginInvoke(RunPending);
			}
		}

		//if (sets.GetOrAdd(timer, _ => []).TryAdd(this, default))
		//{
		//	if (timer.Enabled is false)
		//	{
		//		// The timer needs to be enabled from the UI thread.

		//		if (control.IsHandleCreated is false)
		//		{
		//			control.HandleCreated += (_, _) => timer.Enabled = true;
		//		}
		//		else if (control.InvokeRequired)
		//		{
		//			_ = control.BeginInvoke(() => timer.Enabled = true);
		//		}
		//		else
		//		{
		//			timer.Enabled = true;
		//		}
		//	}
		//}
	}

	private void RunPending()
	{
		pending = FALSE;

		using var enumerator = GetPending().GetEnumerator();

		if (enumerator.MoveNext() is false)
		{
			return;
		}

		_ = PInvoke.SendMessage((HWND)control.Handle, PInvoke.WM_SETREDRAW, (nuint)BOOL.FALSE.Value, 0);
		control.SuspendLayout();

		try
		{
			do
			{
				enumerator.Current.Run();
			}
			while (enumerator.MoveNext());
		}
		finally
		{
			control.ResumeLayout();
			_ = PInvoke.SendMessage((HWND)control.Handle, PInvoke.WM_SETREDRAW, (nuint)BOOL.TRUE.Value, 0);
			control.Refresh();
		}
	}
}
