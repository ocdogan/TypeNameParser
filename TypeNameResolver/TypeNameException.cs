using System;
using System.Runtime.Serialization;

namespace TypeNameResolver
{
	#region TypeNameException

	public class TypeNameException : Exception
	{
		#region Field Members

		private string m_Text;
		private int m_ErrorNo = -1;
		private int m_Position = -1;
		private TypeNameToken m_Token = TypeNameToken.Undefined;

		#endregion Field Members

		#region .Ctors

		public TypeNameException()
		{ }

		public TypeNameException(int errorNo)
		{
			m_ErrorNo = errorNo;
		}

		public TypeNameException(string message, int errorNo = -1, int position = -1, string text = null,
			TypeNameToken token = TypeNameToken.Undefined, Exception innerException = null)
			: base(message, innerException)
		{
			m_Text = text;
			m_Token = token;
			m_ErrorNo = errorNo;
			m_Position = position;
		}

		protected TypeNameException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

		#endregion .Ctors

		#region Properties

		public int ErrorNo
		{
			get { return m_ErrorNo; }
		}

		public int Position
		{
			get { return m_Position; }
		}

		public string Text
		{
			get { return m_Text; }
		}

		public TypeNameToken Token
		{
			get { return m_Token; }
		}

		#endregion Properties
	}

	#endregion TypeNameException

}
