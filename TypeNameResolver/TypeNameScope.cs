using System;
using System.Collections.Generic;
using System.Text;

namespace TypeNameResolver
{
	#region TypeNameScope

    internal class TypeNameScope : ITypeNameScope, ICloneable
	{
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

        private TypeNameBlock m_Name;
        private TypeNameBlock m_AssemblyName;
        private TypeNameBlock m_Version;
        private TypeNameBlock m_PublicKeyToken;
        private TypeNameBlock m_Culture;

        private TypeNameScope m_Parent;

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

        public ITypeNameBlock Name 
        { 
            get { return m_Name; } 
        }

        public ITypeNameBlock AssemblyName 
        { 
            get { return m_AssemblyName; } 
        }

        public ITypeNameBlock Version 
        { 
            get { return m_Version; } 
        }

        public ITypeNameBlock PublicKeyToken 
        { 
            get { return m_PublicKeyToken; } 
        }

        public ITypeNameBlock Culture 
        { 
            get { return m_Culture; } 
        }

		public ITypeNameScope Parent
        {
            get { return m_Parent; }
        }

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
                    ScopeChanged(this, EventArgs.Empty);
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
            get { return m_Parent == null; }
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
			argument.m_Parent = this;

            ScopeChanged(this, EventArgs.Empty);
		}

        protected void ScopeChanged(object sender, EventArgs e)
		{
            ClearNameCache();
            if (m_Parent != null)
            {
                m_Parent.ScopeChanged(sender, e);
            }
		}

		public void SetName(int start, int length = -1)
		{
			if (m_Name != null)
				throw TypeNameParserCommon.NewError(TypeNameError.TypeNameAlreadyDefined, m_Text, -1, TypeNameToken.TypeName);

			m_Name = new TypeNameBlock(m_Text, TypeNameBlockType.TypeName, start, length);
            m_Name.Changed += ScopeChanged;

			ScopeChanged(this, EventArgs.Empty);
		}

		public void SetAssemblyName(int start, int length = -1)
		{
			if (m_AssemblyName != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyNameAlreadyDefined, m_Text, -1, TypeNameToken.AssemblyName);

			m_AssemblyName = new TypeNameBlock(m_Text, TypeNameBlockType.AssemblyName, start, length);
            m_AssemblyName.Changed += ScopeChanged;

			ScopeChanged(this, EventArgs.Empty);
		}

		public void SetVersion(int start, int length = -1)
		{
			if (m_Version != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyVersionAlreadyDefined, m_Text, -1, TypeNameToken.Version);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefineVersionBeforeAssemblyName, m_Text, -1, TypeNameToken.Version);

			m_Version = new TypeNameBlock(m_Text, TypeNameBlockType.Version, start, length);
            m_Version.Changed += ScopeChanged;

			ScopeChanged(this, EventArgs.Empty);
		}

		public void SetCulture(int start, int length = -1)
		{
			if (m_Culture != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyCultureAlreadyDefined, m_Text, -1, TypeNameToken.Culture);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefineCultureBeforeAssemblyName, m_Text, -1, TypeNameToken.Culture);

			m_Culture = new TypeNameBlock(m_Text, TypeNameBlockType.Culture, start, length);
            m_Culture.Changed += ScopeChanged;

			ScopeChanged(this, EventArgs.Empty);
		}

		public void SetPublicKeyToken(int start, int length = -1)
		{
			if (m_PublicKeyToken != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyPublicKeyTokenAlreadyDefined, m_Text, -1, TypeNameToken.PublicKeyToken);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefinePublicKeyTokenBeforeAssemblyName, m_Text, -1, TypeNameToken.PublicKeyToken);

			m_PublicKeyToken = new TypeNameBlock(m_Text, TypeNameBlockType.PublicKeyToken, start, length);
            m_PublicKeyToken.Changed += ScopeChanged;

			ScopeChanged(this, EventArgs.Empty);
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

        public Type ResolveType(bool throwOnError = false, bool ignoreCase = false)
        {
            if (m_Type != null)
                return m_Type;

            var cache = TypeNameParser.GetTypeNameCache(ignoreCase);

            var cacheKey = NameWithAssembly;
            if (cache.TryGetValue(cacheKey, out m_Type) && m_Type != null)
                return m_Type;

            m_Type = TypeNameParser.ResolveType(AssemblyQualifiedName, throwOnError, ignoreCase);

            if (m_Type != null)
                TypeNameParser.AddToTypeNameCache(cacheKey, m_Type);

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

        public object Clone()
        {
            var result = new TypeNameScope(Id, m_Text, m_ExpectedGenericsArgsCount);

            result.m_IsArray = m_IsArray;
            result.m_InBlock = m_InBlock;

            if (m_Name != null)
                result.m_Name = (TypeNameBlock)m_Name.Clone();

            if (m_AssemblyName != null)
				result.m_AssemblyName = (TypeNameBlock)m_AssemblyName.Clone();

            if (m_Culture != null)
				result.m_Culture = (TypeNameBlock)m_Culture.Clone();

            if (m_Version != null)
				result.m_Version = (TypeNameBlock)m_Version.Clone();

			if (m_PublicKeyToken != null)
				result.m_PublicKeyToken = (TypeNameBlock)m_PublicKeyToken.Clone();

            if (GenericsArguments != null)
                foreach (var arg in GenericsArguments)
                {
                    var clone = (TypeNameScope)arg.Clone();
                    result.GenericsArguments.Add(clone);

                    clone.m_Parent = result;
                }

			result.m_AQName = m_AQName;
			result.m_FullName = m_FullName;
			result.m_ShortName = m_ShortName;
			result.m_NameWithAssembly = m_NameWithAssembly;

			return result;
        }

        #endregion ToString

        #endregion Methods
    }

	#endregion TypeNameScope

}
