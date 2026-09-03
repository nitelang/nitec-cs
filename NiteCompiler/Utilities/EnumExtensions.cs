namespace NiteCompiler.Utilities;

public static class EnumExtensions
{
	extension<T>(T enumValue)
		where T : struct, Enum
	{
		/// <summary>
		/// Check if <see langword="this"/> enum value is defined.
		/// </summary>
		/// <returns><see langword="true"/> when defined; otherwise <see langword="false"/>.</returns>
		public bool IsValid()
		{
			return Enum.IsDefined(enumValue);
		}
	}
}