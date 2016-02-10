using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class ComboBox : ButtonText
	{
		#region Members

		/// <summary>The PopUpMenu attached to this ComboBox.</summary>
		public PopUpMenu Menu { get; private set; }

		/// <summary>The background color of the text box.</summary>
		public Color TextBoxBackColor { get; set; }

		/// <summary>The foreground color of the text box.</summary>
		public Color TextBoxForeColor { get; set; }

		/// <summary>The width of the drop-down button.</summary>
		private int buttonWidth;

		/// <summary>The width of the drop-down button.</summary>
		public int ButtonWidth
		{
			get { return buttonWidth; }
			set
			{
				buttonWidth = value;
				locSizeChgd();
			}
		}

		/// <summary>The position of the graphic inside the drop-down button on this ComboBox.</summary>
		private Vector2 buttonGraphicPos;

#if DEBUG
		/// <summary>The index of the currently selected item from the menu.</summary>
		private int selectedIndex;

		/// <summary>The index of the currently selected item from the menu.</summary>
		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				if (value >= Menu.Count)
					throw new Exception("Selected index is outside the range of the list.");
				selectedIndex = value;
			}
		}
#else
		/// <summary>The index of the currently selected item from the menu.</summary>
		public int SelectedIndex { get; set; }
#endif

		/// <summary>The currently selected value.</summary>
		public string SelectedValue
		{
			get { return Menu[SelectedIndex]; }
			set
			{
				for (int i = 0; i < Menu.Count; i++)
				{
					if (Menu[i] == value)
					{
						SelectedIndex = i;
						return;
					}
				}
				throw new Exception("Provided value is not in this list.");
			}
		}

		/// <summary>Whether or not this ComboBox should act as a toggle.  Only returns true.</summary>
		public override bool IsToggle
		{
			get { return true; }
			set { throw new Exception("Cannot set IsToggle on a ComboBox."); }
		}

		/// <summary>Whether or not this ComboBox is pressed.</summary>
		public override bool Pressed
		{
			get { return TopParent.CurMenu == Menu; }
			set
			{
				if (value)
				{
					if (TopParent.CurMenu != Menu)
						TopParent.showMenu(Menu);
				}
				else if (TopParent.CurMenu == Menu)
					TopParent.hideMenu();
			}
		}

		/// <summary>The top-level Desktop that contains this control and its parents.</summary>
		public override Desktop TopParent
		{
			get { return base.TopParent; }
			set
			{
				base.TopParent = value;

				if (Menu != null)
					Menu.TopParent = value;
			}
		}

		/// <summary>The font of the text in this ButtonText.</summary>
		public override SpriteFont Font
		{
			get { return base.Font; }
			set
			{
				base.Font = value;

				if (Menu != null)
					Menu.Font = value;
			}
		}

		/// <summary>The text to display on this ButtonText.</summary>
		public override string Text
		{
			get { return (SelectedIndex == -1) ? "" : Menu[SelectedIndex]; }
			set { throw new Exception("Cannot set Text on a ComboBox."); }
		}

		#region Events

		/// <summary>Called when the selected item is changed.</summary>
		public event EventHandler SelectedItemChanged;

		#endregion Events

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of ComboBox.</summary>
		public ComboBox()
			: base()
		{
			Menu = new PopUpMenu();
			Menu.TopParent = TopParent;
			Menu.Parent = this;
			Menu.OptionAdded += new EventHandler(Menu_OnOptionAdded);
			Menu.OptionRemoved += new EventHandler(Menu_OnOptionRemoved);
			Menu.OptionSelected += new EventHandler(Menu_OnOptionSelected);
			TextBoxBackColor = Desktop.DefComboBoxTextBoxBackColor;
			TextBoxForeColor = Desktop.DefComboBoxTextBoxForeColor;
			buttonWidth = Desktop.DefComboBoxButtonWidth;
			buttonGraphicPos = new Vector2();
			SelectedIndex = -1;
			TextAlign = Desktop.Alignment.TopLeft;
			SidePadding = Desktop.DefComboBoxSidePadding;
		}

		/// <summary>Creates a new instance of ComboBox.</summary>
		/// <param name="toClone">The ComboBox to clone.</param>
		/// <param name="copyItems">Whether or not to copy the items over as well.</param>
		public ComboBox(ComboBox toClone, bool copyItems)
			: base(toClone)
		{
			Menu = new PopUpMenu(toClone.Menu, copyItems);
			Menu.TopParent = TopParent;
			Menu.Parent = this;
			Menu.OptionAdded += new EventHandler(Menu_OnOptionAdded);
			Menu.OptionRemoved += new EventHandler(Menu_OnOptionRemoved);
			Menu.OptionSelected += new EventHandler(Menu_OnOptionSelected);
			TextBoxBackColor = toClone.TextBoxBackColor;
			TextBoxForeColor = toClone.TextBoxForeColor;
			buttonWidth = toClone.buttonWidth;
			buttonGraphicPos = toClone.buttonGraphicPos;
			SelectedIndex = -1;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Called when the location or size of this control is changed.</summary>
		protected override void locSizeChgd()
		{
			base.locSizeChgd();

			if (Menu != null)
			{
				Menu.Left = 0;
				Menu.Top = Height;
			}

			if (Font != null)
			{
				Vector2 size = Font.MeasureString("v");
				buttonGraphicPos = new Vector2((float)Width - (float)ButtonWidth + ((float)ButtonWidth - size.X) / 2f, ((float)Height - (float)size.Y) / 2f);
			}
		}

		/// <summary>Called when the left mouse button is pressed down on this control.</summary>
		protected override void OnMouseLeftDown()
		{
			Pressed = true;

			base.OnMouseLeftDown();
		}

		// <summary>Called when the left mouse button is released from this control.</summary>
		protected override void OnMouseLeftUp()
		{
			//Pressed = true;

			base.OnMouseLeftUp();
		}

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Rectangle newRect = new Rectangle(newLoc.X, newLoc.Y, Width - ButtonWidth, Height);
			Vector2 textPos = new Vector2(TextPos.X + (float)newLoc.X, TextPos.Y + (float)newLoc.Y);

			batch.GraphicsDevice.ScissorRectangle = newRect;
			batch.Draw(TopParent.TBack, newRect, TextBoxBackColor);
			batch.DrawString(Font, Text, textPos, TextBoxForeColor);

			newRect.Width = ButtonWidth;
			newRect.X += Width - ButtonWidth;

			batch.GraphicsDevice.ScissorRectangle = newRect;

			Vector2 vPos = new Vector2((float)(newLoc.X + buttonGraphicPos.X), (float)(newLoc.Y + buttonGraphicPos.Y));

			if (IsToggle && Pressed)
			{
				if (!HasHover)
				{
					batch.Draw(TopParent.TBack, newRect, BackColorP);
					batch.DrawString(Font, "v", vPos, ForeColorP);
				}
				else if (HasFocus && TopParent.IsMouseLeftDown)
				{
					batch.Draw(TopParent.TBack, newRect, BackColorPressed);
					batch.DrawString(Font, "v", vPos, ForeColorPressed);
				}
				else
				{
					batch.Draw(TopParent.TBack, newRect, BackColorHoverP);
					batch.DrawString(Font, "v", vPos, ForeColorHoverP);
				}
			}
			else
			{
				if (!HasHover)
				{
					Draw(batch, newRect);
					batch.DrawString(Font, "v", vPos, ForeColor);
				}
				else if (HasFocus && TopParent.IsMouseLeftDown)
				{
					batch.Draw(TopParent.TBack, newRect, BackColorPressed);
					batch.DrawString(Font, "v", vPos, ForeColorPressed);
				}
				else
				{
					batch.Draw(TopParent.TBack, newRect, BackColorHover);
					batch.DrawString(Font, "v", vPos, ForeColorHover);
				}
			}
		}

		#region Events

		/// <summary>Called when an option is selected from the menu.</summary>
		/// <param name="sender">The menu.</param>
		/// <param name="e">Contains the index of the selected option.</param>
		private void Menu_OnOptionSelected(object sender, EventArgs e)
		{
			int index = ((IndexEventArgs)e).Index;
			SelectedIndex = index;
			OnSelectedItemChanged(index);
		}

		/// <summary>Called when an option is removed from the menu.</summary>
		/// <param name="sender">The menu.</param>
		/// <param name="e">Contains the index of the removed option.</param>
		private void Menu_OnOptionRemoved(object sender, EventArgs e)
		{
			int option = ((IndexEventArgs)e).Index;

			if (SelectedIndex == option)
			{
				SelectedIndex = (Menu.Count > 0) ? 0 : -1;
				OnSelectedItemChanged(SelectedIndex);
			}
			else if (option < SelectedIndex)
			{
				SelectedIndex--;
			}
		}

		/// <summary>Called when an option is added to the menu.</summary>
		/// <param name="sender">The menu.</param>
		/// <param name="e">Contains the index of the added option.</param>
		private void Menu_OnOptionAdded(object sender, EventArgs e)
		{
			int option = ((IndexEventArgs)e).Index;

			if (option <= SelectedIndex)
				SelectedIndex++;
			else if (SelectedIndex == -1)
				SelectedIndex = 0;
		}

		/// <summary>Called when the selected item changes.</summary>
		/// <param name="option">The index of the selected option.</param>
		protected virtual void OnSelectedItemChanged(int option)
		{
			if (SelectedItemChanged != null)
				SelectedItemChanged(this, new IndexEventArgs(option));
		}

		#endregion Events

		#endregion Methods
	}
}
