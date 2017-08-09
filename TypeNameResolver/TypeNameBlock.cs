using System;

namespace TypeNameResolver
{
	#region TypeNameBlock

    internal class TypeNameBlock : StringView, ITypeNameBlock, IStringView, ICloneable
	{
		#region .Ctors

        public TypeNameBlock(string text, TypeNameBlockType type, int start = -1, int end = -1, bool locked = false)
            : base(text, start, end, locked)
		{
			Type = type;
		}

		#endregion .Ctors

		#region Properties

		public TypeNameBlockType Type { get; private set; }

		#endregion Properties

		#region Methods

		public override string ToString()
		{
			return String.Format("[{0}: Type={1}, Start={2}, End={3}, IsLocked={4}, Text='{5}']",
				GetType().Name, Type, Start, End, IsLocked, Text);
		}

        public override object Clone()
		{
			return new TypeNameBlock(m_Text, Type, Start, End, IsLocked);
		}

		#endregion Methods
	}

	#endregion TypeNameBlock

}
