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
    public class Desktop : Container
	{
		#region Members

		public enum Event { MouseLeftUp, MouseLeftDown, MouseRightUp, MouseRightDown, MouseLeftDoubleClick, MouseRightDoubleClick }

		public enum Alignment { TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight }

		/// <summary>The control that has the focus.</summary>
		private Control focused;

		/// <summary>The control that has the focus.</summary>
		public Control Focused
		{
			get { return focused; }
			set
			{
				if (focused != null)
					focused.OnFocusLost();

				focused = value;

				if (focused != null)
					focused.OnFocusReceived();
			}
		}

		/// <summary>The control that the cursor is hovering over.</summary>
		public Control Hovered { get; set; }

		/// <summary>The last control that the cursor pressed.</summary>
		private Control lastControlPressed { get; set; }

		/// <summary>The last time that a control was pressed by the cursor.</summary>
		private DateTime lastTimePressed { get; set; }

		/// <summary>The maximum time between the first and second clicks to be considered a double click.</summary>
		public TimeSpan DoubleClickThreshold { get; set; }

		/// <summary>The texture to use for drawing controls.</summary>
		public Texture2D TBack { get; set; }

		/// <summary>Whether or not the left mouse button is down.</summary>
		public bool IsMouseLeftDown { get; private set; }

		/// <summary>Whether or not the right mouse button is down.</summary>
		public bool IsMouseRightDown { get; private set; }

		/// <summary>The position of the cursor.</summary>
		public Point CursorPos { get; private set; }

		/// <summary>The PopUpMenu currently visible.</summary>
		public PopUpMenu CurMenu { get; private set; }

		/// <summary>The RasterizerState to use for drawing.</summary>
		private RasterizerState rState;

		/// <summary>The RasterizerState to use for drawing.</summary>
		public RasterizerState RState
		{
			get { return rState; }
			private set { rState = value; }
		}

		#region Defaults

		/// <summary>The color of this control.</summary>
		public static Color DefControlBackColor { get; set; }

		/// <summary>The default color of a Button.</summary>
		public static Color DefButtonBackColor { get; set; }

		/// <summary>The default color of a Button when hovered.</summary>
		public static Color DefButtonBackColorHover { get; set; }

		/// <summary>The color of a Button when pressed.</summary>
		public static Color DefButtonBackColorPressed { get; set; }

		/// <summary>The default color of a Button when in Pressed state (toggle only).</summary>
		public static Color DefButtonBackColorP { get; set; }

		/// <summary>The default color of a Button when hovered and in Pressed state (toggle only).</summary>
		public static Color DefButtonBackColorHoverP { get; set; }

		/// <summary>The default font of the text in a ButtonText.</summary>
		public static SpriteFont DefButtonTextFont { get; set; }

		/// <summary>The default color of the foreground of a ButtonText.</summary>
		public static Color DefButtonTextForeColor { get; set; }

		/// <summary>The default color of the foreground of a ButtonText when hovered.</summary>
		public static Color DefButtonTextForeColorHover { get; set; }

		/// <summary>The default color of the foreground of a ButtonText when pressed.</summary>
		public static Color DefButtonTextForeColorPressed { get; set; }

		/// <summary>The default color of the foreground of a ButtonText when in Pressed state (toggle only).</summary>
		public static Color DefButtonTextForeColorP { get; set; }

		/// <summary>The default color of the foreground of a ButtonText when hovered and in Pressed state (toggle only).</summary>
		public static Color DefButtonTextForeColorHoverP { get; set; }

		/// <summary>The default alignment of the text in a ButtonText.</summary>
		public static Desktop.Alignment DefButtonTextTextAlign { get; set; }

		/// <summary>The default padding on either side of the text in a ButtonText.</summary>
		public static int DefButtonTextSidePadding { get; set; }

		/// <summary>The default background color of a text box.</summary>
		public static Color DefComboBoxTextBoxBackColor { get; set; }

		/// <summary>The default foreground color of a text box.</summary>
		public static Color DefComboBoxTextBoxForeColor { get; set; }

		/// <summary>The default padding on either side of the text in a ComboBox.</summary>
		public static int DefComboBoxSidePadding { get; set; }

		/// <summary>The default width of the ComboBox drop-down button.</summary>
		public static int DefComboBoxButtonWidth { get; set; }

		/// <summary>The default font of the text in a Label.</summary>
		public static SpriteFont DefLabelFont { get; set; }

		/// <summary>The default color of the foreground of a Label.</summary>
		public static Color DefLabelForeColor { get; set; }

		/// <summary>The default alignment of the text in a Label.</summary>
		public static Desktop.Alignment DefLabelTextAlign { get; set; }

		/// <summary>Whether or not the default Label should resize itself to fit the text inside it.</summary>
		public static bool DefLabelAutoSize { get; set; }

		/// <summary>The default font of the text in a PopUpMenu.</summary>
		public static SpriteFont DefPopUpMenuFont { get; set; }

		/// <summary>The default color of the background of a PopUpMenu.</summary>
		public static Color DefPopUpMenuBackColor { get; set; }

		/// <summary>The default color of a PopUpMenu when hovered.</summary>
		public static Color DefPopUpMenuBackColorHover { get; set; }

		/// <summary>The default color of the foreground of a PopUpMenu.</summary>
		public static Color DefPopUpMenuForeColor { get; set; }

		/// <summary>The default color of the foreground of a PopUpMenu when hovered.</summary>
		public static Color DefPopUpMenuForeColorHover { get; set; }

		/// <summary>The default padding on either side of the text in a PopUpMenu.</summary>
		public static int DefPopUpMenuSidePadding { get; set; }

		/// <summary>The default font of the text in a TextBox.</summary>
		public static SpriteFont DefTextBoxFont { get; set; }

		/// <summary>The default color of the background of a TextBox.</summary>
		public static Color DefTextBoxBackColor { get; set; }

		/// <summary>The default color of the foreground of a TextBox.</summary>
		public static Color DefTextBoxForeColor { get; set; }

		/// <summary>The default padding on either side of the text in a TextBox.</summary>
		public static int DefTextBoxSidePadding { get; set; }

		/// <summary>The default padding on the top and bottom of the text in a TextBox.</summary>
		public static int DefTextBoxVertPadding { get; set; }

		/// <summary>The color of the selected-text highlighting in a TextBox.</summary>
		public static Color DefTextBoxHighlightColor { get; set; }

		/// <summary>How many pixels to offset the edit cursor horizontally.</summary>
		public static int DefTextBoxEditPositionOffsetX { get; set; }

		/// <summary>How many pixels to offset the edit cursor vertically.</summary>
		public static int DefTextBoxEditPositionOffsetY { get; set; }

		/// <summary>The default number of pixels to scroll whenever a scroll button is pressed.</summary>
		public static int DefScrollBarJumpAmount { get; set; }

		/// <summary>The default color of a ListBox.</summary>
		public static Color DefListBoxBackColor { get; set; }

		/// <summary>The default color of a ListBox when hovered.</summary>
		public static Color DefListBoxBackColorHover { get; set; }

		/// <summary>The default color of a ListBox when Selected.</summary>
		public static Color DefListBoxBackColorSelected { get; set; }

		/// <summary>The default font of the text in a ListBoxText.</summary>
		public static SpriteFont DefListBoxTextFont { get; set; }

		/// <summary>The default fore color of a ListBoxText when hovered.</summary>
		public static Color DefListBoxTextForeColorHover { get; set; }

		/// <summary>The default fore color of a ListBoxText when Selected.</summary>
		public static Color DefListBoxTextForeColorSelected { get; set; }

		/// <summary>The default fore color of a ListBoxText.</summary>
		public static Color DefListBoxTextForeColor { get; set; }

		/// <summary>The default padding on either side of the text in a ListBoxText.</summary>
		public static int DefListBoxTextSidePadding { get; set; }

		#endregion Defaults

		#endregion Members

		#region Constructors

		static Desktop()
		{
			DefControlBackColor = Color.White;
			DefButtonBackColor = Color.White;
			DefButtonBackColorHover = Color.White;
			DefButtonBackColorPressed = Color.White;
			DefButtonBackColorP = Color.White;
			DefButtonBackColorHoverP = Color.White;
			DefButtonTextFont = null;
			DefButtonTextForeColor = Color.Black;
			DefButtonTextForeColorHover = Color.Black;
			DefButtonTextForeColorPressed = Color.Black;
			DefButtonTextForeColorP = Color.Black;
			DefButtonTextForeColorHoverP = Color.Black;
			DefButtonTextTextAlign = Alignment.Center;
			DefButtonTextSidePadding = 5;
			DefComboBoxTextBoxBackColor = Color.White;
			DefComboBoxTextBoxForeColor = Color.White;
			DefComboBoxSidePadding = 5;
			DefLabelFont = null;
			DefLabelForeColor = Color.Black;
			DefLabelTextAlign = Alignment.TopLeft;
			DefLabelAutoSize = true;
			DefPopUpMenuFont = null;
			DefPopUpMenuBackColor = Color.White;
			DefPopUpMenuBackColorHover = Color.White;
			DefPopUpMenuForeColor = Color.Black;
			DefPopUpMenuForeColorHover = Color.Black;
			DefPopUpMenuSidePadding = 5;
			DefTextBoxFont = null;
			DefTextBoxBackColor = Color.White;
			DefTextBoxForeColor = Color.Black;
			DefTextBoxSidePadding = 5;
			DefTextBoxVertPadding = 5;
			DefTextBoxHighlightColor = new Color(0, 0, 255, 100);
			DefTextBoxEditPositionOffsetX = -1;
			DefTextBoxEditPositionOffsetY = -1;
			DefScrollBarJumpAmount = 3;
			DefListBoxBackColor = Color.White;
			DefListBoxBackColorHover = Color.White;
			DefListBoxBackColorSelected = Color.White;
			DefListBoxTextFont = null;
			DefListBoxTextForeColorHover = Color.White;
			DefListBoxTextForeColorSelected = Color.White;
			DefListBoxTextForeColor = Color.Black;
			DefListBoxTextSidePadding = 5;
		}

		/// <summary>Creates a new instance of GUI.</summary>
		public Desktop()
			: base()
		{
			TopParent = this;
			Focused = null;
			Hovered = null;
			lastControlPressed = null;
			lastTimePressed = new DateTime();
			DoubleClickThreshold = new TimeSpan(0, 0, 0, 0, 175);
			TBack = null;
			IsMouseLeftDown = false;
			IsMouseRightDown = false;
			CursorPos = new Point();
			CurMenu = null;
			RState = new RasterizerState() { ScissorTestEnable = true };
			DrawBack = false;
		}

		/// <summary>Creates a new instance of GUI.</summary>
		/// <param name="toClone">The Desktop to clone.</param>
		public Desktop(Desktop toClone)
			: base(toClone)
		{
			TopParent = this;
			Focused = null;
			Hovered = null;
			TBack = toClone.TBack;
			IsMouseLeftDown = false;
			IsMouseRightDown = false;
			CursorPos = new Point();
			CurMenu = null;
			RState = new RasterizerState() { ScissorTestEnable = true };
		}

		#endregion Constructors

		#region Methods

		/// <summary>Performs the specified mouse event on this control.</summary>
		/// <param name="evnt">The event to perform.</param>
		/// <param name="cursorPos">The position of the cursor at the time of the event.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Whether or not the event was performed.</returns>
		public override bool PerformMouseEvent(Event evnt, Point cursorPos, InpState input)
		{
			switch (evnt)
			{
				case Event.MouseLeftUp:
				case Event.MouseRightUp:
					// update mouse state
					if (evnt == Event.MouseLeftUp)
						IsMouseLeftDown = false;
					else
						IsMouseRightDown = false;

					// hide the menu
					if (CurMenu != null && CurMenu.Parent != Focused)
						hideMenu();

					// event
					if (Focused != null)
					{
						Point cPos = cursorPos - Focused.getScreenPos() + Focused.Location;
						if (!Focused.inBounds(cPos) || !Focused.PerformMouseEvent(evnt, cPos, input))
							hideMenu();

						return true;
					}

					hideMenu();
					break;
				case Event.MouseLeftDown:
				case Event.MouseRightDown:
					// update mouse state
					DateTime now = DateTime.UtcNow;
					if (evnt == Event.MouseLeftDown)
					{
						IsMouseLeftDown = true;
						if (Hovered != null && Hovered.AcceptDoubleClicks && Hovered == lastControlPressed && now - lastTimePressed <= DoubleClickThreshold)
							evnt = Event.MouseLeftDoubleClick;
					}
					else
					{
						IsMouseRightDown = true;
						if (Hovered != null && Hovered.AcceptDoubleClicks &&  Hovered == lastControlPressed && now - lastTimePressed <= DoubleClickThreshold)
							evnt = Event.MouseRightDoubleClick;
					}

					PopUpMenu menu = CurMenu;

					// event
					bool result = false;
					if (Hovered != null)
						result = Hovered.PerformMouseEvent(evnt, cursorPos - Hovered.getScreenPos() + Hovered.Location, input);

					// if menu didn't change, hide it
					if (CurMenu != null && menu == CurMenu)
						hideMenu();

					// nothing was clicked
					if (!result)
					{
						Focused = null;
						lastControlPressed = null;
					}
					else
					{
						lastControlPressed = (evnt == Event.MouseLeftDoubleClick || evnt == Event.MouseRightDoubleClick) ? null : Hovered;
						lastTimePressed = now;
					}

					return result;
				default:
					throw new Exception("Unrecognized event: " + evnt.ToString());
			}

			return false;
		}

		/// <summary>Performs a key event on this Control.</summary>
		/// <param name="keys">The pressed keys.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Whether or not the desktop claimed the key presses.</returns>
		public override bool PerformKeyEvent(Keys[] keys, InpState input)
		{
			if (Focused == null)
				return false;

			return Focused.PerformKeyEvent(keys, input);
		}

		/// <summary>Called when the cursor is moved.  Updates which control is hovered.</summary>
		/// <param name="cursorPos">The position of the cursor at the time this method was called.</param>
		public void mouseMove(Point cursorPos)
		{
			CursorPos = cursorPos;
			Hovered = getControlAt(CursorPos);

			if (Focused != null)
				Focused.mouseMove();
		}

		/// <summary>Should never be called.</summary>
		public override void mouseMove()
		{
			throw new Exception("mouseMove() cannot be called on a Desktop object.");
		}

		/// <summary>Gets the control that is at the specified point.</summary>
		/// <param name="location">The point to test.</param>
		/// <returns>The control that is at the specified point.</returns>
		public override Control getControlAt(Point location)
		{
			// check the pop up menu first
			if (CurMenu != null && CurMenu.inBounds(location - CurMenu.getScreenPos() + CurMenu.Location))
				return CurMenu;

			foreach (Control c in Controls)
			{
				Control cc = c.getControlAt(location);

				if (cc != null)
					return cc;
			}

			return null;
		}

		/// <summary>Shows the specified menu.</summary>
		/// <param name="menu">The menu to show.</param>
		public void showMenu(PopUpMenu menu)
		{
			hideMenu();

			CurMenu = menu;

			if (CurMenu.inBounds(CursorPos - CurMenu.getScreenPos() + CurMenu.Location))
				Hovered = CurMenu;
		}

		/// <summary>Hides the current PopUpMenu.</summary>
		public void hideMenu()
		{
			if (Hovered == CurMenu)
				Hovered = null;

			CurMenu = null;
		}

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(SpriteBatch batch)
		{
			base.Draw(batch);

			// draw PopUpMenu
			if (CurMenu != null)
				CurMenu.Draw(batch);
		}

		#endregion Methods
	}
}
