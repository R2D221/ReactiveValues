#if !NETCOREAPP
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

internal static class CompatibilityExtensions
{
	#region Dictionary<TKey, TValue>
	extension<TKey, TValue>(Dictionary<TKey, TValue> @this)
		where TKey : notnull
	{
		public bool TryAdd(TKey key, TValue value)
		{
			if (@this.ContainsKey(key))
			{
				return false;
			}
			else
			{
				@this.Add(key, value);
				return true;
			}
		}
	}

	extension<TKey, TValue>(KeyValuePair<TKey, TValue> @this)
		where TKey : notnull
	{
		public void Deconstruct(out TKey key, out TValue value)
		{
			key = @this.Key;
			value = @this.Value;
		}
	}
	#endregion

	#region EqualityComparer<T>
	extension<T>(EqualityComparer<T>)
	{
		public static EqualityComparer<T> Create(Func<T?, T?, bool> equals, Func<T, int>? getHashCode = null)
		{
			getHashCode ??= _ => throw new NotSupportedException();

			return new DelegateEqualityComparer<T>(equals, getHashCode);
		}
	}

	private sealed class DelegateEqualityComparer<T>(
		Func<T?, T?, bool> equals,
		Func<T, int> getHashCode)
		: EqualityComparer<T>
	{
		public override bool Equals(T? x, T? y) =>
			equals(x, y);

		public override int GetHashCode(T obj) =>
			getHashCode(obj);
	}
	#endregion

	#region Stack<T>
	extension<T>(Stack<T> @this)
	{
		public bool TryPop([MaybeNullWhen(false)] out T result)
		{
			if (@this.Count == 0)
			{
				result = default;
				return false;
			}

			result = @this.Pop();
			return true;
		}

		public bool TryPeek([MaybeNullWhen(false)] out T result)
		{
			if (@this.Count == 0)
			{
				result = default;
				return false;
			}

			result = @this.Peek();
			return true;
		}
	}
	#endregion

	#region Queue<T>
	extension<T>(Queue<T> @this)
	{
		public bool TryDequeue([MaybeNullWhen(false)] out T result)
		{
			if (@this.Count == 0)
			{
				result = default;
				return false;
			}

			result = @this.Dequeue();
			return true;
		}

		public bool TryPeek([MaybeNullWhen(false)] out T result)
		{
			if (@this.Count == 0)
			{
				result = default;
				return false;
			}

			result = @this.Peek();
			return true;
		}
	}
	#endregion

	#region CancellationToken
	private static readonly MethodInfo CancellationToken_Register = typeof(CancellationToken).GetMethod("Register", BindingFlags.Instance | BindingFlags.NonPublic);

	extension(CancellationToken @this)
	{
		public CancellationTokenRegistration UnsafeRegister(Action<object?, CancellationToken> callback, object? state) =>
			(CancellationTokenRegistration)CancellationToken_Register.Invoke(@this, [(object? x) => callback(x, @this), state, false, false]);
	}
	#endregion
}
#endif