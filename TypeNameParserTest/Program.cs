using System;
using System.Collections.Generic;
using System.Linq;

using TypeNameResolver;

namespace TypeNameParserTest
{
    public partial class MainClass
    {
		#region TestMethod
		
        private enum TestMethod
		{
			Undefined = 0,
			AssemblyQualifiedName = 1,
			FullName = 2,
			ToString = 3,
		}

		#endregion TestMethod
		
        public static void Main(string[] args)
        {
            TypeNameParserTests();
        }

        private static bool IsUnix
        {
            get
            {
                var p = (int)Environment.OSVersion.Platform;
				return (p == 4) || (p == 128);                
            }
        }

        private static void TypeNameParserTests()
        {
            var types = GetTypesToTest();

            do
            {
                var methodToTest = SelectTestMethod();
                if ((int)methodToTest < 0)
                    return;

                #region Test all types

                Console.Clear();

                var testMethodName = methodToTest.ToString("F");
                var testMethodCaption = GetTestMethodCaption(methodToTest);

                Console.WriteLine(testMethodCaption);
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

                var failedTests = new List<int>();

                var fgColor = IsUnix ? ConsoleColor.Black : Console.ForegroundColor;
                try
                {
                    for (var i = 0; i < types.Length; i++)
                    {
                        var testNo = i + 1;
                        var type = types[i];
                        try
                        {
                            if (!TestType(type, methodToTest, testNo))
                                failedTests.Add(testNo);
                        }
                        catch (Exception e)
                        {
                            failedTests.Add(testNo);

                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("FAILED !!! ");
                            Console.ForegroundColor = fgColor;

                            var msg = e.Message;

                            var tne = e as TypeNameException;
                            if (tne != null)
                            {
                                var text = String.Empty;
                                if (tne.Position > 0 && !String.IsNullOrEmpty(tne.Text))
                                {
                                    text = tne.Text.Substring(0, Math.Min(tne.Text.Length, tne.Position + 1));
                                }

                                msg = String.Format("ErrorNo={0}, Position={1}, Message='{2}', Text='{3}'", tne.ErrorNo, tne.Position, msg, text);
                            }

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(String.Format("{0}, {1}", e.GetType().Name, msg));
                            Console.ForegroundColor = fgColor;
                        }
                        finally
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("------------------------------------------------");
                            Console.ForegroundColor = fgColor;
                        }
                    }
                }
                finally
                {
                    Console.ForegroundColor = fgColor;
                }

                #endregion Test all types

                #region Test result summary

                if (failedTests.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine();
                    Console.WriteLine("All tests PASSED.");
                    Console.WriteLine();
                    Console.ForegroundColor = fgColor;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red; 
                    Console.WriteLine();
                    Console.WriteLine("################################################");
                    Console.WriteLine("FAILED Tests:");
                    Console.WriteLine("################################################");
                    Console.WriteLine(String.Join(", ", failedTests));
                    Console.WriteLine("################################################");
                    Console.WriteLine();
                    Console.ForegroundColor = fgColor;
                }

                #endregion Test result summary

                Console.WriteLine();
                Console.WriteLine("Press ESC to exit, any key to continue...");

                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    return;
            }
            while (true);
        }

        private static bool TestType(Type typeToTest, TestMethod selectedTestMethod, int currentTestNo)
        {
            var typeName = (string)null;
            switch (selectedTestMethod)
            {
                case TestMethod.AssemblyQualifiedName:
                    typeName = typeToTest.AssemblyQualifiedName;
                    break;
                case TestMethod.FullName:
                    typeName = typeToTest.FullName;
                    break;
                case TestMethod.ToString:
                    typeName = typeToTest.ToString();
                    break;
            }

            return TestType(typeName, selectedTestMethod, currentTestNo);
        }

        private static bool TestType(string typeName, TestMethod selectedTestMethod, int currentTestNo)
        {
            var testMethodName = selectedTestMethod.ToString("F");

            var passed = false;
            var fgColor = IsUnix ? ConsoleColor.Black : Console.ForegroundColor;
            try
            {
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++");
                Console.WriteLine("Test no: " + currentTestNo);
                Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++");
                Console.ForegroundColor = fgColor;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Type's ");
                Console.WriteLine(testMethodName + ": ");
                Console.WriteLine("------------------------------------------------");
                Console.ForegroundColor = fgColor;

                Console.WriteLine(typeName);

                var result = TypeNameParser.Parse(typeName, true);
                var tn = (result != null ? result.Scope : null);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Parser's ");
                Console.WriteLine(testMethodName + ": ");
                Console.WriteLine("------------------------------------------------");
                Console.ForegroundColor = fgColor;

                var tns = (string)null;
                if (tn != null)
                {
                    switch (selectedTestMethod)
                    {
                        case TestMethod.AssemblyQualifiedName:
                            tns = tn.AssemblyQualifiedName;
                            break;
                        case TestMethod.FullName:
                            tns = tn.FullName;
                            break;
                        case TestMethod.ToString:
                            tns = tn.ShortName;
                            break;
                    }
                }

                Console.WriteLine(tns);

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("------------------------------------------------");
                Console.ForegroundColor = fgColor;

                passed = ((typeName ?? String.Empty).Trim() == tns);
                if (passed)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("PASSED");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("FAILED !!!");
                }
            }
            finally
            {
                Console.ForegroundColor = fgColor;
            }

            return passed;
        }

        private static string GetTestMethodCaption(TestMethod testMethod)
        {
            switch (testMethod)
            {
                case TestMethod.AssemblyQualifiedName:
                    return "Assembly Qualified Name Tests";
                case TestMethod.FullName:
                    return "Full Name Tests";
                case TestMethod.ToString:
                    return "To String Tests";
                default:
                    return "Undefined";
            }
        }

        private static TestMethod SelectTestMethod()
        {
            Console.Clear();

            var maxTestMethod = Enum.GetValues(typeof(TestMethod)).Cast<int>().Max();

            for (var i = 1; i < maxTestMethod + 1; i++)
            {
                Console.Write(i + ". ");
                Console.WriteLine(GetTestMethodCaption((TestMethod)i));
            }

            Console.WriteLine();
            Console.WriteLine("Please choose one of the tests or press ESC to exit.");

            do
            {
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                    break;

                var key = (int)(keyInfo.KeyChar - '0');
                if (key > 0 && key < maxTestMethod + 1)
                {
                    return (TestMethod)key;
                }
            }
            while (true);

            return (TestMethod)(-1);
        }
    }
}
