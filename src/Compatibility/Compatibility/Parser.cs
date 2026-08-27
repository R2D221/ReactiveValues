internal static class Parser
{
	public static T Parse<T>(string s, IFormatProvider? formatProvider)
#if NETCOREAPP
		where T : IParsable<T>
	{
		return T.Parse(s, formatProvider);
	}
#else
	{
		return Reflection<T>.Parse(s, formatProvider);
	}

	private static readonly Type[] parseParamTypes = [typeof(string), typeof(IFormatProvider)];

	private static class Reflection<T>
	{
		public static T Parse(string s, IFormatProvider? provider) => func(s, provider);

		private static readonly Func<string, IFormatProvider?, T> func =
			typeof(T).GetMethod("Parse", parseParamTypes) switch
			{
				null => throw new NotSupportedException($"Type {typeof(T)} does not implement IParsable<{typeof(T)}>"),
				{/*notnull*/} method => (Func<string, IFormatProvider?, T>)method.CreateDelegate(typeof(Func<string, IFormatProvider?, T>))
			};
	}
#endif
}
