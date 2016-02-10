using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class LListNode<T>
	{
		#region Members

		/// <summary>The value contained in this linked list node.</summary>
		public T Value { get; set; }

		/// <summary>The next node in the linked list.</summary>
		public LListNode<T> Next { get; set; }

		/// <summary>The previous node in the linked list.</summary>
		public LListNode<T> Previous { get; set; }

		/// <summary>The linked list that this node belongs to.</summary>
		public LList<T> List { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of LListNode.</summary>
		/// <param name="value">The value contained in this linked list node.</param>
		public LListNode(T value)
		{
			Value = value;
			Next = null;
			Previous = null;
			List = null;
		}

		#endregion Constructors
	}
}
