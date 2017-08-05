namespace TypeNameResolver
{
	#region TypeNameParseResult

	public class TypeNameParseResult
	{
		#region .Ctors

		public TypeNameParseResult(ITypeNameScope scope, IStack<TypeNameParseTrace> trace)
		{
			Scope = scope;
			Trace = trace;
		}

		#endregion .Ctors

		#region Properties

		public ITypeNameScope Scope { get; private set; }

		public IStack<TypeNameParseTrace> Trace { get; private set; }

		#endregion Properties
	}

	#endregion TypeNameParseResult
}
