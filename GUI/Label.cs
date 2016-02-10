using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class Label : Control
	{
		#region Members

		/// <summary>The color of the foreground of this Label.</summary>
		public Color ForeColor { get; set; }

		/// <summary>The font of the text in this Label.</summary>
		private SpriteFont font;

		/// <summary>The font of the text in this Label.</summary>
		public SpriteFont Font
		{
			get { return font; }
			set
			{
				font = value;
				locSizeChgd();
			}
		}

		/// <summary>The text to display on this Label.</summary>
		private string text;

		/// <summary>The text to display on this Label.</summary>
		public string Text
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

		/// <summary>The position of the text within this Label.</summary>
		private Vector2 textPos;

		/// <summary>Whether or not this Label should resize itself to fit the text inside it.</summary>
		private bool autoSize;

		/// <summary>Whether or not this Label should resize itself to fit the text inside it.</summary>
		public bool AutoSize
		{
			get { return autoSize; }
			set
			{
				autoSize = value;
				locSizeChgd();
			}
		}

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Label.</summary>
		public Label()
			: base()
		{
			textPos = new Vector2();
			ForeColor = Desktop.DefLabelForeColor;
			font = Desktop.DefLabelFont;
			text = string.Empty;
			textAlign = Desktop.DefLabelTextAlign;
			DrawBack = false;
			Ignore = true;
			autoSize = Desktop.DefLabelAutoSize;
		}

		/// <summary>Creates a new instance of Label.</summary>
		/// <param name="toClone">The Label to clone.</param>
		public Label(Label toClone)
			: base(toClone)
		{
			ForeColor = toClone.ForeColor;
			font = toClone.Font;
			text = toClone.Text;
			textAlign = toClone.TextAlign;
			textPos = toClone.textPos;
			autoSize = toClone.autoSize;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Rectangle newRect = new Rectangle(newLoc.X, newLoc.Y, Width, Height);
			Vector2 tPos = new Vector2(textPos.X + (float)newLoc.X, textPos.Y + (float)newLoc.Y);

			batch.GraphicsDevice.ScissorRectangle = newRect;

			Draw(batch, newRect);
			batch.DrawString(Font, Text, tPos, ForeColor);
		}

		/// <summary>Called when the location or size of this control is changed.</summary>
		protected override void locSizeChgd()
		{
			base.locSizeChgd();

			if (Font == null)
				return;

			Vector2 textSize = (!AutoSize && TextAlign == Desktop.Alignment.TopLeft) ? new Vector2() : Font.MeasureString(Text);

			if (AutoSize)
			{
				bounds.Width = (int)Math.Round(textSize.X);
				bounds.Height = (int)Math.Round(textSize.Y);
			}

			Vector2 halfSize = new Vector2((float)Width / 2f, (float)Height / 2f);

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
					textPos.X = 0f;
					break;
				case Desktop.Alignment.TopCenter:
				case Desktop.Alignment.Center:
				case Desktop.Alignment.BottomCenter:
					textPos.X = (float)(int)((halfSize.X - textSize.X / 2f) + .5f);
					break;
				case Desktop.Alignment.TopRight:
				case Desktop.Alignment.CenterRight:
				case Desktop.Alignment.BottomRight:
					textPos.X = (float)(int)(((float)Width - textSize.X) + .5f);
					break;
				default:
					throw new Exception("Unknown alignment: " + TextAlign.ToString());
			}
		}

		#endregion Methods
	}
}
