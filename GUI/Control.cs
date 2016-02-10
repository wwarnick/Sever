using Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class Control
	{
		#region Members

		/// <summary>The control that owns this control.</summary>
		public Control Parent { get; set; }

		/// <summary>The top-level Desktop that contains this control and its parents.</summary>
		public virtual Desktop TopParent { get; set; }

		/// <summary>The list that contains this control.</summary>
		public ControlList OwningList { get; set; }

		/// <summary>A Rectangle representing this control's position and size.</summary>
		protected Rectangle bounds;

		/// <summary>A Rectangle representing this control's position and size.</summary>
		public Rectangle Bounds
		{
			get { return bounds; }
			set
			{
				bounds = value;
				locSizeChgd();
			}
		}

		/// <summary>The location of this control.</summary>
		public Point Location
		{
			get { return bounds.Location; }
			set
			{
				bounds.X = value.X;
				bounds.Y = value.Y;
				locSizeChgd();
			}
		}

		/// <summary>The left edge of this control.</summary>
		public int Left
		{
			get { return bounds.X; }
			set
			{
				bounds.X = value;
				locSizeChgd();
			}
		}

		/// <summary>The top edge of this control.</summary>
		public int Top
		{
			get { return bounds.Y; }
			set
			{
				bounds.Y = value;
				locSizeChgd();
			}
		}

		/// <summary>The width of this control.</summary>
		public int Width
		{
			get { return bounds.Width; }
			set
			{
				bounds.Width = value;
				locSizeChgd();
			}
		}

		/// <summary>The height of this control.</summary>
		public int Height
		{
			get { return bounds.Height; }
			set
			{
				bounds.Height = value;
				locSizeChgd();
			}
		}

		/// <summary>The right edge of this control.</summary>
		public int Right
		{
			get { return bounds.Right; }
			set { Width = value - bounds.X; }
		}

		/// <summary>The bottom edge of this control.</summary>
		public int Bottom
		{
			get { return bounds.Bottom; }
			set { Height = value - bounds.Y; }
		}

		/// <summary>The center of this control.</summary>
		public Point Center { get { return bounds.Center; } }

		/// <summary>Whether or not this control has the focus.</summary>
		public bool HasFocus { get { return TopParent != null && TopParent.Focused == this; } }

		/// <summary>Whether or not the cursor is hovering over this control.</summary>
		public bool HasHover { get { return TopParent != null && TopParent.Hovered == this; } }

		/// <summary>Whether or not this control should be ignored by user input.</summary>
		public bool Ignore { get; set; }

		/// <summary>Whether or not this control should be displayed.</summary>
		private bool visible;

		/// <summary>Whether or not this control should be displayed.</summary>
		public bool Visible
		{
			get { return visible; }
			set
			{
				if (value == false)
				{
					if (HasFocus)
						TopParent.Focused = null;

					if (HasHover)
						TopParent.Hovered = null;
				}

				visible = value;
			}
		}

		/// <summary>Whether or not to draw the background texture of this control.</summary>
		public bool DrawBack { get; set; }

		/// <summary>The color of this control.</summary>
		public Color BackColor { get; set; }

		/// <summary>Whether or not tabs should stop on this control.</summary>
		private bool stopOnTab;

		/// <summary>Whether or not tabs should stop on this control.</summary>
		public virtual bool StopOnTab
		{
			get { return stopOnTab; }
			set { stopOnTab = value; }
		}

		/// <summary>Whether or not this control distinguishes double clicks from single clicks.</summary>
		public virtual bool AcceptDoubleClicks { get { return false; } }

		#region Events

		/// <summary>Called when the left mouse button is pressed down on this control.</summary>
		public event EventHandler MouseLeftDown;

		/// <summary>Called when the left mouse button is released from this control.</summary>
		public event EventHandler MouseLeftUp;

		/// <summary>Called when the left mouse button double-clicks this control.</summary>
		public event EventHandler MouseLeftDoubleClick;

		/// <summary>Called when the right mouse button is pressed down on this control.</summary>
		public event EventHandler MouseRightDown;

		/// <summary>Called when the right mouse button is released from this control.</summary>
		public event EventHandler MouseRightUp;

		/// <summary>Called when the right mouse button double-clicks this control.</summary>
		public event EventHandler MouseRightDoubleClick;

		/// <summary>Called when this control loses focus.</summary>
		public event EventHandler FocusLost;

		/// <summary>Called when this control receives focus.</summary>
		public event EventHandler FocusReceived;

		#endregion Events

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Control.</summary>
		public Control()
		{
			Parent = null;
			TopParent = null;
			OwningList = null;
			bounds = new Rectangle();
			Ignore = false;
			Visible = true;
			DrawBack = true;
			BackColor = Desktop.DefControlBackColor;
			stopOnTab = false;
		}

		/// <summary>Creates a new instance of Control.</summary>
		/// <param name="toClone">The control to clone.</param>
		public Control(Control toClone)
			: this()
		{
			bounds = toClone.Bounds;
			Ignore = toClone.Ignore;
			Visible = toClone.Visible;
			DrawBack = toClone.DrawBack;
			BackColor = toClone.BackColor;
			stopOnTab = toClone.StopOnTab;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Determines whether the specified point is in the bounds of this control or not.</summary>
		/// <param name="location">The point to test.</param>
		/// <returns>Whether the specified point is in the bounds of this control or not.</returns>
		public bool inBounds(Point location)
		{
			return !Ignore &&
				Visible &&
				Bounds.Contains(location);
		}

		/// <summary>Gets the control that is at the specified point.</summary>
		/// <param name="location">The point to test.</param>
		/// <returns>The control that is at the specified point.</returns>
		public virtual Control getControlAt(Point location)
		{
			return inBounds(location) ? this : null;
		}

		/// <summary>Performs the specified mouse event on this control.</summary>
		/// <param name="evnt">The event to perform.</param>
		/// <param name="cursorPos">The position of the cursor at the time of the event.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Whether or not the event was performed.</returns>
		public virtual bool PerformMouseEvent(Desktop.Event evnt, Point cursorPos, Input.InpState input)
		{
			switch (evnt)
			{
				case Desktop.Event.MouseLeftUp:
					OnMouseLeftUp();
					break;
				case Desktop.Event.MouseLeftDown:
					Focus();
					OnMouseLeftDown();
					break;
				case Desktop.Event.MouseLeftDoubleClick:
					OnMouseLeftDoubleClick();
					break;
				case Desktop.Event.MouseRightUp:
					OnMouseRightUp();
					break;
				case Desktop.Event.MouseRightDown:
					Focus();
					OnMouseRightDown();
					break;
				case Desktop.Event.MouseRightDoubleClick:
					OnMouseRightDoubleClick();
					break;
				default:
					throw new Exception("Unrecognized event: " + evnt.ToString());
			}

			return true;
		}

		/// <summary>Performs a key event on this Control.</summary>
		/// <param name="keys">The pressed keys.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Not sure yet.</returns>
		public virtual bool PerformKeyEvent(Keys[] keys, InpState input)
		{
			if (StopOnTab && Parent != null && !input.OldKey.IsKeyDown(Keys.Tab) && input.Key.IsKeyDown(Keys.Tab))
			{
				Container prnt = (Container)Parent;
				int index = prnt.Controls.IndexOf(this);

				// tab back
				if (input.Key.IsKeyDown(Keys.LeftShift) || input.Key.IsKeyDown(Keys.RightShift))
				{
					for (int i = index - 1; i >= 0; i--)
					{
						if (prnt.Controls[i].StopOnTab)
						{
							prnt.Controls[i].Focus();
							return true;
						}
					}

					for (int i = prnt.Controls.Count - 1; i > index; i--)
					{
						if (prnt.Controls[i].StopOnTab)
						{
							prnt.Controls[i].Focus();
							return true;
						}
					}
				}
				else // tab forward
				{
					for (int i = index + 1; i < prnt.Controls.Count; i++)
					{
						if (prnt.Controls[i].StopOnTab)
						{
							prnt.Controls[i].Focus();
							return true;
						}
					}

					for (int i = 0; i < index; i++)
					{
						if (prnt.Controls[i].StopOnTab)
						{
							prnt.Controls[i].Focus();
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>Places the focus on this control.</summary>
		public void Focus()
		{
			if (TopParent != null)
				TopParent.Focused = this;
		}

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public virtual void Draw(SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Draw(batch, new Rectangle(newLoc.X, newLoc.Y, Width, Height));
		}

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		/// <param name="bounds">A Rectangle representing this control's position and size on the screen.</param>
		protected void Draw(SpriteBatch batch, Rectangle bounds)
		{
			if (DrawBack)
			{
				batch.GraphicsDevice.ScissorRectangle = bounds;
				batch.Draw(TopParent.TBack, bounds, BackColor);
			}
		}

		/// <summary>Finds the position of this control on the screen.</summary>
		/// <returns>The position of this control on the screen.</returns>
		public Point getScreenPos()
		{
			Point loc = new Point();

			for (Control c = this; c.Parent != null; c = c.Parent)
			{
				loc += c.Location;
			}

			return loc;
		}

		/// <summary>Called when the location or size of this control is changed.</summary>
		protected virtual void locSizeChgd()
		{
			// do nothing
		}

		/// <summary>Called when the cursor is moved while this control is in focus.</summary>
		public virtual void mouseMove()
		{
			// do nothing
		}

		#region Events

		/// <summary>Called when the left mouse button is pressed down on this control.</summary>
		protected virtual void OnMouseLeftDown()
		{
			if (MouseLeftDown != null)
				MouseLeftDown(this, new EventArgs());
		}

		/// <summary>Called when the left mouse button is released from this control.</summary>
		protected virtual void OnMouseLeftUp()
		{
			if (MouseLeftUp != null)
				MouseLeftUp(this, new EventArgs());
		}

		/// <summary>Called when the left mouse button double-clicks this control.</summary>
		protected virtual void OnMouseLeftDoubleClick()
		{
			if (MouseLeftDoubleClick != null)
				MouseLeftDoubleClick(this, new EventArgs());
		}

		/// <summary>Called when the right mouse button is pressed down on this control.</summary>
		protected virtual void OnMouseRightUp()
		{
			if (MouseRightUp != null)
				MouseRightUp(this, new EventArgs());
		}

		/// <summary>Called when the right mouse button is released from this control.</summary>
		protected virtual void OnMouseRightDown()
		{
			if (MouseRightDown != null)
				MouseRightDown(this, new EventArgs());
		}

		/// <summary>Called when the right mouse button double-clicks this control.</summary>
		protected virtual void OnMouseRightDoubleClick()
		{
			if (MouseRightDoubleClick != null)
				MouseRightDoubleClick(this, new EventArgs());
		}

		/// <summary>Called when this control loses focus.</summary>
		public virtual void OnFocusLost()
		{
			if (FocusLost != null)
				FocusLost(this, new EventArgs());
		}

		/// <summary>Called when this control receives focus.</summary>
		public virtual void OnFocusReceived()
		{
			if (FocusReceived != null)
				FocusReceived(this, new EventArgs());
		}

		#endregion Events

		#endregion Methods
	}
}
