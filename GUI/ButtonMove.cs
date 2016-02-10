using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class ButtonMove : Button
	{
		#region Members

		/// <summary>The vector from the upper-left corner at which the cursor first pressed the button before it started dragging.</summary>
		private Point dragOffset;

		/// <summary>Called when this ButtonMove is dragged by the mouse.</summary>
		public event EventHandler MouseDrag;

		/// <summary>Whether or not this Button should act as a toggle.</summary>
		public override bool IsToggle
		{
			get { return false; }
			set { throw new Exception("Cannot set IsToggle on a ButtonMove."); }
		}

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of ButtonMove.</summary>
		public ButtonMove()
			: base()
		{
			dragOffset = new Point();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Called when the cursor is moved while this control is in focus.</summary>
		public override void mouseMove()
		{
			base.mouseMove();

			if (TopParent.IsMouseLeftDown)
			{
				Location += TopParent.CursorPos - (getScreenPos() + dragOffset);
				OnMouseDrag();
			}
		}

		/// <summary>Called when the left mouse button is pressed down on this control.</summary>
		protected override void OnMouseLeftDown()
		{
			dragOffset = TopParent.CursorPos - getScreenPos();
			base.OnMouseLeftDown();
		}

		/// <summary>Called when this ButtonMove is dragged by the mouse.</summary>
		private void OnMouseDrag()
		{
			if (MouseDrag != null)
				MouseDrag(this, new EventArgs());
		}

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Rectangle newRect = new Rectangle(newLoc.X, newLoc.Y, Width, Height);

			batch.GraphicsDevice.ScissorRectangle = newRect;

			if (HasFocus && TopParent.IsMouseLeftDown)
				batch.Draw(TopParent.TBack, newRect, BackColorPressed);
			else if (HasHover)
				batch.Draw(TopParent.TBack, newRect, BackColorHover);
			else
				Draw(batch, newRect);
		}

		#endregion Methods
	}
}
