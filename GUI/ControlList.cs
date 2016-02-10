using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class ControlList : IList<Control>
	{
		#region Members

		/// <summary>The top-level GUI that contains this ControlList's owner.</summary>
		private Desktop topOwner;

		/// <summary>The top-level GUI that contains this ControlList's owner.</summary>
		public Desktop TopOwner
		{
			get { return topOwner; }
			set
			{
				foreach (Control c in List)
				{
					c.TopParent = value;
				}

				topOwner = value;
			}
		}

		/// <summary>The container that owns this ControlList.</summary>
		private Container Owner;

		/// <summary>The list that this ControlList wraps.</summary>
		private List<Control> List;

		/// <summary>The number of controls in this list.</summary>
		public int Count { get { return List.Count; } }

		/// <summary>Whether or not this list is read only.</summary>
		public bool IsReadOnly { get { return false; } }

		#endregion Members

		#region Constructors and Indexers

		/// <summary>Creates a new instance of ControlList.</summary>
		/// <param name="owner">The container that owns this ControlList.</param>
		public ControlList(Container owner)
		{
			List = new List<Control>();
			TopOwner = owner.TopParent;
			Owner = owner;
		}

		public Control this[int index]
		{
			get { return List[index]; }
			set
			{
				if (List[index] != null && List[index].OwningList == this)
				{
					List[index].OwningList = null;
					List[index].Parent = null;
					List[index].TopParent = null;
				}

				List[index] = value;

				value.OwningList = this;
				value.Parent = Owner;
				value.TopParent = TopOwner;
			}
		}

		#endregion Constructors

		#region Methods

		/// <summary>Inserts the provided control at the specified index.</summary>
		/// <param name="index">The index at which to insert the control.</param>
		/// <param name="item">The control to insert.</param>
		public void Insert(int index, Control item)
		{
			item.OwningList = this;
			item.Parent = Owner;
			item.TopParent = TopOwner;

			List.Insert(index, item);
		}

		/// <summary>Adds the provided control to this list.</summary>
		/// <param name="control">The control to add.</param>
		public void Add(Control item)
		{
			item.OwningList = this;
			item.Parent = Owner;
			item.TopParent = TopOwner;

			List.Add(item);
		}

		/// <summary>Returns the index of the specified control in this list.</summary>
		/// <param name="item">The control to search for.</param>
		/// <returns>The index of the specified control in this list.</returns>
		public int IndexOf(Control item)
		{
			for (int i = 0; i < List.Count; i++)
			{
				if (List[i] == item)
					return i;
			}

			return -1;
		}

		/// <summary>Removes the control at the specified index.</summary>
		/// <param name="index">The index of the control to remove.</param>
		public void RemoveAt(int index)
		{
			if (List[index].OwningList == this)
			{
				List[index].OwningList = null;
				List[index].Parent = null;
				List[index].TopParent = null;
			}

			List.RemoveAt(index);
		}

		/// <summary>Removes the specified control from this list.</summary>
		/// <param name="item">The control to remove.</param>
		public bool Remove(Control item)
		{
			int index = IndexOf(item);
			if (index == -1)
				return false;

			RemoveAt(index);
			return true;
		}

		/// <summary>Removes the specified controls from this list.</summary>
		/// <param name="items">The controls to remove.</param>
		public void Remove(IEnumerable<Control> items)
		{
			foreach (Control c in items)
			{
				Remove(c);
			}
		}

		/// <summary>Clears all controls from this list.</summary>
		public void Clear()
		{
			foreach (Control c in List)
			{
				if (c.OwningList == this)
				{
					c.OwningList = null;
					c.Parent = null;
					c.TopParent = null;
				}
			}

			List.Clear();
		}

		/// <summary>Determines whether or not the specified control is contained in this list.</summary>
		/// <param name="item">The control to search for.</param>
		/// <returns>Whether or not the specified control is contained in this list.</returns>
		public bool Contains(Control item)
		{
			foreach (Control c in List)
			{
				if (c == item)
					return true;
			}

			return false;
		}

		/// <summary>Copies this list to the provided array.</summary>
		/// <param name="array">The array to copy to.</param>
		/// <param name="arrayIndex">The index within the array to begin copying to.</param>
		public void CopyTo(Control[] array, int arrayIndex)
		{
			int max = Math.Min(List.Count, array.Length - arrayIndex);

			for (int i = 0; i < max; i++)
			{
				array[i + arrayIndex] = List[i];
			}
		}

		/// <summary>Returns an enumerator that allows iteration through this ControlList</summary>
		/// <returns>An enumerator that allows iteration through this ControlList</returns>
		public IEnumerator<Control> GetEnumeratorReverse()
		{
			for (int i = List.Count - 1; i >= 0; i--)
			{
				yield return List[i];
			}
		}

		#region IEnumerable<T> Members

		/// <summary>Returns an enumerator that allows iteration through this ControlList</summary>
		/// <returns>An enumerator that allows iteration through this ControlList</returns>
		public IEnumerator<Control> GetEnumerator()
		{
			foreach (Control c in List)
			{
				yield return c;
			}
		}

		#endregion IEnumerable<T> Members

		#region IEnumerable Members

		/// <summary>This will never be called; it is required by the IEnumerable interface</summary>
		/// <exception cref="Exception">Thrown when this function is called; this function should never be called</exception>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new Exception("System.Collections.IEnumerable.GetEnumerator() should never be called.");
		}

		#endregion IEnumerable Members

		#endregion Methods
	}
}
