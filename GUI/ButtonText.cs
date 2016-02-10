using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class ButtonText : Button
	{
		#region Members

		/// <summary>The color of the foreground of this ButtonText.</summary>
		public Color ForeColor { get; set; }

		/// <summary>The color of the foreground of this ButtonText when hovered.</summary>
		public Color ForeColorHover { get; set; }

		/// <summary>The color of the foreground of this ButtonText when pressed.</summary>
		public Color ForeColorPressed { get; set; }

		/// <summary>The color of the foreground of this ButtonText when in Pressed state (toggle only).</summary>
		public Color ForeColorP { get; set; }

		/// <summary>The color of the foreground of this ButtonText when hovered and in Pressed state (toggle only).</summary>
		public Color ForeColorHoverP { get; set; }

		/// <summary>The font of the text in this ButtonText.</summary>
		private SpriteFont font;

		/// <summary>The font of the text in this ButtonText.</summary>
		public virtual SpriteFont Font
		{
			get { return font; }
			set
			{
				font = value;
				locSizeChgd();
			}
		}

		/// <summary>The text to display on this ButtonText.</summary>
		private string text;

		/// <summary>The text to display on this ButtonText.</summary>
		public virtual string Text
		{
			get { return text; }
			set
			{
				text = value;
				locSizeChgd();
			}
		}

		/// <summary>The alignment of the text.</summary>
		private Desktop.Alignment textAlign;

		/// <summary>The alignment of the text.</summary>
		public Desktop.Alignment TextAlign
		{
			get { return textAlign; }
			set
			{
				textAlign = value;
				locSizeChgd();
			}
		}

		/// <summary>The padding on either side of the text in this ButtonText.</summary>
		private int sidePadding;

		/// <summary>The padding on either side of the text in this ButtonText.</summary>
		public int SidePadding
		{
			get { return sidePadding; }
			set
			{
				sidePadding = value;
				locSizeChgd();
			}
		}

		/// <summary>The position of the text within this ButtonText.</summary>
		private Vector2 textPos;

		/// <summary>The position of the text within this ButtonText.</summary>
		protected Vector2 TextPos
		{
			get { return textPos; }
			private set { textPos = value; }
		}

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of ButtonText.</summary>
		public ButtonText()
			: base()
		{
			TextPos = new Vector2();
			ForeColor = Desktop.DefButtonTextForeColor;
			ForeColorHover = Desktop.DefButtonTextForeColorHover;
			ForeColorPressed = Desktop.DefButtonTextForeColorPressed;
			ForeColorP = Desktop.DefButtonTextForeColorP;
			ForeColorHoverP = Desktop.DefButtonTextForeColorHoverP;
			textAlign = Desktop.DefButtonTextTextAlign;
			sidePadding = Desktop.DefButtonTextSidePadding;
			font = Desktop.DefButtonTextFont;
			text = string.Empty;
		}

		/// <summary>Creates a new instance of ButtonText.</summary>
		/// <param name="toClone">The ButtonText to clone.</param>
		public ButtonText(ButtonText toClone)
			: base(toClone)
		{
			TextPos = toClone.TextPos;
			ForeColor = toClone.ForeColor;
			ForeColorHover = toClone.ForeColorHover;
			ForeColorPressed = toClone.ForeColorPressed;
			ForeColorP = toClone.ForeColorP;
			ForeColorHoverP = toClone.ForeColorHoverP;
			font = toClone.Font;
			text = toClone.Text;
			textAlign = toClone.TextAlign;
			sidePadding = toClone.sidePadding;
			locSizeChgd();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Rectangle newRect = new Rectangle(newLoc.X, newLoc.Y, Width, Height);
			Vector2 textPos = new Vector2(TextPos.X + (float)newLoc.X, TextPos.Y + (float)newLoc.Y);

			batch.GraphicsDevice.ScissorRectangle = newRect;

			if (IsToggle && Pressed)
			{
				if (!HasHover)
				{
					batch.Draw(TopParent.TBack, newRect, BackColorP);
					batch.DrawString(Font, Text, textPos, ForeColorP);
				}
				else if (HasFocus && TopParent.IsMouseLeftDown)
				{
					batch.Draw(TopParent.TBack, newRect, BackColorPressed);
					batch.DrawString(Font, Text, textPos, ForeColorPressed);
				}
				else
				{
					batch.Draw(TopParent.TBack, newRect, BackColorHoverP);
					batch.DrawString(Font, Text, textPos, ForeColorHoverP);
				}
			}
			else
			{
				if (!HasHover)
				{
					Draw(batch, newRect);
					batch.DrawString(Font, Text, textPos, ForeColor);
				}
				else if (HasFocus && TopParent.IsMouseLeftDown)
				{
					batch.Draw(TopParent.TBack, newRect, BackColorPressed);
					batch.DrawString(Font, Text, textPos, ForeColorPressed);
				}
				else
				{
					batch.Draw(TopParent.TBack, newRect, BackColorHover);
					batch.DrawString(Font, Text, textPos, ForeColorHover);
				}
			}
		}

		/// <summary>Called when the location or size of this control is changed.</summary>
		protected override void locSizeChgd()
		{
			base.locSizeChgd();

			if (Font == null)
				return;

			Vector2 halfSize = new Vector2((float)Width / 2f, (float)Height / 2f);
			Vector2 textSize = (TextAlign == Desktop.Alignment.TopLeft) ? new Vector2() : Font.MeasureString(Text);

			switch (TextAlign)
			{
				case Desktop.Alignment.TopLeft:
				case Desktop.Alignment.TopCenter:
				case Desktop.Alignment.TopRight:
					textPos.Y = 0f;
					break;
				case Desktop.Alignment.CenterLeft:
				case Desktop.Alignment.Center:
				case Desktop.Alignment.CenterRight:
					textPos.Y = (float)(int)((halfSize.Y - textSize.Y / 2f) + .5f);
					break;
				case Desktop.Alignment.BottomLeft:
				case Desktop.Alignment.BottomCenter:
				case Desktop.Alignment.BottomRight:
					textPos.Y = (float)(int)(((float)Height - textSize.Y) + .5f);
					break;
				default:
					throw new Exception("Unknown alignment: " + TextAlign.ToString());
			}

			switch (TextAlign)
			{
				case Desktop.Alignment.TopLeft:
				case Desktop.Alignment.CenterLeft:
				case Desktop.Alignment.BottomLeft:
					textPos.X = (float)sidePadding;
					break;
				case Desktop.Alignment.TopCenter:
				case Desktop.Alignment.Center:
				case Desktop.Alignment.BottomCenter:
					textPos.X = (float)(int)((halfSize.X - textSize.X / 2f) + .5f);
					break;
				case Desktop.Alignment.TopRight:
				case Desktop.Alignment.CenterRight:
				case Desktop.Alignment.BottomRight:
					textPos.X = (float)(int)(((float)Width - sidePadding - textSize.X) + .5f);
					break;
				default:
					throw new Exception("Unknown alignment: " + TextAlign.ToString());
			}
		}

		#endregion Methods
	}
}
