using System;

namespace TypeNameResolver
{
	#region StringView

	internal class StringView : IStringView
	{
		#region Field Members

		protected int m_TextLen;
		protected string m_Text;

		private int m_Start = -1;
		private int m_End = -1;
		private bool m_Locked;

		#endregion Field Members

		#region .Ctors

		public StringView(string text, int start = -1, int end = -1, bool locked = false)
		{
			Start = start;
			End = end;
			m_Text = text;
			m_Locked = locked;
			m_TextLen = text != null ? text.Length : 0;
		}

		#endregion .Ctors

		#region Properties

		public int Start
		{
			get { return m_Start; }
			set
			{
				if (!m_Locked)
				{
					m_Start = value;
				}
			}
		}

		public int End
		{
			get { return m_End; }
			set
			{
				if (!m_Locked)
				{
					m_End = value;
				}
			}
		}

		public bool IsEmpty
		{
			get
			{
				return (m_TextLen < 1 || Start < 0 ||
					Start > m_TextLen - 1 || End <= Start);
			}
		}

		public bool IsLocked
		{
			get { return m_Locked; }
		}

		public void Lock()
		{
			m_Locked = true;
		}

		public string Text
		{
			get
			{
				return IsEmpty ? String.Empty :
					m_Text.Substring(Start, Math.Max(0, Math.Min(End, m_TextLen) - Start));
			}
		}

		#endregion Properties

		#region Methods

		public override string ToString()
		{
			return String.Format("[{0}: Start={1}, End={2}, IsLocked={3}, Text='{4}']",
				GetType().Name, Start, End, IsLocked, Text);
		}

		#endregion Methods
	}

	#endregion StringView

}
