using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using AssertFailedException = Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException;

namespace Signals.Tests;

internal static class Assertions
{
	public static void Assert(Assertion assertion) => assertion.Validate();

	public static AssertValue<T> That<T>(T value, [CallerArgumentExpression(nameof(value))] string? expression = null) =>
		new(value, expression ?? throw new ArgumentNullException(nameof(expression)));

	public static AssertAction That(Action action, [CallerArgumentExpression(nameof(action))] string? expression = null) =>
		new(action, expression ?? throw new ArgumentNullException(nameof(expression)));

	public static Assertion SequenceEqual<T, TEnumerable>(this AssertValue<TEnumerable> assertValue, IEnumerable<T> expected)
		where TEnumerable : IEnumerable<T>
		=>
		new SequenceEqualAssertion<T>(assertValue.value, assertValue.expression, expected);
}

internal sealed class AssertAction(Action action, string expression)
{
	public Assertion Throws<TException>()
		where TException : Exception =>
		new ExceptionAssertion<TException>(action, expression);
}

internal sealed class AssertValue<T>(T value, string expression)
{
	internal readonly T value = value;
	internal readonly string expression = expression;

	public override bool Equals(object? obj) => throw new NotSupportedException();
	public override int GetHashCode() => throw new NotSupportedException();
	public static Assertion operator ==(AssertValue<T> @this, T expected) => new EqualityAssertion<T>(@this.value, @this.expression, expected);
	public static Assertion operator !=(AssertValue<T> @this, T expected) => new InequalityAssertion<T>(@this.value, @this.expression, expected);
}

internal static class AssertExtensions
{
	extension<T>(T)
		where T : IComparable<T>
	{
		public static Assertion operator <(AssertValue<T> @this, T expected) => new LessThanAssertion<T>(@this.value, @this.expression, expected);
		public static Assertion operator <=(AssertValue<T> @this, T expected) => new LessThanOrEqualAssertion<T>(@this.value, @this.expression, expected);
		public static Assertion operator >(AssertValue<T> @this, T expected) => new GreaterThanAssertion<T>(@this.value, @this.expression, expected);
		public static Assertion operator >=(AssertValue<T> @this, T expected) => new GreaterThanOrEqualAssertion<T>(@this.value, @this.expression, expected);
	}
}

internal sealed class EqualityAssertion<T>(T value, string expression, T expected) : Assertion
{
	private static readonly EqualityComparer<T> equality = EqualityComparer<T>.Default;

	private readonly T value = value;
	private readonly string expression = expression;
	private readonly T expected = expected;

	public override void Validate()
	{
		if (equality.Equals(value, expected) is false)
		{
			throw new AssertFailedException(
				$"""
				Assertion error in '{expression}'.
				- Expected: {expected}
				- Actual:   {value}
				""");
		}
	}
}

internal sealed class InequalityAssertion<T>(T value, string expression, T expected) : Assertion
{
	private static readonly EqualityComparer<T> equality = EqualityComparer<T>.Default;

	private readonly T value = value;
	private readonly string expression = expression;
	private readonly T expected = expected;

	public override void Validate()
	{
		if (equality.Equals(value, expected))
		{
			throw new AssertFailedException(
				$"""
				Assertion error in '{expression}'.
				- Expected: not {expected}
				- Actual:   {value}
				""");
		}
	}
}

internal sealed class LessThanAssertion<T>(T value, string expression, T expected) : Assertion
	where T : IComparable<T>
{
	private static readonly Comparer<T> comparer = Comparer<T>.Default;

	private readonly T value = value;
	private readonly string expression = expression;
	private readonly T expected = expected;

	public override void Validate()
	{
		if ((comparer.Compare(value, expected) < 0) is false)
		{
			throw new AssertFailedException(
				$"""
				Assertion error in '{expression}'.
				- Expected: < {expected}
				- Actual:   {value}
				""");
		}
	}
}

internal sealed class LessThanOrEqualAssertion<T>(T value, string expression, T expected) : Assertion
	where T : IComparable<T>
{
	private static readonly Comparer<T> comparer = Comparer<T>.Default;

	private readonly T value = value;
	private readonly string expression = expression;
	private readonly T expected = expected;

	public override void Validate()
	{
		if ((comparer.Compare(value, expected) <= 0) is false)
		{
			throw new AssertFailedException(
				$"""
				Assertion error in '{expression}'.
				- Expected: <= {expected}
				- Actual:   {value}
				""");
		}
	}
}

internal sealed class GreaterThanAssertion<T>(T value, string expression, T expected) : Assertion
	where T : IComparable<T>
{
	private static readonly Comparer<T> comparer = Comparer<T>.Default;

	private readonly T value = value;
	private readonly string expression = expression;
	private readonly T expected = expected;

	public override void Validate()
	{
		if ((comparer.Compare(value, expected) > 0) is false)
		{
			throw new AssertFailedException(
				$"""
				Assertion error in '{expression}'.
				- Expected: > {expected}
				- Actual:   {value}
				""");
		}
	}
}

internal sealed class GreaterThanOrEqualAssertion<T>(T value, string expression, T expected) : Assertion
	where T : IComparable<T>
{
	private static readonly Comparer<T> comparer = Comparer<T>.Default;

	private readonly T value = value;
	private readonly string expression = expression;
	private readonly T expected = expected;

	public override void Validate()
	{
		if ((comparer.Compare(value, expected) >= 0) is false)
		{
			throw new AssertFailedException(
				$"""
				Assertion error in '{expression}'.
				- Expected: >= {expected}
				- Actual:   {value}
				""");
		}
	}
}

internal sealed class ExceptionAssertion<TException>(Action action, string expression) : Assertion
	where TException : Exception
{
	public override void Validate()
	{
		try
		{
			action();
		}
		catch (TException)
		{
			return;
		}

		throw new AssertFailedException(
			$"""
			Assertion error in '{expression}'.
			- Expected: Exception of type {typeof(TException).Name}
			- Actual:   no Exception
			""");
	}
}

internal sealed class SequenceEqualAssertion<T>(IEnumerable<T> value, string expression, IEnumerable<T> expected) : Assertion
{
	public override void Validate()
	{
		var cached = (value: (ImmutableArray<T>)[..value], expected: (ImmutableArray<T>)[..expected]);

		if (cached.value.SequenceEqual(cached.expected) is false)
		{
			throw new AssertFailedException(
				$"""
				Assertion error in '{expression}'.
				- Expected: [{string.Join(",", expected)}]
				- Actual:   [{string.Join(",", value)}]
				""");
		}
	}
}

internal abstract class Assertion
{
	public abstract void Validate();
}