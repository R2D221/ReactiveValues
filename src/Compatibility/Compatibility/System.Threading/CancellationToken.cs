#if !NETCOREAPP
using System.Reflection;

internal static partial class CompatibilityExtensions
{
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