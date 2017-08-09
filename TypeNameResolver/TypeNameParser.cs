﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeNameResolver
{
    public static class TypeNameParser
    {
        #region Constants

        private const int MaxAllowedGenericsCountCharLength = 3;

        #endregion Constants

        #region Static Members

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Assembly, bool>> s_AbsenceCache = 
            new ConcurrentDictionary<string, ConcurrentDictionary<Assembly, bool>>();
		
        private static readonly ConcurrentDictionary<string, Type> s_OrdinalTypeNameCache = new ConcurrentDictionary<string, Type>();
        private static readonly ConcurrentDictionary<string, Type> s_TypeNameCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        #endregion Static Members

		#region Resolve Methods

		internal static void AddToTypeNameCache(string typeName, Type type)
		{
			s_TypeNameCache[typeName] = type;
			s_OrdinalTypeNameCache[typeName] = type;
		}
		
        internal static ConcurrentDictionary<string, Type> GetTypeNameCache(bool ignoreCase)
        {
            return (ignoreCase ? s_TypeNameCache : s_OrdinalTypeNameCache);
        }

        public static Type ResolveType(string typeToResolve, bool throwOnError = false, bool ignoreCase = false)
		{
			var cache = GetTypeNameCache(ignoreCase);

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
                    var cacheKey = typeName + ", " + assembly.GetName().Name;
					if (cache.TryGetValue(cacheKey, out type) && type != null)
						return type;

					type = assembly.GetType(typeName, throwOnError, ignoreCase);
					if (type != null)
					    AddToTypeNameCache(cacheKey, type);

                    return type;
				}

				if (cache.TryGetValue(typeName, out type) && type != null)
					return type;

				ConcurrentDictionary<Assembly, bool> absence;
				if (!s_AbsenceCache.TryGetValue(typeName, out absence))
				{
					absence = new ConcurrentDictionary<Assembly, bool>();
					s_AbsenceCache[typeName] = absence;
				}

				var assemblies = AppDomain.CurrentDomain.GetAssemblies()
							  .Where(a => !absence.ContainsKey(a))
							  .ToDictionary(a => ignoreCase ? a.GetName().Name.ToLowerInvariant() : a.GetName().Name);

				if (assemblies.Count == 0)
					return null;

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

					type = possibileAssemblyNames
						.Where(name => assemblies.ContainsKey(name))
                        .Select(name => assemblies[name].GetType(typeName, false, ignoreCase2))
						.FirstOrDefault(t => t != null);

					if (type != null)
					{
						AddToTypeNameCache(typeName, type);
                        s_AbsenceCache.TryRemove(typeName, out absence);

                        return type;
					}

                    possibileAssemblyNames.ForEach(name =>
                    {
                        var asm = assemblies[name];
						absence[asm] = true;
						
                        assemblies.Remove(name);
                    });
				}

				if (nsEmpty)
				{
					var systemAssemblies = assemblies
                        .Where(kvp =>
                            String.Compare(kvp.Key, "mscorlib", ignoreCase2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0 ||
							String.Compare(kvp.Key, "system", ignoreCase2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0 ||
							kvp.Key.StartsWith("system.", ignoreCase2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                        .Select(kvp => kvp.Value)
                        .ToList();

                    systemAssemblies.ForEach(a => {
                        absence[a] = true;
                        assemblies.Remove(ignoreCase2 ? a.GetName().Name.ToLowerInvariant() : a.GetName().Name);
                    });
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
                    AddToTypeNameCache(typeName, type);
					s_AbsenceCache.TryRemove(typeName, out absence);
					
                    return type;
                }

				assemblies
					.Select(kvp => kvp.Value)
					.ToList()
					.ForEach(a => absence[a] = true);

				return null;
			}, throwOnError, ignoreCase);

			if (throwOnError && result == null)
				throw new TypeLoadException(String.Format("Type '{0}' cannot be found", typeToResolve));

			return result;
		}

		#endregion Resolve Methods

		#region Parse Methods

		public static TypeNameParseResult Parse(string str, bool debug = false)
        {
			if (String.IsNullOrEmpty(str))
				throw TypeNameParserCommon.NewError(TypeNameError.InvalidTypeName, str, 0, TypeNameToken.Undefined);

            using (var context = new ParseContext(str, debug))
            {
				Parse(context);
                return new TypeNameParseResult(context.Root, new ReadonlyStack<TypeNameParseTrace>(context.Trace));
            }
		}

		private static void Parse(ParseContext context)
        {
            while (context.Next())
            {
                switch (context.Token)
                {
                    case TypeNameToken.TypeNameStart:
                        {
                            context.EatWhitespace(throwErrorOnComplete: false);
                            context.CurrentScope.SetName(context.CurrentPos, -1);

                            if (!context.IsValidTypeNameChar(true))
                                throw context.DefaultError();

                            context.Previous();
                            context.Token = TypeNameToken.TypeName;
                        }
                        break;
                    case TypeNameToken.TypeName:
                        {
                            var ch = context.CurrentChar.Value;

                            if (ch == '`')
                            {
                                context.Previous();
                                context.Token = TypeNameToken.GenericsCountStart;
                            }
                            else if (ch == '[')
                            {
                                context.Previous();
                                context.Token = TypeNameToken.ArrayStart;
                            }
                            else if (ch == ',' || ch == ']' || char.IsWhiteSpace(ch))
                            {
                                context.CurrentScope.Name.End = context.CurrentPos;

                                context.Previous();
                                context.Token = TypeNameToken.TypeNameEnd;
                            }
                            else
                            {
                                if (!context.IsValidTypeNameChar())
                                    throw context.DefaultError();

                                if (context.AtLast)
                                    context.CurrentScope.Name.End = context.CurrentPos + 1;
                            }
                        }
                        break;
                    case TypeNameToken.TypeNameEnd:
                        {
                            context.EatWhitespace(throwErrorOnComplete: false);

                            if (context.CurrentPos > context.TextLength - 1)
                                continue;

                            switch (context.CurrentChar)
                            {
                                case ',':
                                    {
                                        context.Previous();
                                        context.Token = TypeNameToken.AssemblyNameStart;

                                        if (context.CurrentScope.IsGenericArgument && !context.CurrentScope.InNameBlock)
                                            context.Token = TypeNameToken.GenericsArgumentEnd;
                                    }
                                    break;
                                case ']':
                                    {
                                        if (!context.CurrentScope.IsGenericArgument)
                                            throw context.DefaultError();

                                        context.Previous();
                                        context.Token = TypeNameToken.GenericsArgumentEnd;

                                        if (context.CurrentScope.InNameBlock)
                                            context.Token = TypeNameToken.GenericsArgBlockClose;
                                    }
                                    break;
                                case '[':
                                    {
                                        if (!context.CurrentScope.IsGenericType || context.CurrentScope.HasGenericsArgument)
                                            throw context.DefaultError();

                                        context.CurrentScope.Name.End = context.CurrentPos;

                                        context.Previous();
                                        context.Token = TypeNameToken.GenericsStart;
                                    }
                                    break;
                                default:
                                    {
                                        if (!context.IsWhitespace())
                                            throw context.DefaultError();
                                    }
                                    break;
                            }
                        }
                        break;
                    case TypeNameToken.ArrayStart:
                        {
                            if (context.CurrentChar != '[')
                                throw context.DefaultError();

                            context.CurrentScope.IsArray = true;
                            context.Token = TypeNameToken.Array;
                        }
                        break;
                    case TypeNameToken.Array:
                        {
                            if (context.CurrentChar == ']')
                            {
                                context.Previous();
                                context.Token = TypeNameToken.ArrayEnd;
                            }
                            else if (context.CurrentChar != ',')
                                throw context.DefaultError();
                        }
                        break;
                    case TypeNameToken.ArrayEnd:
                        {
                            if (context.CurrentChar != ']' || !context.CurrentScope.IsArray)
                                throw context.DefaultError();

                            context.CurrentScope.Name.End = context.CurrentPos + 1;
                            context.Token = TypeNameToken.TypeName;
                        }
                        break;
                    case TypeNameToken.GenericsCountStart:
                        {
                            if (context.CurrentChar != '`')
                                throw context.DefaultError();

                            context.Token = TypeNameToken.GenericsCount;
                        }
                        break;
                    case TypeNameToken.GenericsCount:
                        {
                            if (context.CurrentChar < '0' || context.CurrentChar > '9')
                            {
                                context.Previous();
                                context.Token = TypeNameToken.GenericsCountEnd;
                            }
                            else if (context.AtLast)
                            {
                                context.CurrentScope.Name.End = context.CurrentPos + 1;
                            }
                        }
                        break;
                    case TypeNameToken.GenericsCountEnd:
                        {
							if (context.CurrentChar != '[')
							{
                                var genericsBase = (context.CurrentChar == ',') && 
	                                    context.CurrentScope.IsRoot &&
										!context.CurrentScope.IsGenericType &&
										!context.CurrentScope.HasGenericsArgument;

								if (!genericsBase)
									throw context.DefaultError();
							}

							context.CurrentScope.Name.End = context.CurrentPos;

                            var genericsArgsCount = ParseGenericsArgsCount(context);

                            context.CurrentScope.Name.End = context.CurrentPos;
                            context.CurrentScope.ExpectedGenericsArgsCount = genericsArgsCount;

                            context.Previous();
                            context.Token = TypeNameToken.TypeNameEnd;
                        }
                        break;
                    case TypeNameToken.GenericsStart:
                        {
                            if (context.CurrentChar != '[')
                                throw context.DefaultError();

                            context.CurrentScope.GenericsBlockDepth++;
                            context.Token = TypeNameToken.Generics;
                        }
                        break;
                    case TypeNameToken.Generics:
                        {
                            context.EatWhitespace();

                            switch (context.CurrentChar)
                            {
                                case ',':
                                    if (!context.CurrentScope.HasGenericsArgument)
                                        throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenericsArgument, context);

                                    context.Token = TypeNameToken.GenericsArgumentStart;
                                    break;
                                case '[':
                                    if (context.CurrentScope.HasGenericsArgument)
                                        throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenericsArgument, context);

                                    context.Previous();
                                    context.Token = TypeNameToken.GenericsArgumentStart;
                                    break;
                                case ']':
                                    context.Previous();
                                    context.Token = TypeNameToken.GenericsEnd;
                                    break;
                                default:
                                    context.Previous();
                                    context.Token = TypeNameToken.GenericsArgumentStart;
                                    break;
                            }
                        }
                        break;
                    case TypeNameToken.GenericsEnd:
                        {
                            if (context.CurrentChar != ']')
                                throw context.DefaultError();

                            context.CurrentScope.GenericsBlockDepth--;

                            if (context.Stack.Count == 0)
                                throw context.DefaultError();

                            if (context.InRoot || context.CurrentScope.InNameBlock)
                            {
                                context.Token = TypeNameToken.TypeNameEnd;
                            }
                            else
                            {
                                context.EndScope();
                                context.Token = TypeNameToken.Generics;
                            }
                        }
                        break;
                    case TypeNameToken.GenericsArgBlockOpen:
                        {
                            if (context.CurrentChar != '[' || context.CurrentScope.InNameBlock)
                                throw context.DefaultError();

                            context.CurrentScope.NameBlockDepth++;

                            context.Token = TypeNameToken.TypeNameStart;
                        }
                        break;
                    case TypeNameToken.GenericsArgBlockClose:
                        {
                            if (context.CurrentChar != ']' || !context.CurrentScope.InNameBlock)
                                throw context.DefaultError();

                            context.CurrentScope.NameBlockDepth--;

                            context.Token = TypeNameToken.GenericsArgumentEnd;
                        }
                        break;
                    case TypeNameToken.GenericsArgumentStart:
                        {
                            context.EatWhitespace();

                            if (context.CurrentChar == ']')
                            {
                                context.Previous();
                                context.Token = TypeNameToken.GenericsEnd;

                                continue;
                            }

                            context.StartScope();

                            context.Token = TypeNameToken.TypeNameStart;
                            if (context.CurrentChar == '[')
                                context.Token = TypeNameToken.GenericsArgBlockOpen;

                            context.Previous();
                        }
                        break;
                    case TypeNameToken.GenericsArgumentEnd:
                        {
							if (context.Stack.Count < 2)
								throw context.DefaultError();
                            
                            context.EatWhitespace();

                            if (!(context.CurrentChar == ',' || context.CurrentChar == ']'))
                                throw context.DefaultError();

                            context.EndScope();
                            context.Token = TypeNameToken.Generics;

                            context.Previous();
                        }
                        break;
                    case TypeNameToken.AssemblyNameStart:
                        {
                            if (context.CurrentChar != ',')
                                throw context.DefaultError();

                            context.Token = TypeNameToken.AssemblyName;
                        }
                        break;
                    case TypeNameToken.AssemblyPropertyStart:
                        {
                            context.EatWhitespace();

                            if (!context.IsWhitespace())
                            {
								switch (context.CurrentChar)
                                {
                                    case 'v':
                                    case 'V':
                                        context.Token = TypeNameToken.Version;
                                        break;
                                    case 'c':
                                    case 'C':
                                        context.Token = TypeNameToken.Culture;
                                        break;
                                    case 'p':
                                    case 'P':
                                        context.Token = TypeNameToken.PublicKeyToken;
                                        break;
                                    default:
                                        throw context.DefaultError();
                                }

                                context.Previous();
                            }
                        }
                        break;
                    case TypeNameToken.Version:
                    case TypeNameToken.Culture:
                    case TypeNameToken.PublicKeyToken:
                        {
							if (context.CurrentScope == null || context.CurrentScope.AssemblyName == null)
							{
								switch (context.Token)
								{
									case TypeNameToken.Version:
										throw context.CreateError(TypeNameError.CannotDefineVersionBeforeAssemblyName);
									case TypeNameToken.Culture:
                                        throw context.CreateError(TypeNameError.CannotDefineCultureBeforeAssemblyName);
									case TypeNameToken.PublicKeyToken:
                                        throw context.CreateError(TypeNameError.CannotDefinePublicKeyTokenBeforeAssemblyName);
								}
							}

							var tokenStr = context.TokenAsString;
                            var propertySample = TypeNameParserCommon.GetAssemblyPropertySampleText(context.Token);

                            var invalidToken = (context.CurrentPos > (context.TextLength - propertySample.Length) || 
                                                !TypeNameParserCommon.Equals(context.Text, tokenStr, context.CurrentPos));
                            
                            if (invalidToken)
                                throw context.DefaultError();

                            switch (context.Token)
                            {
                                case TypeNameToken.Version:
                                    context.Token = TypeNameToken.VersionValueStart;
                                    break;
                                case TypeNameToken.Culture:
                                    context.Token = TypeNameToken.CultureValueStart;
                                    break;
                                case TypeNameToken.PublicKeyToken:
                                    context.Token = TypeNameToken.PublicKeyTokenValueStart;
                                    break;
                            }

                            context.CurrentPos += tokenStr.Length - 1;
                        }
                        break;
                    case TypeNameToken.VersionValueStart:
                    case TypeNameToken.CultureValueStart:
                    case TypeNameToken.PublicKeyTokenValueStart:
                        {
                            context.EatWhitespace();

                            if (context.CurrentChar != '=')
                                throw context.DefaultError();

                            switch (context.Token)
                            {
                                case TypeNameToken.VersionValueStart:
                                    context.Token = TypeNameToken.VersionValue;
                                    break;
                                case TypeNameToken.CultureValueStart:
                                    context.Token = TypeNameToken.CultureValue;
                                    break;
                                case TypeNameToken.PublicKeyTokenValueStart:
                                    context.Token = TypeNameToken.PublicKeyTokenValue;
                                    break;
                            }
                        }
                        break;
                    case TypeNameToken.AssemblyName:
                    case TypeNameToken.VersionValue:
                    case TypeNameToken.CultureValue:
                    case TypeNameToken.PublicKeyTokenValue:
                        {
                            var isLastChar = false;

                            if (context.IsValidChar())
                            {
                                isLastChar = (context.AtLast);
                                if (!isLastChar)
                                    continue;
                            }

                            var ch = context.CurrentChar.Value;

                            if (!(ch == ']' || ch == ',' || (isLastChar && (context.Stack.Count == 1))))
                                throw context.DefaultError();

                            var startPos = TypeNameParserCommon.LastIndexOf(context.Token == TypeNameToken.AssemblyName ? ',' : '=', 
                                                       context.Text, context.CurrentPos - 1) + 1;

                            startPos = context.FindNonwhitespace(startPos, !context.InRoot);
                            var endPos = (context.FindNonwhitespaceBackward(context.CurrentPos - 1, !context.InRoot) + 1) + (isLastChar ? 1 : 0);

                            switch (context.Token)
                            {
                                case TypeNameToken.AssemblyName:
                                    context.CurrentScope.SetAssemblyName(startPos, endPos);
                                    break;
                                case TypeNameToken.VersionValue:
                                    context.CurrentScope.SetVersion(startPos, endPos);
                                    break;
                                case TypeNameToken.CultureValue:
                                    context.CurrentScope.SetCulture(startPos, endPos);
                                    break;
                                case TypeNameToken.PublicKeyTokenValue:
                                    context.CurrentScope.SetPublicKeyToken(startPos, endPos);
                                    break;
                            }

                            if (ch == ',')
                            {
                                context.Token = TypeNameToken.AssemblyPropertyStart;
                            }
                            else if (ch == ']')
                            {
                                context.Previous();
                                context.Token = TypeNameToken.TypeNameEnd;
                            }
                        }
                        break;
                }
            }
        }

        private static int ParseGenericsArgsCount(ParseContext context)
        {
            var start = TypeNameParserCommon.LastIndexOf('`', context.Text, context.CurrentPos - 1);
            if (start < 0)
                throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenericsCount, context);

            var len = context.CurrentPos - start - 1;
            if (len > MaxAllowedGenericsCountCharLength)
                throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenericsCount, context);

            if (int.TryParse(context.Text.Substring(start + 1, len), out int result))
                return result;

            throw TypeNameParserCommon.NewError(TypeNameError.InvalidGenericsCount, context);
        }

        #endregion Parse Methods
	}
}
