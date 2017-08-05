namespace TypeNameResolver
{
	#region TypeNameToken

	public enum TypeNameToken
	{
		Undefined,
		TypeNameStart,
		TypeName,
		TypeNameEnd,
		GenericsCountStart,
		GenericsCount,
		GenericsCountEnd,
		GenericsStart,
		Generics,
		GenericsEnd,
		GenericsArgBlockOpen,
		GenericsArgumentStart,
		GenericsArguments,
		GenericsArgumentEnd,
		GenericsArgBlockClose,
		ArrayStart,
		Array,
		ArrayEnd,
		AssemblyNameStart,
		AssemblyName,
		AssemblyNameEnd,
		AssemblyPropertyStart,
		Version,
		VersionValueStart,
		VersionValue,
		Culture,
		CultureValueStart,
		CultureValue,
		PublicKeyToken,
		PublicKeyTokenValueStart,
		PublicKeyTokenValue,
	}

	#endregion TypeNameToken
}
