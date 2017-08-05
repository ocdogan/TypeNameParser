using System;

namespace TypeNameResolver
{
	#region TypeNameParseTrace

	public struct TypeNameParseTrace
	{
		#region Field Members

		private int m_Pos;
		private string m_Name;
		private TypeNameToken m_Token;
		private TypeNameToken m_PrevToken;

		#endregion Field Members

		#region .Ctors

		public TypeNameParseTrace(TypeNameToken token, TypeNameToken prevToken = TypeNameToken.Undefined,
			int pos = -1, string name = null)
		{
			m_Pos = pos;
			m_Token = token;
			m_PrevToken = prevToken;
			m_Name = name;
		}

		#endregion .Ctors

		#region Properties

		public int Pos
		{
			get { return m_Pos; }
			private set { m_Pos = value; }
		}

		public string Name
		{
			get { return m_Name; }
			private set { m_Name = value; }
		}

		public TypeNameToken Token
		{
			get { return m_Token; }
			private set { m_Token = value; }
		}

		public TypeNameToken PrevToken
		{
			get { return m_PrevToken; }
			private set { m_PrevToken = value; }
		}

		#endregion Properties

		#region Methods

		public override string ToString()
		{
			return String.Format("[{0}: Pos={1}, Token={2}, PrevToken={3}, Name='{4}']",
			   GetType().Name, Pos, Token, PrevToken, Name);
		}

		#endregion Methods
	}

	#endregion TypeNameParseTrace
}
