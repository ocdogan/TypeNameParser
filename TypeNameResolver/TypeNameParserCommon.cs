using System;

namespace TypeNameResolver
{
    public static class TypeNameParserCommon
    {
		#region Parse Helper Methods

		internal static TypeNameException NewError(TypeNameError error, ParseContext context, Exception innerException = null)
		{
			return new TypeNameException(error.Message, error.ErrorNo, context.CurrentPos, context.Text, context.Token, innerException);
		}

		internal static TypeNameException NewError(TypeNameError error, string str, int pos, TypeNameToken token, Exception innerException = null)
		{
			return new TypeNameException(error.Message, error.ErrorNo, pos, str, token, innerException);
		}

		public static bool IsWhiteSpace(char? ch)
		{
			return (ch.HasValue && char.IsWhiteSpace(ch.Value));
		}

		public static bool Equals(string str, string checkFor, int start)
		{
			if (start <= str.Length - checkFor.Length)
			{
				var end = start + checkFor.Length;
				for (var i = start; i < end; i++)
				{
					if (char.ToLowerInvariant(str[i]) != char.ToLowerInvariant(checkFor[i - start]))
						return false;
				}
				return true;
			}
			return false;
		}

		public static int LastIndexOf(char ch, string inStr, int searchStartingFrom)
		{
			if (searchStartingFrom < inStr.Length)
			{
				var pos = searchStartingFrom;
				while (pos > -1)
				{
					if (inStr[pos] == ch)
						return pos;
					pos--;
				}
			}
			return -1;
		}

		public static string GetAssemblyPropertySampleText(TypeNameToken token)
		{
			switch (token)
			{
				case TypeNameToken.Version:
					return "Version=0.0.0.0";
				case TypeNameToken.Culture:
					return "Culture=en";
				case TypeNameToken.PublicKeyToken:
					return "PublicKeyToken=null";
			}
			return String.Empty;
		}

		#endregion Parse Helper Methods
	}
}
