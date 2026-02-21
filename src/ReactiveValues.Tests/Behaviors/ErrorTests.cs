using ReactiveValues;
using System.Diagnostics;

namespace Signals.Tests.Behaviors;

[TestClass]
public sealed class ErrorTests
{
	private sealed class FirstException : Exception;
	private sealed class SecondException : Exception;
	private sealed class EqualsException : Exception;

	private static object? ThrowException(string type)
	{
		switch (type)
		{
		case "first": throw new FirstException();
		case "second": throw new SecondException();

		default: throw new UnreachableException();
		}
	}

	[TestMethod(DisplayName = "Errors are cached by computed signals")]
	public void Test1()
	{
		var s = new ReactiveValue<string>("first");
		var n = 0;
		var c = new ReactiveFunc<object?>(() =>
		{
			n++;
			return ThrowException(s.Value);
		});
		var n2 = 0;
		var c2 = new ReactiveFunc<object?>(() =>
		{
			n2++;
			return c.Value;
		});
		Assert(That(n) == 0);

		Assert(That(() => _ = c.Value).Throws<FirstException>());
		Assert(That(() => _ = c2.Value).Throws<FirstException>());
		Assert(That(n) == 1);
		Assert(That(n2) == 1);

		Assert(That(() => _ = c.Value).Throws<FirstException>());
		Assert(That(() => _ = c2.Value).Throws<FirstException>());
		Assert(That(n) == 1);
		Assert(That(n2) == 1);

		s.Value = ("second");
		Assert(That(() => _ = c.Value).Throws<SecondException>());
		Assert(That(() => _ = c2.Value).Throws<SecondException>());
		Assert(That(n) == 2);
		Assert(That(n2) == 2);

		// Doesn't retrigger on Setting state to the same value
		s.Value = ("second");
		Assert(That(n) == 2);
	}

	[TestMethod(DisplayName = "Errors are cached by computed signals when watched")]
	public void Test2()
	{
		var s = new ReactiveValue<string>("first");
		var n = 0;
		var c = new ReactiveFunc<object?>(() =>
		{
			n++;
			return ThrowException(s.Value);
		});

		var w = new TestWatcher(() => { });

		// We deviate from the spec here...
		// Should watcher.watch() evaluate the signal immediately?

		Assert(That(n) ==  0);

		Assert(That(() => w.Watch(new Effect(() => _ = c.Value))).Throws<FirstException>());
		Assert(That(n) ==  1);

		Assert(That(() => _ = c.Value).Throws<FirstException>());
		Assert(That(n) ==  1);

		s.Value = ("second");
		Assert(That(() => _ = c.Value).Throws<SecondException>());
		Assert(That(n) ==  2);

		s.Value = ("second");
		Assert(That(n) ==  2);
	}

	[TestMethod(DisplayName = "Errors are cached by computed signals when equals throws")]
	public void Test3()
	{
		var s = new ReactiveValue<int>(0);
		var cSpy = ProxyFunc.For(() => s.Value);
		var c = new ReactiveFunc<int>(cSpy.Invoke,
			EqualityComparer<int>.Create((_, _) =>
			{
				throw new EqualsException();
			}));

		_ = c.Value;
		s.Value = (1);

		// Error is cached; c throws again without needing to rerun.
		Assert(That(() => _ = c.Value).Throws<EqualsException>());
		Assert(That(cSpy.TimesCalled) == 2);
		Assert(That(() => _ = c.Value).Throws<EqualsException>());
		Assert(That(cSpy.TimesCalled) == 2);
	}
}
