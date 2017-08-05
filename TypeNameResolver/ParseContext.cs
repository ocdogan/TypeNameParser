using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TypeNameResolver
{
	#region ParseContext

	internal class ParseContext : IDisposable
	{
		#region Static Members

		#region Static Readonly Members

		private static readonly char[] InvalidAssemblyNameChars =
			Path.GetInvalidFileNameChars().Concat(new char[] { '*', '/', '[', ']' }).ToArray();

		private static readonly char[] InvalidTypeNameChars = { ',', '[', ']' };

		private static readonly TypeNameToken[] ValidTerminationTokens =
			{
				TypeNameToken.TypeName,
				TypeNameToken.ArrayEnd,
				TypeNameToken.AssemblyName,
				TypeNameToken.VersionValue,
				TypeNameToken.CultureValue,
				TypeNameToken.PublicKeyTokenValue,
				TypeNameToken.GenericsEnd,
			};

		#endregion Static Readonly Members

		#endregion Static Members

		#region Field Members

		private string m_Text;
		private int m_TextLen;
		private bool m_Debug;

		private int m_Id;
		private int m_Pos = -1;
		private char? m_CurrChar;
		private bool m_InRoot = true;
		private TypeNameToken m_Token = TypeNameToken.TypeNameStart;
		private TypeNameToken m_PrevToken = TypeNameToken.Undefined;

		private TypeNameScope m_Root;
		private TypeNameScope m_CurrentScope;

		private Stack<TypeNameScope> m_Stack = new Stack<TypeNameScope>();
		private Stack<TypeNameParseTrace> m_Trace = new Stack<TypeNameParseTrace>();

		#endregion Field Members

		#region .Ctors

		public ParseContext(string text, bool debug = false)
		{
			m_Debug = debug;
			m_Text = text;
			m_TextLen = (text != null ? text.Length : 0);

			m_Root = new TypeNameScope(m_Id++, m_Text, true);
			m_Stack.Push(m_Root);

			m_CurrentScope = m_Root;

			if (m_Debug)
			{
				m_Trace.Push(new TypeNameParseTrace(m_Token, m_PrevToken, m_Pos, null));
			}
		}

		#endregion .Ctors

		#region Properties

		public bool AtLast
		{
			get { return (m_TextLen > 0 && (m_Pos == m_TextLen - 1)); }
		}

		public char? CurrentChar
		{
			get { return m_CurrChar; }
		}

		public TypeNameScope CurrentScope
		{
			get { return m_CurrentScope; }
		}

		public bool InRoot
		{
			get { return m_InRoot; }
		}

		public int CurrentPos
		{
			get { return m_Pos; }
			set
			{
				m_Pos = Math.Min(m_TextLen, Math.Max(-1, value));
				m_CurrChar = (m_Pos > -1 && m_Pos < m_TextLen) ? (char?)Text[m_Pos] : null;
			}
		}

		public TypeNameToken PrevToken
		{
			get { return m_PrevToken; }
		}

		public TypeNameScope Root
		{
			get { return m_Root; }
		}

		public Stack<TypeNameScope> Stack
		{
			get { return m_Stack; }
		}

		public string Text
		{
			get { return m_Text; }
		}

		public int TextLength
		{
			get { return m_TextLen; }
		}

		public TypeNameToken Token
		{
			get { return m_Token; }
			set
			{
				if (m_Token != value)
				{
					m_PrevToken = m_Token;
					m_Token = value;

					if (m_Debug)
					{
						var scope = CurrentScope;
						var name = (scope == null ? null : (scope.Name == null ? null : scope.Name.Text));

						m_Trace.Push(new TypeNameParseTrace(m_Token, m_PrevToken, m_Pos, name));
					}
				}
			}
		}

		public string TokenAsString
		{
			get { return m_Token.ToString("F"); }
		}

		public Stack<TypeNameParseTrace> Trace
		{
			get { return m_Trace; }
		}

		#endregion Properties

		#region Methods

		public void Dispose()
		{
			m_Root = null;
			m_CurrentScope = null;

			m_Stack = null;
			m_Trace = null;
		}

		public bool Next()
		{
			if (m_Pos < m_TextLen - 1)
			{
				CurrentPos++;
				return true;
			}
			return false;
		}

		public bool Previous()
		{
			if (m_Pos > -1)
			{
				CurrentPos--;
				return true;
			}
			return false;
		}

		public ITypeNameScope StartScope()
		{
			var parentScope = m_Stack.Peek();

			if (!parentScope.CanAddGenericsArgument())
				throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenericsArgumentStart, m_Text, m_Pos, m_Token);

			var scope = new TypeNameScope(m_Id++, m_Text);
			parentScope.AddGenericsArgument(scope);

			m_Stack.Push(scope);

			m_CurrentScope = scope;
			m_InRoot = false;

			return scope;
		}

		public ITypeNameScope EndScope()
		{
			var ret = m_Stack.Pop();

			m_CurrentScope = (m_Stack.Count > 0 ? m_Stack.Peek() : null);
			m_InRoot = (m_CurrentScope == m_Root);

			return ret;
		}

		public int FindNonwhitespace(int fromPos = -1, bool throwErrorOnComplete = true)
		{
			if (m_TextLen == 0)
				return -1;

			if (fromPos < 0) fromPos = m_Pos;

			var pos = Math.Min(Math.Max(fromPos, -1), m_TextLen);
			while (pos < m_TextLen && pos > -1)
			{
				if (char.IsWhiteSpace(m_Text[pos]))
					pos++;
				break;
			}

			if (throwErrorOnComplete && pos > m_TextLen - 1)
				throw TypeNameParserCommon.NewError(TypeNameError.InvalidTypeNameTermination, this);

			return pos;
		}

		public int FindNonwhitespaceBackward(int fromPos = -1, bool throwErrorOnComplete = true)
		{
			if (m_TextLen == 0)
				return -1;

			if (fromPos < 0) fromPos = m_Pos;

			var pos = Math.Min(Math.Max(fromPos, -1), m_TextLen - 1);
			while (pos > -1)
			{
				if (char.IsWhiteSpace(m_Text[pos]))
					pos--;
				break;
			}

			if (throwErrorOnComplete && pos < 0)
				throw TypeNameParserCommon.NewError(TypeNameError.InvalidTypeName, this);

			return pos;
		}

		public int EatWhitespace(int fromPos = -1, bool throwErrorOnComplete = true)
		{
			var pos = FindNonwhitespace(fromPos, throwErrorOnComplete);

			CurrentPos = Math.Min(m_TextLen, Math.Max(0, pos));
			return CurrentPos;
		}

		public int EatWhitespaceBackward(int fromPos = -1, bool throwErrorOnComplete = true)
		{
			var pos = FindNonwhitespaceBackward(fromPos, throwErrorOnComplete);

			CurrentPos = Math.Min(m_TextLen, Math.Max(0, pos));
			return CurrentPos;
		}

		public bool IsValidAssemblyNameChar()
		{
			var ch = CurrentChar;
			if (ch.HasValue)
			{
				return (ch != ',') && !InvalidAssemblyNameChars.Contains(ch.Value);
			}
			return false;
		}

		public bool IsValidTypeNameChar(bool isFirstChar = false)
		{
			var ch = CurrentChar;
			if (ch.HasValue)
			{
				if (isFirstChar)
				{
					return (ch == '@' || ch == '_' || char.IsLetter(ch.Value));
				}
				return !InvalidTypeNameChars.Contains(ch.Value);
			}
			return false;
		}

		public bool IsValidVersionChar()
		{
			var ch = CurrentChar;
			if (ch.HasValue)
			{
				return (ch == '.' || char.IsNumber(ch.Value));
			}
			return false;
		}

		public bool IsValidCultureChar()
		{
			var ch = CurrentChar;
			if (ch.HasValue)
			{
				return !(ch.Value == ',' || ch.Value == ']');
			}
			return false;
		}

		public bool IsValidPublicKeyTokenChar()
		{
			var ch = CurrentChar;
			if (ch.HasValue)
			{
				return (char.IsNumber(ch.Value) ||
						(ch >= 'a' && ch <= 'f') ||
						(ch >= 'A' && ch <= 'F') ||
						ch == 'n' || ch == 'u' || ch == 'l' ||
						ch == 'N' || ch == 'U' || ch == 'L');
			}
			return false;
		}

		public bool IsValidChar(bool isFirstChar = false)
		{
			switch (m_Token)
			{
				case TypeNameToken.TypeName:
					return IsValidTypeNameChar(isFirstChar);
				case TypeNameToken.AssemblyName:
					return IsValidAssemblyNameChar();
				case TypeNameToken.VersionValue:
					return IsValidVersionChar();
				case TypeNameToken.CultureValue:
					return IsValidCultureChar();
				case TypeNameToken.PublicKeyTokenValue:
					return IsValidPublicKeyTokenChar();
			}
			return true;
		}

		public bool IsWhitespace()
		{
			var ch = CurrentChar;
			if (ch.HasValue)
			{
				return char.IsWhiteSpace(ch.Value);
			}
			return false;
		}

		public bool Is(char ch)
		{
			if (m_CurrChar.HasValue)
			{
				return m_CurrChar == ch;
			}
			return false;
		}

		public TypeNameException CreateError(TypeNameError error)
		{
			return TypeNameParserCommon.NewError(error, this);
		}

		public TypeNameException DefaultError()
		{
			switch (m_Token)
			{
				case TypeNameToken.AssemblyName:
					return CreateError(TypeNameError.InvalidAssemblyName);
				case TypeNameToken.Version:
					return CreateError(TypeNameError.InvalidAssemblyVersionValue);
				case TypeNameToken.Culture:
					return CreateError(TypeNameError.InvalidAssemblyCultureValue);
				case TypeNameToken.PublicKeyToken:
					return CreateError(TypeNameError.InvalidAssemblyPublicKeyTokenValue);
				case TypeNameToken.AssemblyPropertyStart:
					return CreateError(TypeNameError.InvalidAssemblyProperty);
				case TypeNameToken.TypeNameStart:
					return CreateError(TypeNameError.InvalidTypeNameStart);
				case TypeNameToken.TypeName:
					return CreateError(TypeNameError.InvalidTypeNameChar);
				case TypeNameToken.TypeNameEnd:
					return CreateError(TypeNameError.InvalidTypeNameEnd);
				case TypeNameToken.ArrayStart:
					return CreateError(TypeNameError.InvalidArrayStart);
				case TypeNameToken.Array:
					return CreateError(TypeNameError.InvalidArray);
				case TypeNameToken.ArrayEnd:
					return CreateError(TypeNameError.InvalidArrayEnd);
				case TypeNameToken.GenericsCountStart:
					return CreateError(TypeNameError.InvalidGenericsCount);
				case TypeNameToken.GenericsCount:
					return CreateError(TypeNameError.InvalidGenericsCount);
				case TypeNameToken.GenericsCountEnd:
					return CreateError(TypeNameError.InvalidGenericsCount);
				case TypeNameToken.GenericsStart:
					return CreateError(TypeNameError.InvalidGenericsCount);
				case TypeNameToken.Generics:
					return CreateError(TypeNameError.InvalidGenerics);
				case TypeNameToken.GenericsEnd:
					return CreateError(TypeNameError.InvalidGenerics);
				case TypeNameToken.GenericsArgBlockOpen:
					return CreateError(TypeNameError.InvalidGenericsArgBlockOpen);
				case TypeNameToken.GenericsArgBlockClose:
					return CreateError(TypeNameError.InvalidGenericsArgBlockClose);
				case TypeNameToken.GenericsArgumentStart:
					return CreateError(TypeNameError.InvalidGenericsArgumentStart);
				case TypeNameToken.GenericsArgumentEnd:
					return CreateError(TypeNameError.InvalidGenericsArgumentEnd);
				case TypeNameToken.AssemblyNameStart:
					return CreateError(TypeNameError.InvalidAssemblyName);
				case TypeNameToken.VersionValueStart:
					return CreateError(TypeNameError.InvalidAssemblyVersion);
				case TypeNameToken.CultureValueStart:
					return CreateError(TypeNameError.InvalidAssemblyCulture);
				case TypeNameToken.PublicKeyTokenValueStart:
					return CreateError(TypeNameError.InvalidAssemblyPublicKeyToken);
				case TypeNameToken.VersionValue:
					return CreateError(TypeNameError.InvalidAssemblyVersionValue);
				case TypeNameToken.CultureValue:
					return CreateError(TypeNameError.InvalidAssemblyCultureValue);
				case TypeNameToken.PublicKeyTokenValue:
					return CreateError(TypeNameError.InvalidAssemblyPublicKeyTokenValue);
			}
			return CreateError(TypeNameError.InvalidTypeName);
		}

		#endregion Methods
	}

	#endregion ParseContext

}
