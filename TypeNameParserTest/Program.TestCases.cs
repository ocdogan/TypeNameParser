using System;
using System.Collections.Generic;
using System.Text;

#region Test classes without namespace

public class D<T, K> { }

public class L<T> { }

public class S<T> { }

public class St { }

public class Sh { }

public class I { }

public class Ln { }

public class Sb { }

#endregion Test classes without namespace

namespace TypeNameParserTest
{
    public partial class MainClass
    {
		private static Type[] GetTypesToTest()
		{
			return new Type[] {
				typeof(L<>),
				typeof(D<,>),
				typeof(List<>),
                typeof(Stack<>),
				typeof(Dictionary<,>),
                typeof(Tuple<,,>),

				typeof(St),
				typeof(Ln),
				typeof(L<St>),
				typeof(L<Ln>),

				typeof(byte),
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(ushort),
				typeof(uint),
				typeof(ulong),
				typeof(decimal),
				typeof(double),
				typeof(float),
				typeof(bool),
				typeof(string),

				typeof(byte[]),
				typeof(short[]),
				typeof(int[]),
				typeof(long[]),
				typeof(ushort[]),
				typeof(uint[]),
				typeof(ulong[]),
				typeof(decimal[]),
				typeof(double[]),
				typeof(float[]),
				typeof(bool[]),
				typeof(string[]),

				typeof(string[]),
				typeof(string[][]),
				typeof(string[][][]),
				typeof(string[,]),
				typeof(string[,,]),
				typeof(string[,,,]),
				typeof(string[,][]),
				typeof(string[,][,]),
				typeof(string[,][,][]),
				typeof(string[,][,][,]),
				typeof(string[][,]),
				typeof(string[][,][]),
				typeof(string[][,][,]),

				typeof(List<string>),
				typeof(List<string[]>),
				typeof(List<string[][]>),
				typeof(List<string[][][]>),
				typeof(List<string[,]>),
				typeof(List<string[,,]>),
				typeof(List<string[,,,]>),
				typeof(List<string[,][]>),
				typeof(List<string[,][,]>),
				typeof(List<string[,][,][]>),
				typeof(List<string[,][,][,]>),
				typeof(List<string[][,]>),
				typeof(List<string[][,][]>),
				typeof(List<string[][,][,]>),

				typeof(Dictionary<Int32, List<string>>),
				typeof(Dictionary<Int32, List<string[]>>),
				typeof(Dictionary<Int32, List<string[][]>>),
				typeof(Dictionary<Int32, List<string[][][]>>),
				typeof(Dictionary<Int32, List<string[,]>>),
				typeof(Dictionary<Int32, List<string[,,]>>),
				typeof(Dictionary<Int32, List<string[,,,]>>),
				typeof(Dictionary<Int32, List<string[,][]>>),
				typeof(Dictionary<Int32, List<string[,][,]>>),
				typeof(Dictionary<Int32, List<string[,][,][]>>),
				typeof(Dictionary<Int32, List<string[,][,][,]>>),
				typeof(Dictionary<Int32, List<string[][,]>>),
				typeof(Dictionary<Int32, List<string[][,][]>>),
				typeof(Dictionary<Int32, List<string[][,][,]>>),

				typeof(Stack<Dictionary<Int32, List<string>>>),
				typeof(Stack<Dictionary<Int32, List<string[]>>>),
				typeof(Stack<Dictionary<Int32, List<string[][]>>>),
				typeof(Stack<Dictionary<Int32, List<string[][][]>>>),
				typeof(Stack<Dictionary<Int32, List<string[,]>>>),
				typeof(Stack<Dictionary<Int32, List<string[,,]>>>),
				typeof(Stack<Dictionary<Int32, List<string[,,,]>>>),
				typeof(Stack<Dictionary<Int32, List<string[,][]>>>),
				typeof(Stack<Dictionary<Int32, List<string[,][,]>>>),
				typeof(Stack<Dictionary<Int32, List<string[,][,][]>>>),
				typeof(Stack<Dictionary<Int32, List<string[,][,][,]>>>),
				typeof(Stack<Dictionary<Int32, List<string[][,]>>>),
				typeof(Stack<Dictionary<Int32, List<string[][,][]>>>),
				typeof(Stack<Dictionary<Int32, List<string[][,][,]>>>),

				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][][]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,,]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,,,]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,][]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,][,]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,][]>>>>),
				typeof(Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,][,]>>>>),

				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][][]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,,]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,,,]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,][]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,][,]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,][]>>>>>),
				typeof(Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,][,]>>>>>),

				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][][]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,,]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,,,]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,][]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,][,]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,][]>>>>>, StringBuilder>),
				typeof(Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,][,]>>>>>, StringBuilder>),

				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][][]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,,]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,,,]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,][]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[,][,][,]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,][]>>>>>, StringBuilder>, double>),
				typeof(Tuple<decimal, Dictionary<Dictionary<List<short>, Dictionary<Int64[], Stack<Dictionary<Int32, List<string[][,][,]>>>>>, StringBuilder>, double>),
            };
		}
	}
}
