using System;
using System.Collections.Generic;
using System.Text;

namespace TypeNameResolver
{
	#region TypeNameScope

	internal class TypeNameScope : ITypeNameScope
	{
		#region Field Members

		private string m_Text;
		private bool m_InBlock;
		private int m_NameBlockDepth;
		private int m_ExpectedGenericsArgsCount = -1;
		private int m_GenericsBlockDepth;

		#endregion Field Members

		#region .Ctors

		public TypeNameScope(int id, string text, bool isRoot = false, int genericsArgsCount = -1)
		{
			Id = id;
			m_Text = text;
			IsRoot = isRoot;
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
				var sBuilder = new StringBuilder(200);
				AppendAssemblyQualifiedName(sBuilder);
				return sBuilder.ToString();
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
				var sBuilder = new StringBuilder(200);
				AppendFullName(sBuilder);
				return sBuilder.ToString();
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

		public bool IsArray { get; set; }

		public bool IsGenericArgument
		{
			get { return Parent != null; }
		}

		public bool IsGenericType
		{
			get { return m_ExpectedGenericsArgsCount > 0; }
		}

		public bool IsRoot { get; private set; }

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

		public string ShortName
		{
			get
			{
				var sBuilder = new StringBuilder(200);
				AppendShortName(sBuilder);
				return sBuilder.ToString();
			}
		}

		#endregion Properties

		#region Methods

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
		}

		public void SetName(int start, int length = -1)
		{
			if (Name != null)
				throw TypeNameParserCommon.NewError(TypeNameError.TypeNameAlreadyDefined, m_Text, -1, TypeNameToken.TypeName);

			Name = new TypeNameBlock(m_Text, TypeNameBlockType.TypeName, start, length);
		}

		public void SetAssemblyName(int start, int length = -1)
		{
			if (AssemblyName != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyNameAlreadyDefined, m_Text, -1, TypeNameToken.AssemblyName);

			AssemblyName = new TypeNameBlock(m_Text, TypeNameBlockType.AssemblyName, start, length);
		}

		public void SetVersion(int start, int length = -1)
		{
			if (Version != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyVersionAlreadyDefined, m_Text, -1, TypeNameToken.Version);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefineVersionBeforeAssemblyName, m_Text, -1, TypeNameToken.Version);

			Version = new TypeNameBlock(m_Text, TypeNameBlockType.Version, start, length);
		}

		public void SetCulture(int start, int length = -1)
		{
			if (Culture != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyCultureAlreadyDefined, m_Text, -1, TypeNameToken.Culture);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefineCultureBeforeAssemblyName, m_Text, -1, TypeNameToken.Culture);

			Culture = new TypeNameBlock(m_Text, TypeNameBlockType.Culture, start, length);
		}

		public void SetPublicKeyToken(int start, int length = -1)
		{
			if (PublicKeyToken != null)
				throw TypeNameParserCommon.NewError(TypeNameError.AssemblyPublicKeyTokenAlreadyDefined, m_Text, -1, TypeNameToken.PublicKeyToken);

			if (AssemblyName == null)
				throw TypeNameParserCommon.NewError(TypeNameError.CannotDefinePublicKeyTokenBeforeAssemblyName, m_Text, -1, TypeNameToken.PublicKeyToken);

			PublicKeyToken = new TypeNameBlock(m_Text, TypeNameBlockType.PublicKeyToken, start, length);
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

		public void AppendAssemblyQualifiedName(StringBuilder sBuilder)
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
						GenericsArguments[i].AppendAssemblyQualifiedName(sBuilder);
					}
				}

				sBuilder.Append(']');
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

		public void AppendFullName(StringBuilder sBuilder)
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
						GenericsArguments[i].AppendFullName(sBuilder);
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

		public void AppendShortName(StringBuilder sBuilder)
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
						GenericsArguments[i].AppendShortName(sBuilder);
					}
				}

				sBuilder.Append(']');
			}
		}

		#endregion Append Name Methods

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
