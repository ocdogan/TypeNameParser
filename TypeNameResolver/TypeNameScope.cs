using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TypeNameResolver
{
	#region TypeNameScope

	internal class TypeNameScope : ITypeNameScope
	{
		#region Static Members

        private static readonly ConcurrentDictionary<string, Type> s_OrdinalTypeCache = new ConcurrentDictionary<string, Type>();
        private static readonly ConcurrentDictionary<string, Type> s_TypeCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        #endregion Static Members

        #region Field Members

		private Type m_Type;
		private string m_Text;
		private bool m_InBlock;
		private bool m_IsArray;
		private int m_NameBlockDepth;
		private int m_ExpectedGenericsArgsCount = -1;
		private int m_GenericsBlockDepth;

        private string m_AQName;
		private string m_FullName;
        private string m_ShortName;
		private string m_NameWithAssembly;

		#endregion Field Members

		#region .Ctors

		public TypeNameScope(int id, string text, int genericsArgsCount = -1)
		{
			Id = id;
			m_Text = text;
			if (genericsArgsCount > -1)
			{
				ExpectedGenericsArgsCount = genericsArgsCount;
			}
		}

		#endregion .Ctors

		#region Properties

		public ITypeNameBlock Name { get; private set; }
		public ITypeNameBlock AssemblyName { get; private set; }
		public ITypeNameBlock Version { get; private set; }
		public ITypeNameBlock PublicKeyToken { get; private set; }
		public ITypeNameBlock Culture { get; private set; }

		public ITypeNameScope Parent { get; private set; }
		public List<ITypeNameScope> GenericsArguments { get; private set; }

		public string AssemblyQualifiedName
		{
			get
            {
                if (m_AQName == null)
                {
                    var sBuilder = new StringBuilder(200);
                    AppendAssemblyQualifiedName(sBuilder);
                    m_AQName = sBuilder.ToString();
                }
                return m_AQName;
			}
		}

		public int ExpectedGenericsArgsCount
		{
			get { return Math.Max(0, m_ExpectedGenericsArgsCount); }
			set
			{
				if ((value < 1) || (m_ExpectedGenericsArgsCount > -1))
					throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenericsCount, m_Text, -1, TypeNameToken.Undefined);

				m_ExpectedGenericsArgsCount = value;
				if (m_ExpectedGenericsArgsCount > 0)
				{
					GenericsArguments = new List<ITypeNameScope>(m_ExpectedGenericsArgsCount);
				}
			}
		}

		public string FullName
		{
			get
			{
                if (m_FullName == null)
                {
                    var sBuilder = new StringBuilder(200);
                    AppendFullName(sBuilder);
                    m_FullName = sBuilder.ToString();
                }
                return m_FullName;
			}
		}

		public int GenericsBlockDepth
		{
			get { return m_GenericsBlockDepth; }
			set
			{
				if (IsGenericType &&
					((m_GenericsBlockDepth == 1 && value == 0) ||
					 (m_GenericsBlockDepth == 0 && value == 1)))
				{
					m_GenericsBlockDepth = value;
					return;
				}
				throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenerics, m_Text, -1, TypeNameToken.Undefined);
			}
		}

		public int GenericsArgsCount
		{
			get { return GenericsArguments != null ? GenericsArguments.Count : 0; }
		}

		public bool HasGenericsArgument
		{
			get { return GenericsArguments != null ? GenericsArguments.Count > 0 : false; }
		}

		public int Id { get; private set; }

		public bool InNameBlock
		{
			get { return m_InBlock; }
			private set
			{
				if (!m_InBlock)
				{
					m_InBlock = value;
				}
			}
		}

		public bool IsArray
        {
            get { return m_IsArray; }
            set
            {
                if (m_IsArray != value && !IsGenericType)
                {
                    m_IsArray = value;
                    ClearNameCache();
                }
            }
        }

		public bool IsGenericArgument
		{
			get { return Parent != null; }
		}

		public bool IsGenericType
		{
			get { return m_ExpectedGenericsArgsCount > 0; }
		}

        public bool IsRoot 
        { 
            get { return Parent == null; }
        }

		public int NameBlockDepth
		{
			get { return m_NameBlockDepth; }
			set
			{
				if (!IsRoot &&
					((m_NameBlockDepth == 1 && value == 0) ||
					 (m_NameBlockDepth == 0 && value == 1)))
				{
					m_NameBlockDepth = value;
					InNameBlock = (value > 0);

					return;
				}
				throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenerics, m_Text, -1, TypeNameToken.Undefined);
			}
		}

		public string NameWithAssembly
		{
			get
			{
				if (m_NameWithAssembly == null)
				{
					var sBuiler = new StringBuilder(200);
					AppendNameWithAssembly(sBuiler);
					m_NameWithAssembly = sBuiler.ToString();
				}
				return m_NameWithAssembly;
			}
		}

		public string ShortName
		{
			get
			{
                if (m_ShortName == null)
                {
                    var sBuilder = new StringBuilder(200);
                    AppendShortName(sBuilder);
                    m_ShortName = sBuilder.ToString();
                }
                return m_ShortName;
			}
		}

		#endregion Properties

		#region Methods

        private void ClearNameCache()
        {
		    m_AQName = null;
		    m_FullName = null;
		    m_ShortName = null;
            m_NameWithAssembly = null;
	    }

		public bool CanAddGenericsArgument()
		{
			return IsGenericType &&
				(GenericsArguments.Count < m_ExpectedGenericsArgsCount);
		}

		public void AddGenericsArgument(TypeNameScope argument)
		{
			if (argument == null || argument.Parent != null || !CanAddGenericsArgument())
				throw TypeNameParserCommon.NewError(TypeNameError.GenericsArgumentsCountExceeded, m_Text, -1, TypeNameToken.Undefined);

			GenericsArguments.Add(argument);
			argument.Parent = this;

            ClearNameCache();
		}

		public void SetName(int start, int length = -1)
		{
			if (Name != null)
				throw TypeNameParserCommon.NewError(TypeNameError.TypeNameAlreadyDefined, m_Text, -1, TypeNameToken.TypeName);

			Name = new TypeNameBlock(m_Text, TypeNameBlockType.TypeName, start, length);

			ClearNameCache();
		}

		public void SetAssemblyName(int start, int length = -1)
		{
			if (AssemblyName != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyNameAlreadyDefined, m_Text, -1, TypeNameToken.AssemblyName);

			AssemblyName = new TypeNameBlock(m_Text, TypeNameBlockType.AssemblyName, start, length);

			ClearNameCache();
		}

		public void SetVersion(int start, int length = -1)
		{
			if (Version != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyVersionAlreadyDefined, m_Text, -1, TypeNameToken.Version);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefineVersionBeforeAssemblyName, m_Text, -1, TypeNameToken.Version);

			Version = new TypeNameBlock(m_Text, TypeNameBlockType.Version, start, length);

			ClearNameCache();
		}

		public void SetCulture(int start, int length = -1)
		{
			if (Culture != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyCultureAlreadyDefined, m_Text, -1, TypeNameToken.Culture);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefineCultureBeforeAssemblyName, m_Text, -1, TypeNameToken.Culture);

			Culture = new TypeNameBlock(m_Text, TypeNameBlockType.Culture, start, length);

			ClearNameCache();
		}

		public void SetPublicKeyToken(int start, int length = -1)
		{
			if (PublicKeyToken != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyPublicKeyTokenAlreadyDefined, m_Text, -1, TypeNameToken.PublicKeyToken);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefinePublicKeyTokenBeforeAssemblyName, m_Text, -1, TypeNameToken.PublicKeyToken);

			PublicKeyToken = new TypeNameBlock(m_Text, TypeNameBlockType.PublicKeyToken, start, length);

			ClearNameCache();
		}

		public int IndexOf(ITypeNameScope child)
		{
			if ((child != null) && (child.Parent == this))
			{
				var args = GenericsArguments;
				if (args != null)
				{
					return args.IndexOf(child);
				}
			}
			return -1;
		}

		#region Append Name Methods

		internal void AppendAssemblyQualifiedName(StringBuilder sBuilder)
		{
			var hasAssembly = (AssemblyName != null) && !AssemblyName.IsEmpty;
			if (hasAssembly && !IsRoot)
			{
				sBuilder.Append('[');
			}

			sBuilder.Append(Name != null ? Name.Text : String.Empty);

			if (IsGenericType)
			{
				var genericsBase = IsRoot && IsGenericType && !HasGenericsArgument;

                if (!genericsBase)
                {
                    sBuilder.Append('[');

                    var genericsArgsCount = GenericsArguments != null ? GenericsArguments.Count : 0;

                    for (var i = 0; i < ExpectedGenericsArgsCount; i++)
                    {
                        if (i > 0)
                        {
                            sBuilder.Append(",");
                        }

                        if (i < genericsArgsCount)
                        {
                            ((TypeNameScope)GenericsArguments[i]).AppendAssemblyQualifiedName(sBuilder);
                        }
                    }

                    sBuilder.Append(']');
                }
			}

			if (hasAssembly)
			{
				sBuilder.Append(", ");
				sBuilder.Append(AssemblyName.Text);

				sBuilder.Append(", Version=");
				sBuilder.Append(Version != null && !Version.IsEmpty ? Version.Text : "0.0.0.0");

				sBuilder.Append(", Culture=");
				sBuilder.Append(Culture != null && !Culture.IsEmpty ? Culture.Text : "neutral");

				sBuilder.Append(", PublicKeyToken=");
				sBuilder.Append(PublicKeyToken != null && !PublicKeyToken.IsEmpty ? PublicKeyToken.Text : "null");
			}

			if (hasAssembly && !IsRoot)
			{
				sBuilder.Append(']');
			}
		}

		internal void AppendFullName(StringBuilder sBuilder)
		{
			var hasAssembly = (AssemblyName != null) && !AssemblyName.IsEmpty;
			if (hasAssembly && !IsRoot)
			{
				sBuilder.Append('[');
			}

			sBuilder.Append(Name != null ? Name.Text : String.Empty);

			if (IsGenericType)
			{
				sBuilder.Append('[');

				var genericsArgsCount = GenericsArguments != null ? GenericsArguments.Count : 0;

				for (var i = 0; i < ExpectedGenericsArgsCount; i++)
				{
					if (i > 0)
					{
						sBuilder.Append(",");
					}

					if (i < genericsArgsCount)
					{
                        ((TypeNameScope)GenericsArguments[i]).AppendFullName(sBuilder);
					}
				}

				sBuilder.Append(']');
			}

			if (hasAssembly && !IsRoot)
			{
				sBuilder.Append(", ");
				sBuilder.Append(AssemblyName.Text);

				sBuilder.Append(", Version=");
				sBuilder.Append(Version != null && !Version.IsEmpty ? Version.Text : "0.0.0.0");

				sBuilder.Append(", Culture=");
				sBuilder.Append(Culture != null && !Culture.IsEmpty ? Culture.Text : "neutral");

				sBuilder.Append(", PublicKeyToken=");
				sBuilder.Append(PublicKeyToken != null && !PublicKeyToken.IsEmpty ? PublicKeyToken.Text : "null");

				sBuilder.Append(']');
			}
		}

		internal void AppendShortName(StringBuilder sBuilder)
		{
			sBuilder.Append(Name != null ? Name.Text : String.Empty);

			if (IsGenericType)
			{
				sBuilder.Append('[');

				var genericsArgsCount = GenericsArguments != null ? GenericsArguments.Count : 0;

				for (var i = 0; i < ExpectedGenericsArgsCount; i++)
				{
					if (i > 0)
					{
						sBuilder.Append(",");
					}

					if (i < genericsArgsCount)
					{
                        ((TypeNameScope)GenericsArguments[i]).AppendShortName(sBuilder);
					}
				}

				sBuilder.Append(']');
			}
		}

        internal void AppendNameWithAssembly(StringBuilder sBuilder)
		{
			var hasAssembly = (AssemblyName != null) && !AssemblyName.IsEmpty;
			if (hasAssembly && !IsRoot)
			{
				sBuilder.Append('[');
			}

			sBuilder.Append(Name != null ? Name.Text : String.Empty);

			if (IsGenericType)
			{
				var genericsBase = IsRoot && IsGenericType && !HasGenericsArgument;

				if (!genericsBase)
				{
					sBuilder.Append('[');

					var genericsArgsCount = GenericsArguments != null ? GenericsArguments.Count : 0;

					for (var i = 0; i < ExpectedGenericsArgsCount; i++)
					{
						if (i > 0)
						{
							sBuilder.Append(",");
						}

						if (i < genericsArgsCount)
						{
							((TypeNameScope)GenericsArguments[i]).AppendNameWithAssembly(sBuilder);
						}
					}

					sBuilder.Append(']');
				}
			}

			if (hasAssembly)
			{
				sBuilder.Append(", ");
				sBuilder.Append(AssemblyName.Text);
			}

			if (hasAssembly && !IsRoot)
			{
				sBuilder.Append(']');
			}
		}

		#endregion Append Name Methods

        public static Type ResolveType(string typeToResolve, bool throwOnError = false, bool ignoreCase = false)
		{
			var cache = (ignoreCase ? s_TypeCache : s_OrdinalTypeCache);

			var result = Type.GetType(typeToResolve,
			(assemblyName) =>
			{
				var assemblies = AppDomain.CurrentDomain.GetAssemblies()
							  .ToDictionary(a => ignoreCase ? a.GetName().Name.ToLowerInvariant() : a.GetName().Name);

				var assembly = (Assembly)null;
				assemblies.TryGetValue(ignoreCase ? assemblyName.Name.ToLowerInvariant() : assemblyName.Name, out assembly);
				return assembly;
			},
			(assembly, typeName, ignoreCase2) =>
			{
				Type type;
				if (assembly != null)
				{
					var tAqn = typeName + ", " + assembly.GetName().Name;
					if (cache.TryGetValue(tAqn, out type) && type != null)
						return type;

					type = assembly.GetType(typeName, throwOnError, ignoreCase);
					if (type != null)
					{
						s_TypeCache[tAqn] = type;
						s_OrdinalTypeCache[tAqn] = type;
					}
					return type;
				}

				if (cache.TryGetValue(typeToResolve, out type) && type != null)
					return type;

				var tn = typeName;
				var ns = (string)null;

				var nsEmpty = true;
				var li = typeName.LastIndexOf('.');
				if (li > -1)
				{
					ns = typeName.Substring(0, li);
					tn = typeName.Substring(li + 1);

					nsEmpty = String.IsNullOrEmpty(ns);
				}

				var assemblies = AppDomain.CurrentDomain.GetAssemblies()
							  .ToDictionary(a => ignoreCase ? a.GetName().Name.ToLowerInvariant() : a.GetName().Name);

				if (!nsEmpty)
				{
					var mscorlib = typeof(bool).Assembly.GetName();
					var possibileAssemblyNames = new List<string>
					{
						ignoreCase2 ? mscorlib.Name.ToLowerInvariant() : mscorlib.Name
					};

					var possibleAssemblyName = String.Empty;
					var possibilities = ns.Split('.')
						.Select(part =>
						{
							possibleAssemblyName += "." + (ignoreCase2 ? part.ToLowerInvariant() : part);
							return possibleAssemblyName.Substring(1);
						});

					possibileAssemblyNames.AddRange(possibilities);

					var possibleType = possibileAssemblyNames
						.Where(name => assemblies.ContainsKey(name))
						.Select(name => assemblies[name].GetType(typeName, false, ignoreCase2))
						.FirstOrDefault(t => t != null);

					if (possibleType != null)
					{
						s_TypeCache[typeName] = possibleType;
						s_OrdinalTypeCache[typeName] = possibleType;

						return possibleType;
					}

					possibileAssemblyNames.ForEach(name =>
					{
						if (assemblies.ContainsKey(name))
							assemblies.Remove(name);
					});
				}

				if (nsEmpty)
				{
					assemblies = assemblies.Where(kvp =>
									 !(String.Compare(kvp.Key, "mscorlib", ignoreCase2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0 ||
									   String.Compare(kvp.Key, "system", ignoreCase2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0 ||
									   kvp.Key.StartsWith("system.", ignoreCase2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
									).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
				}

				type = assemblies.Select(kvp =>
				{
					var asm = kvp.Value;

					var t = asm.GetType(typeName, false, ignoreCase2);
					if (nsEmpty && (t == null))
					{
						t = asm.GetTypes().FirstOrDefault(t2 => String.IsNullOrEmpty(t2.Namespace) &&
												   ignoreCase2 ?
												   String.Compare(t2.Name, tn, StringComparison.OrdinalIgnoreCase) == 0 :
												   String.CompareOrdinal(t2.Name, tn) == 0);
					}
					return t;
				})
				.FirstOrDefault(t => t != null);

				if (type != null)
				{
					s_TypeCache[typeName] = type;
					s_OrdinalTypeCache[typeName] = type;
				}

				return type;
			}, throwOnError, ignoreCase);

			if (throwOnError && result == null)
				throw new TypeLoadException(String.Format("Type '{0}' cannot be found", typeToResolve));

            return result;
		}

        public Type ResolveType(bool throwOnError = false, bool ignoreCase = false)
        {
            if (m_Type != null)
                return m_Type;

            var cache = (ignoreCase ? s_TypeCache : s_OrdinalTypeCache);

            var cacheKey = NameWithAssembly;
            if (cache.TryGetValue(cacheKey, out m_Type) && m_Type != null)
                return m_Type;

            m_Type = ResolveType(AssemblyQualifiedName, throwOnError, ignoreCase);

            if (m_Type != null)
            {
                s_TypeCache[cacheKey] = m_Type;
                s_OrdinalTypeCache[cacheKey] = m_Type;
            }

            return m_Type;
        }

		#region ToString

		public override string ToString()
		{
			var sBuilder = new StringBuilder("[TypeName: IsRoot=");
			sBuilder.Append(IsRoot);

			sBuilder.AppendFormat(", InNameBlock={0}, IsGenericType={1}, ExpectedGenericsArgsCount={2}, GenericsArgsCount={3}",
								  InNameBlock, IsGenericType, ExpectedGenericsArgsCount, GenericsArgsCount);

			if (Name != null)
			{
				sBuilder.Append(", Name=");
				sBuilder.Append(Name);
			}

			if (AssemblyName != null)
			{
				sBuilder.Append(", AssemblyName=");
				sBuilder.Append(AssemblyName);
			}

			if (Version != null)
			{
				sBuilder.Append(", Version=");
				sBuilder.Append(Version);
			}

			if (Culture != null)
			{
				sBuilder.Append(", Culture=");
				sBuilder.Append(Culture);
			}

			if (PublicKeyToken != null)
			{
				sBuilder.Append(", PublicKeyToken=");
				sBuilder.Append(PublicKeyToken);
			}

			sBuilder.Append(']');

			return sBuilder.ToString();
		}

		#endregion ToString

		#endregion Methods
	}

	#endregion TypeNameScope

}
