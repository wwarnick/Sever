using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class ListBoxText : ListBox<string>
	{
		#region Members

		/// <summary>The font of the text in this ListBoxText.</summary>
		private SpriteFont font;

		/// <summary>The font of the text in this ListBoxText.</summary>
		public SpriteFont Font
		{
			get { return font; }
			set
			{
				font = value;
				refreshItemSizes();
			}
		}

		/// <summary>The fore color of this ListBoxText.</summary>
		public Color ForeColor { get; set; }

		/// <summary>The fore color of this ListBoxText when hovered.</summary>
		public Color ForeColorHover { get; set; }

		/// <summary>The fore color of this ListBoxText when Selected.</summary>
		public Color ForeColorSelected { get; set; }

		/// <summary>The padding on either side of the text in this ListBoxText.</summary>
		public int SidePadding { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of ListBoxText</summary>
		public ListBoxText()
			: base()
		{
			font = Desktop.DefListBoxTextFont;
			ForeColor = Desktop.DefListBoxTextForeColor;
			ForeColorHover = Desktop.DefListBoxTextForeColorHover;
			ForeColorSelected = Desktop.DefListBoxTextForeColorSelected;
			SidePadding = Desktop.DefListBoxTextSidePadding;
		}

		/// <summary>Creates a new instance of ListBox.</summary>
		/// <param name="toClone">The ListBox to clone.</param>
		/// <param name="copyItems">Whether or not to copy the items.</param>
		public ListBoxText(ListBoxText toClone, bool copyItems)
			: base(toClone, copyItems)
		{
			font = toClone.font;
			ForeColor = toClone.ForeColor;
			ForeColorHover = toClone.ForeColorHover;
			ForeColorSelected = toClone.ForeColorSelected;
			SidePadding = toClone.SidePadding;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Refreshes the size of the specified item.</summary>
		/// <param name="index">The index of the item to refresh.</param>
		protected override void refreshItemSize(int index)
		{
			itemHeight[index] = (Font != null)
				? ((int)(Font.MeasureString(items[index]).Y + .5f))
				: 1;
		}

		/// <summary>Draws the specified item.</summary>
		/// <param name="index">The index of the item to draw.</param>
		/// <param name="rect">The rectangle to draw the item in.</param>
		/// <param name="selected">Whether or not the item is selected.</param>
		/// <param name="hovered">Whether or not the item is selected.</param>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		protected override void DrawItem(int index, Rectangle rect, bool selected, bool hovered, SpriteBatch batch)
		{
			batch.DrawString(Font, items[index], new Vector2((float)(rect.X + SidePadding), (float)(rect.Y)), selected ? ForeColorSelected : hovered ? ForeColorHover : ForeColor);
		}

		#endregion Methods
	}
}
