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
	public class TextBox : Control
	{
		#region Members

		/// <summary>The text in this textbox.</summary>
		private StringBuilder text;

		/// <summary>The text in this textbox.</summary>
		public string Text
		{
			get { return text.ToString(); }
			set
			{
				editIndex = 0;
				editPosition = 0f;
				ScrollValue = 0f;
				text.Clear();
				text.Append(value);
			}
		}

		/// <summary>The color of the foreground of this TextBox.</summary>
		public Color ForeColor { get; set; }

		/// <summary>The font of the text in this TextBox.</summary>
		private SpriteFont font;

		/// <summary>The font of the text in this TextBox.</summary>
		public SpriteFont Font
		{
			get { return font; }
			set
			{
				font = value;

				if (font != null)
					updateEditPosition();
			}
		}

		/// <summary>The current character index to edit from.</summary>
		private int editIndex;

		/// <summary>The current position of the edit cursor.</summary>
		private float editPosition;

		/// <summary>How far that this textbox is scrolled to the right.</summary>
		private float scrollValue;

		/// <summary>How far that this textbox is scrolled to the right.</summary>
		public float ScrollValue
		{
			get { return scrollValue; }
			set
			{
				scrollValue = value;
				updateTextPos();
			}
		}

		/// <summary>The position of the text in this TextBox.</summary>
		private Vector2 textPos;

		/// <summary>The padding on either side of the text in this TextBox.</summary>
		private int sidePadding;

		/// <summary>The padding on either side of the text in this TextBox.</summary>
		public int SidePadding
		{
			get { return sidePadding; }
			set
			{
				sidePadding = value;
				updateTextPos();
			}
		}

		/// <summary>Whether or not entering alpha characters is allowed in this TextBox.</summary>
		public bool AllowAlpha { get; set; }

		/// <summary>Whether or not entering numeric characters is allowed in this TextBox.</summary>
		public bool AllowNumeric { get; set; }

		/// <summary>The special characters that are allowed.  Null means any.  Blank string means none.</summary>
		public string AllowedSpecChars { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of TextBox.</summary>
		public TextBox()
			: base()
		{
			text = new StringBuilder();
			BackColor = Desktop.DefTextBoxBackColor;
			ForeColor = Desktop.DefTextBoxForeColor;
			font = Desktop.DefTextBoxFont;
			editIndex = 0;
			editPosition = 0f;
			scrollValue = 0f;
			textPos = new Vector2();
			SidePadding = Desktop.DefTextBoxSidePadding;
			AllowAlpha = true;
			AllowNumeric = true;
			AllowedSpecChars = null;
			StopOnTab = true;
		}

		/// <summary>Creates a new instance of TextBox.</summary>
		/// <param name="toClone">The TextBox to clone.</param>
		public TextBox(TextBox toClone)
			: base(toClone)
		{
			text = new StringBuilder(toClone.text.ToString());
			ForeColor = toClone.ForeColor;
			font = toClone.Font;
			editIndex = 0;
			editPosition = 0f;
			scrollValue = 0f;
			textPos = toClone.textPos;
			sidePadding = toClone.sidePadding;
			AllowAlpha = toClone.AllowAlpha;
			AllowNumeric = toClone.AllowNumeric;
			AllowedSpecChars = toClone.AllowedSpecChars;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Performs the specified mouse event on this control.</summary>
		/// <param name="evnt">The event to perform.</param>
		/// <param name="cursorPos">The position of the cursor at the time of the event.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Whether or not the event was performed.</returns>
		public override bool PerformMouseEvent(Desktop.Event evnt, Point cursorPos, InpState input)
		{
			if (evnt == Desktop.Event.MouseLeftDown)
			{
				editIndex = getCharPos((float)(cursorPos.X - Left));
				updateEditPosition();
			}

			return base.PerformMouseEvent(evnt, cursorPos, input);
		}

		/// <summary>Finds the character position of the specified point in the string.</summary>
		/// <param name="pos">The position to search for.</param>
		/// <returns>The character position of the specified point in the string.</returns>
		private int getCharPos(float pos)
		{
			pos += ScrollValue - SidePadding;

			for (int i = 1; i <= text.Length; i++)
			{
				float diff = pos - Font.MeasureString(text.ToString(0, i)).X;

				if (diff < 0 && -diff > Font.MeasureString(text.ToString(i - 1, 1)).X / 2f)
					return i - 1;
			}

			return text.Length;
		}

		/// <summary>Performs a key event on this Control.</summary>
		/// <param name="keys">The pressed keys.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Not sure yet.</returns>
		public override bool PerformKeyEvent(Keys[] keys, InpState input)
		{
			if (base.PerformKeyEvent(keys, input))
				return true;

			foreach (Keys key in keys)
			{
				// make sure the key doesn't stick
				if (input.OldKey.IsKeyDown(key))
					continue;

				// A-Z
				if ((key >= Keys.A && key <= Keys.Z)) // alpha
				{
					if (AllowAlpha)
					{
						string str = null;

						if (input.Key.IsKeyDown(Keys.LeftShift) || input.Key.IsKeyDown(Keys.RightShift))
							str = ((char)key).ToString();
						else
							str = ((char)(key - Keys.A + 'a')).ToString();

						text.Insert(editIndex, str);
						editIndex++;
						updateEditPosition();
					}
				}

				// 0-9
				else if (key >= Keys.D0 && key <= Keys.D9)
				{
					bool usedSpec = false;
					if (input.Key.IsKeyDown(Keys.LeftShift) || input.Key.IsKeyDown(Keys.RightShift))
					{
						char ch;

						switch (key)
						{
							case Keys.D1:
								ch = '!';
								break;
							case Keys.D2:
								ch = '@';
								break;
							case Keys.D3:
								ch = '#';
								break;
							case Keys.D4:
								ch = '$';
								break;
							case Keys.D5:
								ch = '%';
								break;
							case Keys.D6:
								ch = '^';
								break;
							case Keys.D7:
								ch = '&';
								break;
							case Keys.D8:
								ch = '*';
								break;
							case Keys.D9:
								ch = '(';
								break;
							default: // Keys.D0:
								ch = ')';
								break;
						}

						if (AllowedSpecChars == null || AllowedSpecChars.Contains(ch))
						{
							usedSpec = true;

							text.Insert(editIndex, ch.ToString());
							editIndex++;
							updateEditPosition();
						}
					}

					if (!usedSpec && AllowNumeric)
					{
						text.Insert(editIndex, ((char)(key - Keys.D0 + '0')).ToString());
						editIndex++;
						updateEditPosition();
					}
				}

				// [space]
				else if (key == Keys.Space)
				{
					if (AllowedSpecChars == null || AllowedSpecChars.Contains(' '))
					{
						text.Insert(editIndex, ((char)key).ToString());
						editIndex++;
						updateEditPosition();
					}
				}

				// .
				else if (key == Keys.OemPeriod)
				{
					if (AllowedSpecChars == null || AllowedSpecChars.Contains('.'))
					{
						text.Insert(editIndex, '.');
						editIndex++;
						updateEditPosition();
					}
				}

				// -
				else if (key == Keys.OemMinus)
				{
					if (AllowedSpecChars == null || AllowedSpecChars.Contains('-'))
					{
						text.Insert(editIndex, '-');
						editIndex++;
						updateEditPosition();
					}
				}

				// ,
				else if (key == Keys.OemComma)
				{
					if (AllowedSpecChars == null || AllowedSpecChars.Contains(','))
					{
						text.Insert(editIndex, ',');
						editIndex++;
						updateEditPosition();
					}
				}

				// ' or "
				else if (key == Keys.OemQuotes)
				{
					if ((input.Key.IsKeyDown(Keys.LeftShift) || input.Key.IsKeyDown(Keys.RightShift)) && (AllowedSpecChars == null || AllowedSpecChars.Contains('\"')))
					{
						text.Insert(editIndex, '\"');
						editIndex++;
						updateEditPosition();
					}
					else if (AllowedSpecChars == null || AllowedSpecChars.Contains('\''))
					{
						text.Insert(editIndex, '\'');
						editIndex++;
						updateEditPosition();
					}
				}

				// ;
				else if (key == Keys.OemSemicolon)
				{
					if (AllowedSpecChars == null || AllowedSpecChars.Contains(';'))
					{
						text.Insert(editIndex, ';');
						editIndex++;
						updateEditPosition();
					}
				}

				// \ or |
				else if (key == Keys.OemPipe)
				{
					if ((input.Key.IsKeyDown(Keys.LeftShift) || input.Key.IsKeyDown(Keys.RightShift)) && (AllowedSpecChars == null || AllowedSpecChars.Contains('|')))
					{
						text.Insert(editIndex, '|');
						editIndex++;
						updateEditPosition();
					}
					else if (AllowedSpecChars == null || AllowedSpecChars.Contains('\\'))
					{
						text.Insert(editIndex, '\\');
						editIndex++;
						updateEditPosition();
					}
				}

				// / or ?
				else if (key == Keys.OemQuestion)
				{
					if ((input.Key.IsKeyDown(Keys.LeftShift) || input.Key.IsKeyDown(Keys.RightShift)) && (AllowedSpecChars == null || AllowedSpecChars.Contains('?')))
					{
						text.Insert(editIndex, '?');
						editIndex++;
						updateEditPosition();
					}
					else if (AllowedSpecChars == null || AllowedSpecChars.Contains('/'))
					{
						text.Insert(editIndex, '/');
						editIndex++;
						updateEditPosition();
					}
				}

				// [left]
				else if (key == Keys.Left)
				{
					if (editIndex > 0)
					{
						editIndex--;
						updateEditPosition();
					}
				}

				// [right]
				else if (key == Keys.Right)
				{
					if (editIndex < text.Length)
					{
						editIndex++;
						updateEditPosition();
					}
				}

				// [backspace]
				else if (key == Keys.Back)
				{
					if (editIndex > 0)
					{
						text.Remove(editIndex - 1, 1);
						editIndex--;

						updateEditPosition();

						if (Font.MeasureString(text).X < (float)Width)
							ScrollValue = 0f;
						else
							keepCursorInView();
					}
				}

				// [delete]
				else if (key == Keys.Delete)
				{
					if (editIndex < text.Length)
					{
						text.Remove(editIndex, 1);

						if (Font.MeasureString(text).X < (float)Width)
							ScrollValue = 0f;
					}
				}

				// [home]
				else if (key == Keys.Home)
				{
					if (editIndex > 0)
					{
						editIndex = 0;
						updateEditPosition();
					}
				}

				// [end]
				else if (key == Keys.End)
				{
					if (editIndex < text.Length)
					{
						editIndex = text.Length;
						updateEditPosition();
					}
				}
			}

			keepCursorInView();

			return true;
		}

		/// <summary>Updates the edit position.</summary>
		private void updateEditPosition()
		{
			editPosition = Font.MeasureString(text.ToString(0, editIndex)).X;
		}

		/// <summary>Updates the text position.</summary>
		private void updateTextPos()
		{
			textPos = new Vector2(-ScrollValue + SidePadding, 0f);
		}

		/// <summary>Scrolls, if necessary, to keep the cursor in view.</summary>
		private void keepCursorInView()
		{
			if (editPosition < ScrollValue)
				ScrollValue = editPosition;
			else if (editPosition + SidePadding * 2f > ScrollValue + (float)Width)
				ScrollValue = editPosition - (float)Width + SidePadding * 2f;
		}

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
		{
			Point newLoc = getScreenPos();
			Rectangle newRect = new Rectangle(newLoc.X, newLoc.Y, Width, Height);
			Vector2 textP = new Vector2(textPos.X + (float)newLoc.X, textPos.Y + (float)newLoc.Y);

			batch.GraphicsDevice.ScissorRectangle = newRect;

			Draw(batch, newRect);
			batch.DrawString(Font, Text, textP, ForeColor);

			// draw pipe
			if (HasFocus)
				batch.DrawString(Font, "|", new Vector2((float)Math.Round(textP.X + editPosition), textP.Y), ForeColor);
		}

		#endregion Methods
	}
}
