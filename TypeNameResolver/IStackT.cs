using System.Collections;
using System.Collections.Generic;

namespace TypeNameResolver
{
	#region IStack<T>

	public interface IStack<T> : IEnumerable<T>, IEnumerable, ICollection
	{
		void Clear();
		bool Contains(T item);
		void CopyTo(T[] array, int arrayIndex);
		T Peek();
		T Pop();
		void Push(T item);
		T[] ToArray();
		void TrimExcess();
	}

	#endregion IStack<T>
}
