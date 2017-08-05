using System;
using System.Collections.Generic;
using System.Text;

namespace TypeNameResolver
{
	#region ITypeNameScope

	public interface ITypeNameScope
	{
		ITypeNameBlock Culture { get; }
		int ExpectedGenericsArgsCount { get; set; }
		int GenericsArgsCount { get; }
		List<ITypeNameScope> GenericsArguments { get; }
		int GenericsBlockDepth { get; set; }
		bool HasGenericsArgument { get; }
		int Id { get; }
		bool InNameBlock { get; }
		bool IsArray { get; set; }
		bool IsGenericArgument { get; }
		bool IsGenericType { get; }
		bool IsRoot { get; }
		int NameBlockDepth { get; set; }

		string ShortName { get; }
		string FullName { get; }
		string AssemblyQualifiedName { get; }

		ITypeNameBlock Name { get; }
		ITypeNameBlock AssemblyName { get; }
		ITypeNameBlock Version { get; }
		ITypeNameScope Parent { get; }
		ITypeNameBlock PublicKeyToken { get; }

		int IndexOf(ITypeNameScope child);

        Type ResolveType();
	}

	#endregion ITypeNameScope
}
