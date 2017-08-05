using System;

namespace TypeNameResolver
{
	#region TypeNameBlock

	internal class TypeNameBlock : StringView, ITypeNameBlock, IStringView
	{
		#region .Ctors

		public TypeNameBlock(string text, TypeNameBlockType type, int start = -1, int length = -1)
			: base(text, start, length)
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

		#endregion Methods
	}

	#endregion TypeNameBlock

}
