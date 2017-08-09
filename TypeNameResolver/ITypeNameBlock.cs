using System;

namespace TypeNameResolver
{
	#region ITypeNameBlock

	public interface ITypeNameBlock : IStringView, ICloneable
	{
		TypeNameBlockType Type { get; }
	}

	#endregion ITypeNameBlock
}
