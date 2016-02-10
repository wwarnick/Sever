using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class Button : Control
	{
		#region Members

		/// <summary>The color of this Button when hovered.</summary>
		public Color BackColorHover { get; set; }

		/// <summary>The color of this Button when pressed.</summary>
		public Color BackColorPressed { get; set; }

		/// <summary>The color of this Button when in Pressed state (toggle only).</summary>
		public Color BackColorP { get; set; }

		/// <summary>The color of this Button when hovered and in Pressed state (toggle only).</summary>
		public Color BackColorHoverP { get; set; }

		/// <summary>Whether or not this Button should act as a toggle.</summary>
		private bool isToggle;

		/// <summary>Whether or not this Button should act as a toggle.</summary>
		public virtual bool IsToggle
		{
			get { return isToggle; }
			set { isToggle = value; }
		}

		/// <summary>Whether or not this Button is pressed (toggle only).</summary>
		private bool pressed;

		/// <summary>Whether or not this Button is pressed (toggle only).</summary>
		public virtual bool Pressed
		{
			get { return pressed; }
			set { pressed = value; }
		}

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Button.</summary>
		public Button()
			: base()
		{
			BackColor = Desktop.DefButtonBackColor;
			BackColorHover = Desktop.DefButtonBackColorHover;
			BackColorPressed = Desktop.DefButtonBackColorPressed;
			BackColorP = Desktop.DefButtonBackColorP;
			BackColorHoverP = Desktop.DefButtonBackColorHoverP;
			isToggle = false;
			pressed = false;
		}

		/// <summary>Creates a new instance of Button.</summary>
		/// <param name="toClone">The button to clone.</param>
		public Button(Button toClone)
			: base(toClone)
		{
			BackColorHover = toClone.BackColorHover;
			BackColorPressed = toClone.BackColorPressed;
			BackColorP = toClone.BackColorP;
			BackColorHoverP = toClone.BackColorHoverP;
			isToggle = toClone.IsToggle;
			pressed = false;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Rectangle newRect = new Rectangle(newLoc.X, newLoc.Y, Width, Height);

			batch.GraphicsDevice.ScissorRectangle = newRect;

			if (IsToggle && Pressed)
			{
				if (!HasHover)
					batch.Draw(TopParent.TBack, newRect, BackColorP);
				else if (HasFocus && TopParent.IsMouseLeftDown)
					batch.Draw(TopParent.TBack, newRect, BackColorPressed);
				else
					batch.Draw(TopParent.TBack, newRect, BackColorHoverP);
			}
			else
			{
				if (!HasHover)
					Draw(batch, newRect);
				else if (HasFocus && TopParent.IsMouseLeftDown)
					batch.Draw(TopParent.TBack, newRect, BackColorPressed);
				else
					batch.Draw(TopParent.TBack, newRect, BackColorHover);
			}
		}

		#region Events

		/// <summary>Called when the left mouse button is released from this control.</summary>
		protected override void OnMouseLeftUp()
		{
			pressed = IsToggle && !Pressed;

			base.OnMouseLeftUp();
		}

		#endregion Events

		#endregion Methods
	}
}
