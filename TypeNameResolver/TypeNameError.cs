namespace TypeNameResolver
{
    #region TypeNameError

    internal class TypeNameError
    {
        #region Field Members

        public int ErrorNo;
        public string Message;

		#endregion Field Members

		#region Errors

		public static readonly TypeNameError InvalidArray = new TypeNameError { Message = "Invalid array", ErrorNo = 1000 };
		public static readonly TypeNameError InvalidArrayStart = new TypeNameError { Message = "Invalid array start", ErrorNo = 1001 };
		public static readonly TypeNameError InvalidArrayEnd = new TypeNameError { Message = "Invalid array", ErrorNo = 1002 };

		public static readonly TypeNameError InvalidTypeName = new TypeNameError { Message = "Invalid type name", ErrorNo = 2000 };
		public static readonly TypeNameError InvalidTypeNameStart = new TypeNameError { Message = "Invalid type name start", ErrorNo = 2001 };
		public static readonly TypeNameError InvalidTypeNameEnd = new TypeNameError { Message = "Invalid type name end", ErrorNo = 2002 };
		public static readonly TypeNameError InvalidTypeNameChar = new TypeNameError { Message = "Invalid character for type name", ErrorNo = 2003 };
		public static readonly TypeNameError InvalidTypeNameTermination = new TypeNameError { Message = "Invalid type name termination", ErrorNo = 2004 };

		public static readonly TypeNameError InvalidGenerics = new TypeNameError { Message = "Invalid generics definition", ErrorNo = 3000 };
		public static readonly TypeNameError InvalidGenericsStart = new TypeNameError { Message = "Invalid generics start", ErrorNo = 3001 };
		public static readonly TypeNameError InvalidGenericsEnd = new TypeNameError { Message = "Invalid generics end", ErrorNo = 3002 };

		public static readonly TypeNameError InvalidGenericsArgBlockOpen = new TypeNameError { Message = "Invalid generics argument block open", ErrorNo = 3003 };
		public static readonly TypeNameError InvalidGenericsArgBlockClose = new TypeNameError { Message = "Invalid generics argument block close", ErrorNo = 3004 };

		public static readonly TypeNameError InvalidGenericsArgument = new TypeNameError { Message = "Invalid generics argument", ErrorNo = 3005 };
		public static readonly TypeNameError InvalidGenericsArgumentStart = new TypeNameError { Message = "Invalid generics argument start", ErrorNo = 3006 };
		public static readonly TypeNameError InvalidGenericsArgumentEnd = new TypeNameError { Message = "Invalid generics argumentend", ErrorNo = 3007 };

		public static readonly TypeNameError InvalidGenericsCount = new TypeNameError { Message = "Invalid generics argumet count", ErrorNo = 3006 };

		public static readonly TypeNameError InvalidAssemblyName = new TypeNameError { Message = "Invalid assembly name", ErrorNo = 4000 };
		public static readonly TypeNameError InvalidAssemblyVersion = new TypeNameError { Message = "Invalid assembly version", ErrorNo = 4001 };
		public static readonly TypeNameError InvalidAssemblyVersionValue = new TypeNameError { Message = "Invalid assembly version value", ErrorNo = 4002 };
		public static readonly TypeNameError InvalidAssemblyCulture = new TypeNameError { Message = "Invalid assembly culture", ErrorNo = 4003 };
		public static readonly TypeNameError InvalidAssemblyCultureValue = new TypeNameError { Message = "Invalid assembly culture value", ErrorNo = 4004 };
		public static readonly TypeNameError InvalidAssemblyPublicKeyToken = new TypeNameError { Message = "Invalid assembly public key context.Token", ErrorNo = 4005 };
		public static readonly TypeNameError InvalidAssemblyPublicKeyTokenValue = new TypeNameError { Message = "Invalid assembly public key context.Token value", ErrorNo = 4006 };
		public static readonly TypeNameError InvalidAssemblyProperty = new TypeNameError { Message = "Invalid assembly property", ErrorNo = 4007 };
		public static readonly TypeNameError InvalidAssemblyPropertyValue = new TypeNameError { Message = "Invalid assembly property value", ErrorNo = 4008 };

		public static readonly TypeNameError TypeNameAlreadyDefined = new TypeNameError { Message = "Type name already defined", ErrorNo = 5000 };

		public static readonly TypeNameError AssemblyNameAlreadyDefined = new TypeNameError { Message = "Assembly name is already defined", ErrorNo = 5001 };
		public static readonly TypeNameError AssemblyVersionAlreadyDefined = new TypeNameError { Message = "Assembly's version is already defined", ErrorNo = 5002 };
		public static readonly TypeNameError AssemblyCultureAlreadyDefined = new TypeNameError { Message = "Assembly's culture is already defined", ErrorNo = 5003 };
		public static readonly TypeNameError AssemblyPublicKeyTokenAlreadyDefined = new TypeNameError { Message = "Assembly's public key context.Token is already defined", ErrorNo = 5004 };

		public static readonly TypeNameError CannotDefineVersionBeforeAssemblyName = new TypeNameError { Message = "Can not define assembly version before assembly name", ErrorNo = 6000 };
		public static readonly TypeNameError CannotDefineCultureBeforeAssemblyName = new TypeNameError { Message = "Can not define assembly culture before assembly name", ErrorNo = 6001 };
		public static readonly TypeNameError CannotDefinePublicKeyTokenBeforeAssemblyName = new TypeNameError { Message = "Can not define assembly public key token before assembly name", ErrorNo = 6002 };

		public static readonly TypeNameError GenericsArgumentsCountExceeded = new TypeNameError { Message = "Generics argumets count exceeded", ErrorNo = 9000 };

		#endregion Errors
	}

	#endregion TypeNameError
}
