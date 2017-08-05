namespace TypeNameResolver
{
	#region ITypeNameBlock

	public interface ITypeNameBlock : IStringView
	{
		TypeNameBlockType Type { get; }
	}

	#endregion ITypeNameBlock
}
