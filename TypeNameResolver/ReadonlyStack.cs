using System;
using System.Collections;
using System.Collections.Generic;

namespace TypeNameResolver
{
	#region ReadonlyStack

	public class ReadonlyStack<T> : IStack<T>, IEnumerable<T>, IEnumerable, ICollection
	{
		#region Field Members

		private Stack<T> m_Stack;

		#endregion Field Members

		#region .Ctors

		public ReadonlyStack(Stack<T> stack)
		{
			m_Stack = stack;
		}

		#endregion .Ctors

		#region Properties

		public int Count
		{
			get { return m_Stack.Count; }
		}

		bool ICollection.IsSynchronized
		{
			get { return ((ICollection)m_Stack).IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return ((ICollection)m_Stack).SyncRoot; }
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(T item)
		{
			return m_Stack.Contains(item);
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			((ICollection)m_Stack).CopyTo(array, arrayIndex);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			m_Stack.CopyTo(array, arrayIndex);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)m_Stack).GetEnumerator();
		}

		public Stack<T>.Enumerator GetEnumerator()
		{
			return m_Stack.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return ((IEnumerable<T>)m_Stack).GetEnumerator();
		}

		public T Peek()
		{
			return m_Stack.Peek();
		}

		public T Pop()
		{
			throw new NotImplementedException();
		}

		public void Push(T item)
		{
			throw new NotImplementedException();
		}

		public T[] ToArray()
		{
			return m_Stack.ToArray();
		}

		public void TrimExcess()
		{
			m_Stack.TrimExcess();
		}

		#endregion Properties
	}

	#endregion ReadonlyStack
}
