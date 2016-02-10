using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public abstract class ListBox<T>
		: Container
	{
		#region Members

		/// <summary>The list of items.</summary>
		protected List<T> items;

		/// <summary>The height of each item in the list.</summary>
		protected List<int> itemHeight;

		/// <summary>The color of this ListBox when hovered.</summary>
		public Color BackColorHover { get; set; }

		/// <summary>The color of this ListBox when Selected.</summary>
		public Color BackColorSelected { get; set; }

		/// <summary>The ScrollBar in use.</summary>
		protected ScrollBar sclScroll;

		/// <summary>The currently selected index.</summary>
		public int SelectedIndex;

		/// <summary>The currently selected item.</summary>
		public T SelectedItem
		{
			get { return items[SelectedIndex]; }
			set { SelectedIndex = items.IndexOf(value); }
		}

		/// <summary>Whether or not this control distinguishes double clicks from single clicks.</summary>
		public override bool AcceptDoubleClicks { get { return true; } }

		/// <summary>Called when the selected index is changed.</summary>
		public event EventHandler SelectedIndexChanged;

		#endregion Members

		#region Constructors and Indexers

		/// <summary>Creates a new instance of ListBox.</summary>
		public ListBox()
			: base()
		{
			items = new List<T>();
			itemHeight = new List<int>();
			BackColor = Desktop.DefListBoxBackColor;
			BackColorHover = Desktop.DefListBoxBackColorHover;
			BackColorSelected = Desktop.DefListBoxBackColorSelected;
			sclScroll = new ScrollBar();
			sclScroll.Width = 20;
			Controls.Add(sclScroll);
			SelectedIndex = -1;
		}

		/// <summary>Creates a new instance of ListBox.</summary>
		/// <param name="toClone">The ListBox to clone.</param>
		/// <param name="copyItems">Whether or not to copy the items.</param>
		public ListBox(ListBox<T> toClone, bool copyItems)
			: base(toClone)
		{
			if (copyItems)
			{
				items = new List<T>(toClone.items);
				itemHeight = new List<int>(toClone.itemHeight);
			}
			else
			{
				items = new List<T>();
				itemHeight = new List<int>();
			}

			BackColorHover = toClone.BackColorHover;
			BackColorSelected = toClone.BackColorSelected;
			sclScroll = new ScrollBar();
			sclScroll.Width = 20;
			Controls.Add(sclScroll);
			SelectedIndex = -1;
		}

		/// <summary>Gets or sets the item at the specified index.</summary>
		/// <param name="index">The index of the item to set.</param>
		/// <returns>The item at the specified index.</returns>
		public T this[int index]
		{
			get { return items[index]; }
			set
			{
				items[index] = value;
				refreshItemSizes();
			}
		}

		#endregion Constructors and Indexers

		#region Methods

		/// <summary>Called when the location or size of this control is changed.</summary>
		protected override void locSizeChgd()
		{
			base.locSizeChgd();

			sclScroll.Bounds = new Rectangle(Width - 20, 0, 20, Height);
			sclScroll.ActiveRange = Height;
		}

		/// <summary>Adds an item to the list.</summary>
		/// <param name="item">The item to add.</param>
		public void Add(T item)
		{
			items.Add(item);
			itemHeight.Add(1);
			refreshItemSizes();
		}

		/// <summary>Adds a range of items to the list.</summary>
		/// <param name="range">The range of items to add.</param>
		public void AddRange(IEnumerable<T> range)
		{
			foreach (T item in range)
			{
				items.Add(item);
				itemHeight.Add(1);
			}
			refreshItemSizes();
		}

		/// <summary>Inserts an item at the specified index in the list.</summary>
		/// <param name="index">The index at which to insert the item.</param>
		/// <param name="item">The item to insert.</param>
		public void Insert(int index, T item)
		{
			items.Insert(index, item);
			itemHeight.Insert(index, 1);
			if (index <= SelectedIndex)
				SelectedIndex++;
			refreshItemSizes();
		}

		/// <summary>Inserts a range of items into the list.</summary>
		/// <param name="index">The index at which to insert the items.</param>
		/// <param name="range">The range to insert.</param>
		public void InsertRange(int index, IEnumerable<T> range)
		{
			items.InsertRange(index, range);
			
			int count = 0;
			foreach (T item in range)
			{
				itemHeight.Insert(index, 1);
				count++;
			}

			if (index <= SelectedIndex)
				SelectedIndex += count;

			refreshItemSizes();
		}

		/// <summary>Removes the item at the specified index in the list.</summary>
		/// <param name="index">The index of the item to remove.</param>
		public void RemoveAt(int index)
		{
			items.RemoveAt(index);
			itemHeight.RemoveAt(index);
			if (items.Count == 0 || index == SelectedIndex)
			{
				SelectedIndex = -1;
				OnSelectedIndexChanged();
			}
			else if (index < SelectedIndex)
				SelectedIndex--;
			refreshItemSizes();
		}

		/// <summary>Removes the specified item from the list.</summary>
		/// <param name="item">The item to remove.</param>
		public void Remove(T item)
		{
			RemoveAt(items.IndexOf(item));
		}

		/// <summary>Removes a range of items from the list.</summary>
		/// <param name="index">The index of the beginning of the range to remove.</param>
		/// <param name="num">The number of items in the range to remove.</param>
		public void RemoveRange(int index, int num)
		{
			items.RemoveRange(index, num);
			itemHeight.RemoveRange(index, num);

			if (items.Count == 0 || (SelectedIndex >= index && SelectedIndex < index + num))
			{
				SelectedIndex = -1;
				OnSelectedIndexChanged();
			}
			else if (SelectedIndex > index)
				SelectedIndex -= num;

			refreshItemSizes();
		}

		/// <summary>Clears all items from the list.</summary>
		public void Clear()
		{
			items.Clear();
			itemHeight.Clear();
			refreshItemSizes();
			if (SelectedIndex != -1)
			{
				SelectedIndex = -1;
				OnSelectedIndexChanged();
			}
			else
				SelectedIndex = -1;
		}

		/// <summary>Returns the index of the specified item in the list.</summary>
		/// <param name="item">The item to search for.</param>
		/// <returns>The index of the specified item in the list.</returns>
		public int IndexOf(T item)
		{
			return items.IndexOf(item);
		}

		/// <summary>Refreshes the size of each item in the list.</summary>
		protected void refreshItemSizes()
		{
			int totalHeight = 0;

			for (int i = 0; i < items.Count; i++)
			{
				refreshItemSize(i);
				totalHeight += itemHeight[i];
			}

			sclScroll.Range = totalHeight;
		}

		/// <summary>Refreshes the size of the specified item.</summary>
		/// <param name="index">The index of the item to refresh.</param>
		protected abstract void refreshItemSize(int index);

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Draw(batch, new Rectangle(newLoc.X, newLoc.Y, sclScroll.Left, Height));

			int curPoint = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (curPoint <= sclScroll.Scroll + Height)
				{
					if (curPoint + itemHeight[i] >= sclScroll.Scroll)
					{
						Rectangle rect = new Rectangle(newLoc.X, newLoc.Y + curPoint - sclScroll.Scroll, sclScroll.Left, itemHeight[i]);
						int scissorTop = Math.Max(rect.Y, newLoc.Y);
						Rectangle scissorRect = new Rectangle(rect.X, scissorTop, rect.Width, Math.Min(rect.Y + rect.Height, newLoc.Y + Height) - scissorTop);

						bool selected = (SelectedIndex == i);
						bool hovered = HasHover && scissorRect.Contains(TopParent.CursorPos);

						batch.GraphicsDevice.ScissorRectangle = scissorRect;
						if (selected)
							batch.Draw(TopParent.TBack, scissorRect, BackColorSelected);
						else if (hovered)
							batch.Draw(TopParent.TBack, scissorRect, BackColorHover);

						DrawItem(i, rect, selected, hovered, batch);
					}
				}
				else
					break;

				curPoint += itemHeight[i];
			}

			sclScroll.Draw(batch);
		}

		/// <summary>Draws the specified item.</summary>
		/// <param name="index">The index of the item to draw.</param>
		/// <param name="rect">The rectangle to draw the item in.</param>
		/// <param name="selected">Whether or not the item is selected.</param>
		/// <param name="hovered">Whether or not the item is selected.</param>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		protected abstract void DrawItem(int index, Rectangle rect, bool selected, bool hovered, SpriteBatch batch);

		/// <summary>Performs the specified mouse event on this control.</summary>
		/// <param name="evnt">The event to perform.</param>
		/// <param name="cursorPos">The position of the cursor at the time of the event.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Whether or not the event was performed.</returns>
		public override bool PerformMouseEvent(Desktop.Event evnt, Point cursorPos, Input.InpState input)
		{
			if (evnt == Desktop.Event.MouseLeftDown || evnt == Desktop.Event.MouseLeftDoubleClick)
			{
				int cPos = cursorPos.Y - Top;
				int mPos = -sclScroll.Scroll;

				for (int i = 0; i < items.Count; i++)
				{
					if (cPos >= mPos && cPos < mPos + itemHeight[i])
					{
						SelectedIndex = (SelectedIndex != i || evnt == Desktop.Event.MouseLeftDoubleClick) ? i : -1;
						OnSelectedIndexChanged();
						break;
					}
					mPos += itemHeight[i];
				}
			}

			base.PerformMouseEvent(evnt, cursorPos, input);

			return true;
		}

		/// <summary>Called when the selected index is changed.</summary>
		private void OnSelectedIndexChanged()
		{
			if (SelectedIndexChanged != null)
				SelectedIndexChanged(this, new EventArgs());
		}

		#endregion Methods
	}
}
