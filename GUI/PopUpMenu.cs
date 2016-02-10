using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class PopUpMenu : Control
	{
		#region Members

		/// <summary>The items to choose from in this menu.</summary>
		private List<string> items;

		/// <summary>The size of each item in this menu.</summary>
		private List<Vector2> itemSizes;

		/// <summary>The color of this PopUpMenu when hovered.</summary>
		public Color BackColorHover { get; set; }

		/// <summary>The color of the foreground of this PopUpMenu.</summary>
		public Color ForeColor { get; set; }

		/// <summary>The color of the foreground of this PopUpMenu when hovered.</summary>
		public Color ForeColorHover { get; set; }

		/// <summary>The font of the text in this PopUpMenu.</summary>
		private SpriteFont font;

		/// <summary>The font of the text in this PopUpMenu.</summary>
		public SpriteFont Font
		{
			get { return font; }
			set
			{
				font = value;

				if (items != null && itemSizes != null)
				{
					for (int i = 0; i < items.Count; i++)
					{
						updateItemSize(i);
					}
				}

				locSizeChgd();
			}
		}

		/// <summary>The padding on either side of the text in this PopUpMenu.</summary>
		private int sidePadding;

		/// <summary>The padding on either side of the text in this PopUpMenu.</summary>
		public int SidePadding
		{
			get { return sidePadding; }
			set
			{
				sidePadding = value;

				for (int i = 0; i < items.Count; i++)
				{
					updateItemSize(i);
				}

				locSizeChgd();
			}
		}

		/// <summary>The number of items in this menu.</summary>
		public int Count { get { return items.Count; } }

		/// <summary>Whether or not tabs should stop on this control.</summary>
		public override bool StopOnTab
		{
			get { return false; }
			set { throw new Exception("Cannot set StopOnTab for PopUpMenus."); }
		}

		#region Events

		/// <summary>Called when an option is selected.</summary>
		public event EventHandler OptionSelected;

		/// <summary>Called when an option is removed.</summary>
		public event EventHandler OptionRemoved;

		/// <summary>Called when an option is added.</summary>
		public event EventHandler OptionAdded;

		#endregion Events

		#endregion Members

		#region Constructors and Indexers

		/// <summary>Creates a new instance of PopUpMenu.</summary>
		public PopUpMenu()
			: base()
		{
			items = new List<string>();
			itemSizes = new List<Vector2>();
			BackColor = Desktop.DefPopUpMenuBackColor;
			BackColorHover = Desktop.DefPopUpMenuBackColorHover;
			ForeColor = Desktop.DefPopUpMenuForeColor;
			ForeColorHover = Desktop.DefPopUpMenuForeColorHover;
			font = Desktop.DefPopUpMenuFont;
			sidePadding = Desktop.DefPopUpMenuSidePadding;
			locSizeChgd();
		}

		/// <summary>Creates a new instance of PopUpMenu.</summary>
		/// <param name="toClone">The PopUpMenu to clone.</param>
		/// <param name="copyItems">Whether or not to copy the items over as well.</param>
		public PopUpMenu(PopUpMenu toClone, bool copyItems)
			: base(toClone)
		{
			if (copyItems)
			{
				items = new List<string>(toClone.items);
				itemSizes = new List<Vector2>(toClone.itemSizes);
			}
			else
			{
				items = new List<string>();
				itemSizes = new List<Vector2>();
			}
			BackColorHover = toClone.BackColorHover;
			ForeColor = toClone.ForeColor;
			ForeColorHover = toClone.ForeColorHover;
			font = toClone.font;
			sidePadding = toClone.sidePadding;

			locSizeChgd();
		}

		/// <summary>Gets or sets the item at the specified index.</summary>
		/// <param name="index">The index of the item to set.</param>
		/// <returns>The item at the specified index.</returns>
		public string this[int index]
		{
			get { return items[index]; }
			set
			{
				items[index] = value;
				updateItemSize(index);
				locSizeChgd();
			}
		}

		#endregion Constructors and Indexers

		#region Methods

		/// <summary>Updates the size of the specified item in the menu.</summary>
		/// <param name="index">The index of the item to update.</param>
		private void updateItemSize(int index)
		{
			if (Font == null)
			{
				itemSizes[index] = Vector2.Zero;
			}
			else
			{
				Vector2 temp = Font.MeasureString(items[index]);
				itemSizes[index] = new Vector2(temp.X + ((float)SidePadding * 2f), temp.Y);
			}
		}

		/// <summary>Adds the provided range of items to the list.</summary>
		/// <param name="range">The items to add.</param>
		public void addRange(IEnumerable<string> range)
		{
			int index = items.Count;
			foreach (string item in range)
			{
				items.Add(item);
				itemSizes.Add(Vector2.Zero);
				updateItemSize(index);
				OnOptionAdded(index);
				index++;
			}
			locSizeChgd();
		}

		/// <summary>Adds an item to the list.</summary>
		/// <param name="item">The item to add.</param>
		public void addItem(string item)
		{
			items.Add(item);
			itemSizes.Add(Vector2.Zero);
			updateItemSize(itemSizes.Count - 1);
			locSizeChgd();
			OnOptionAdded(items.Count - 1);
		}

		/// <summary>Inserts an item into the list.</summary>
		/// <param name="index">The index at which to insert the item.</param>
		/// <param name="item">The item to insert.</param>
		public void insertItem(int index, string item)
		{
			items.Insert(index, item);
			itemSizes.Insert(index, Vector2.Zero);
			updateItemSize(index);
			locSizeChgd();
			OnOptionAdded(index);
		}

		/// <summary>Inserts the provided range of items to the list.</summary>
		/// <param name="index">The index at which to insert the items.</param>
		/// <param name="range">The items to insert.</param>
		public void insertRange(int index, IEnumerable<string> range)
		{
			foreach (string item in range)
			{
				items.Insert(index, item);
				itemSizes.Insert(index, Vector2.Zero);
				updateItemSize(index);
				OnOptionAdded(index);
				index++;
			}
			locSizeChgd();
		}

		/// <summary>Finds the index of the specified item in the list.</summary>
		/// <param name="item">The item to search for.</param>
		public int indexOf(string item)
		{
			return items.IndexOf(item);
		}

		/// <summary>Removes the item at the specified index.</summary>
		/// <param name="index">The index at which to remove the item.</param>
		public void removeAt(int index)
		{
			items.RemoveAt(index);
			itemSizes.RemoveAt(index);
			locSizeChgd();
			OnOptionRemoved(index);
		}

		/// <summary>Removes the specified item from the list.</summary>
		/// <param name="item">The item to remove.</param>
		public void remove(string item)
		{
			removeAt(items.IndexOf(item));
		}

		/// <summary>Clears all items out of the menu.</summary>
		public void clear()
		{
			while (items.Count > 0)
			{
				items.RemoveAt(items.Count - 1);
				itemSizes.RemoveAt(itemSizes.Count - 1);
				OnOptionRemoved(items.Count);
			}

			locSizeChgd();
		}

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Draw(batch, new Rectangle(newLoc.X, newLoc.Y, Width, Height));

			Vector2 cursorPos = new Vector2(-1f, -1f);
			if (HasHover)
			{
				cursorPos.X = (float)(TopParent.CursorPos.X - newLoc.X);
				cursorPos.Y = (float)(TopParent.CursorPos.Y);
			}

			Vector2 pos = new Vector2((float)(newLoc.X + SidePadding), (float)newLoc.Y);
			for (int i = 0; i < items.Count; i++)
			{
				Rectangle newRect = new Rectangle((int)Math.Round((float)newLoc.X), (int)Math.Round(pos.Y), Width, (int)Math.Round(itemSizes[i].Y));
				batch.GraphicsDevice.ScissorRectangle = newRect;

				bool selected = cursorPos.Y >= pos.Y && cursorPos.Y < pos.Y + itemSizes[i].Y;

				if (selected)
					batch.Draw(TopParent.TBack, newRect, BackColorHover);

				batch.DrawString(Font, items[i], pos, (selected ? ForeColorHover : ForeColor));
				pos.Y += itemSizes[i].Y;
			}
		}

		/// <summary>Performs the specified mouse event on this control.</summary>
		/// <param name="evnt">The event to perform.</param>
		/// <param name="cursorPos">The position of the cursor at the time of the event.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Whether or not the event was performed.</returns>
		public override bool PerformMouseEvent(Desktop.Event evnt, Point cursorPos, Input.InpState input)
		{
			base.PerformMouseEvent(evnt, cursorPos, input);

			if (evnt == Desktop.Event.MouseLeftDown)
			{
				float cPos = (float)(cursorPos.Y - Top);
				float mPos = 0f;

				for (int i = 0; i < items.Count; i++)
				{
					if (cPos >= mPos && cPos < mPos + itemSizes[i].Y)
					{
						OnOptionSelected(i);
						break;
					}
					mPos += itemSizes[i].Y;
				}
			}

			return true;
		}

		/// <summary>Called when the location or size of this control is changed.</summary>
		protected override void locSizeChgd()
		{
			base.locSizeChgd();

			if (Font == null || items == null || itemSizes == null)
				return;

			Vector2 newSize = Vector2.Zero;

			foreach (Vector2 size in itemSizes)
			{
				newSize.X = Math.Max(newSize.X, size.X);
				newSize.Y += size.Y;
			}

			bounds.Width = (int)Math.Round(newSize.X);
			bounds.Height = (int)Math.Round(newSize.Y);
		}

		#region Events

		/// <summary>Called when an option is selected.</summary>
		/// <param name="option">The index of the selected option.</param>
		protected virtual void OnOptionSelected(int option)
		{
			if (OptionSelected != null)
				OptionSelected(this, new IndexEventArgs(option));
		}

		/// <summary>Called when an option is removed.</summary>
		/// <param name="option">The index of the removed option.</param>
		protected virtual void OnOptionRemoved(int option)
		{
			if (OptionRemoved != null)
				OptionRemoved(this, new IndexEventArgs(option));
		}

		/// <summary>Called when an option is added.</summary>
		/// <param name="option">The index of the added option.</param>
		protected virtual void OnOptionAdded(int option)
		{
			if (OptionAdded != null)
				OptionAdded(this, new IndexEventArgs(option));
		}

		#endregion Events

		#endregion Methods
	}
}
