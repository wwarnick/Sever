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
	public class TextBoxMulti : Control
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
				selectedFromIndex = -1;
				editPositionX = 0f;
				editPositionY = 0f;
				desiredEditPositionX = 0f;
				scrollValueH = 0f;
				ScrollValueV = 0f;
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
				{
					refreshFontHeight();
					updateEditPosition();
				}
			}
		}

		/// <summary>The height of the font.</summary>
		private float FontHeight { get; set; }

		/// <summary>The current character index to edit from.</summary>
		private int editIndex;

		/// <summary>The first index in the selection.  -1 if nothing selected.</summary>
		private int selectedFromIndex;

		/// <summary>Changes when the cursor blinks.</summary>
		private int cursBlinkOffset;

		/// <summary>The current horizontal position of the edit cursor.</summary>
		private float editPositionX;

		/// <summary>The current vertical position of the edit cursor.</summary>
		private float editPositionY;

		/// <summary>The desired horizontal position of the edit cursor.</summary>
		private float desiredEditPositionX;

		/// <summary>How far that this textbox is scrolled to the right.</summary>
		private float scrollValueH;

		/// <summary>How far that this textbox is scrolled to the right.</summary>
		public float ScrollValueH
		{
			get { return scrollValueH; }
			set
			{
				scrollValueH = value;
				updateTextPos();
			}
		}

		/// <summary>How far that this textbox is scrolled downwards.</summary>
		private float scrollValueV;

		/// <summary>How far that this textbox is scrolled downwards.</summary>
		public float ScrollValueV
		{
			get { return scrollValueV; }
			set
			{
				scrollValueV = value;
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

		/// <summary>The padding on the top and bottom of this TextBox.</summary>
		private int vertPadding;

		/// <summary>The padding on the top and bottom of this TextBox.</summary>
		public int VertPadding
		{
			get { return vertPadding; }
			set
			{
				vertPadding = value;
				updateTextPos();
			}
		}

		/// <summary>Whether or not entering alpha characters is allowed in this TextBox.</summary>
		public bool AllowAlpha { get; set; }

		/// <summary>Whether or not entering numeric characters is allowed in this TextBox.</summary>
		public bool AllowNumeric { get; set; }

		/// <summary>The special characters that are allowed.  Null means any.  Blank string means none.</summary>
		public string AllowedSpecChars { get; set; }

		/// <summary>The color of the selected-text highlighting.</summary>
		public Color HighlightColor { get; set; }

		/// <summary>The last time a key was pressed.</summary>
		private DateTime lastPressedKey;

		/// <summary>The last time a key event was raised.</summary>
		private DateTime lastKeyEvent;

		/// <summary>A list of highlight rectangles.</summary>
		private List<Rectangle> highlights;

		/// <summary>Whether or not this textbox allows multiple lines.</summary>
		public bool AllowMultiLine { get; set; }

		/// <summary>How many pixels to offset the edit cursor horizontally.</summary>
		public int EditPositionOffsetX { get; set; }

		/// <summary>How many pixels to offset the edit cursor vertically.</summary>
		public int EditPositionOffsetY { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of TextBox.</summary>
		public TextBoxMulti()
			: base()
		{
			text = new StringBuilder();
			BackColor = Desktop.DefTextBoxBackColor;
			ForeColor = Desktop.DefTextBoxForeColor;
			font = Desktop.DefTextBoxFont;
			refreshFontHeight();
			editIndex = 0;
			selectedFromIndex = -1;
			cursBlinkOffset = 0;
			editPositionX = 0f;
			editPositionY = 0f;
			desiredEditPositionX = 0f;
			scrollValueH = 0f;
			scrollValueV = 0f;
			textPos = new Vector2();
			sidePadding = Desktop.DefTextBoxSidePadding;
			VertPadding = Desktop.DefTextBoxVertPadding;
			AllowAlpha = true;
			AllowNumeric = true;
			AllowedSpecChars = null;
			HighlightColor = Desktop.DefTextBoxHighlightColor;
			lastPressedKey = DateTime.MinValue;
			lastKeyEvent = DateTime.MinValue;
			highlights = null;
			AllowMultiLine = false;
			EditPositionOffsetX = Desktop.DefTextBoxEditPositionOffsetX;
			EditPositionOffsetY = Desktop.DefTextBoxEditPositionOffsetY;
			StopOnTab = true;
		}

		/// <summary>Creates a new instance of TextBox.</summary>
		/// <param name="toClone">The TextBox to clone.</param>
		public TextBoxMulti(TextBoxMulti toClone)
			: base(toClone)
		{
			text = new StringBuilder(toClone.text.ToString());
			ForeColor = toClone.ForeColor;
			font = toClone.Font;
			FontHeight = toClone.FontHeight;
			editIndex = 0;
			selectedFromIndex = -1;
			cursBlinkOffset = 0;
			editPositionX = 0f;
			editPositionY = 0f;
			desiredEditPositionX = 0f;
			scrollValueH = 0f;
			scrollValueV = 0f;
			textPos = toClone.textPos;
			sidePadding = toClone.sidePadding;
			vertPadding = toClone.vertPadding;
			AllowAlpha = toClone.AllowAlpha;
			AllowNumeric = toClone.AllowNumeric;
			AllowedSpecChars = toClone.AllowedSpecChars;
			HighlightColor = toClone.HighlightColor;
			lastPressedKey = DateTime.MinValue;
			lastKeyEvent = DateTime.MinValue;
			highlights = null;
			AllowMultiLine = toClone.AllowMultiLine;
			EditPositionOffsetX = toClone.EditPositionOffsetX;
			EditPositionOffsetY = toClone.EditPositionOffsetY;
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
				cursBlinkOffset = DateTime.Now.Millisecond;

				if (input.Key.IsKeyDown(Keys.LeftShift) || input.Key.IsKeyDown(Keys.RightShift))
				{
					if (selectedFromIndex == -1)
						selectedFromIndex = editIndex;
				}
				else
				{
					selectedFromIndex = -1;
				}

				editIndex = getCharPos((float)(cursorPos.X - Left), (float)(cursorPos.Y - Top));
				updateEditPosition();
				refreshDesiredEditPositionX();

				refreshHighlights();
			}

			return base.PerformMouseEvent(evnt, cursorPos, input);
		}

		/// <summary>Called when the cursor is moved while this control is in focus.</summary>
		public override void mouseMove()
		{
			base.mouseMove();

			if (TopParent.IsMouseLeftDown)
			{
				Point pos = getScreenPos();
				int temp = getCharPos((float)(TopParent.CursorPos.X - pos.X), (float)(TopParent.CursorPos.Y - pos.Y));

				if (temp != selectedFromIndex)
				{
					selectedFromIndex = temp;

					if (selectedFromIndex == editIndex)
						selectedFromIndex = -1;

					refreshHighlights();
				}
			}
		}

		/// <summary>Finds the character position of the specified point in the string.</summary>
		/// <param name="x">The horizontal position to search for.</param>
		/// <param name="y">The vertical position to search for.</param>
		/// <returns>The character position of the specified point in the string.</returns>
		private int getCharPos(float x, float y)
		{
			x += ScrollValueH - SidePadding;
			y += ScrollValueV - VertPadding;

			int row = (int)(y / FontHeight);

			int index = -1;
			int endOfLine = text.Length;
			int r = 0;
			int nl = 0;

			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
				{
					r++;
					nl = i;
					if (r == row)
					{
						index = i + 1;
					}
					else if (r == row + 1)
					{
						endOfLine = i;
						break;
					}
				}
			}

			if (index == -1)
				index = row > r ? nl : 0;

			for (int i = index + 1; i <= endOfLine; i++)
			{
				if (i == text.Length || text[i] == '\n')
					return i;

				float diff = x - Font.MeasureString(text.ToString(index, i - index)).X;

                if (diff < 0 && -diff > Font.MeasureString(text.ToString(i - 1, 1)).X / 2f)
					return i - 1;
			}

			return endOfLine;
		}

		/// <summary>Performs a key event on this Control.</summary>
		/// <param name="keys">The pressed keys.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Not sure yet.</returns>
		public override bool PerformKeyEvent(Keys[] keys, InpState input)
		{
			if (base.PerformKeyEvent(keys, input))
				return true;

			DateTime now = DateTime.Now;
			cursBlinkOffset = now.Millisecond;
			bool shiftDown = (input.Key.IsKeyDown(Keys.LeftShift) || input.Key.IsKeyDown(Keys.RightShift));

			foreach (Keys key in keys)
			{
				string insertChar = null;

				bool sameKey = input.OldKey.IsKeyDown(key);

				// make sure the key doesn't stick
				if (sameKey & ((now - lastPressedKey).TotalSeconds < .5 || (now - lastKeyEvent).Milliseconds < 64))
					continue;

				if (!sameKey)
					lastPressedKey = now;

				// Ctrl + X/C/V
				if ((input.Key.IsKeyDown(Keys.LeftControl) || input.Key.IsKeyDown(Keys.RightControl)) && (key == Keys.X || key == Keys.C || key == Keys.V))
				{
					switch (key)
					{
						case Keys.X: // cut
							if (selectedFromIndex != -1 && selectedFromIndex != editIndex)
							{
								int from = Math.Min(editIndex, selectedFromIndex);
								int to = Math.Max(editIndex, selectedFromIndex);
								System.Windows.Forms.Clipboard.SetText(text.ToString(from, to - from));

								// delete selected text
								text.Remove(from, to - from);
								editIndex = from;
								selectedFromIndex = -1;

								updateEditPosition();

								if (Font.MeasureString(text).X < (float)Width)
									ScrollValueH = 0f;
								else
									keepCursorInView();

								refreshDesiredEditPositionX();
							}
							break;
						case Keys.C: // copy
							if (selectedFromIndex != -1 && selectedFromIndex != editIndex)
							{
								int from = Math.Min(editIndex, selectedFromIndex);
								int to = Math.Max(editIndex, selectedFromIndex);
								System.Windows.Forms.Clipboard.SetText(text.ToString(from, to - from));
							}
							break;
						case Keys.V: // paste
							if (System.Windows.Forms.Clipboard.ContainsText())
							{
								// remove invalid special characters
								StringBuilder buf = new StringBuilder(System.Windows.Forms.Clipboard.GetText());
								for (int i = 0; i < buf.Length; i++)
								{
									if (!(AllowAlpha && ((buf[i] >= 'a' && buf[i] <= 'z') || (buf[i] >= 'A' && buf[i] <= 'Z')))
										&& !(AllowNumeric && buf[i] >= '0' && buf[i] <= '9')
										&& !(AllowMultiLine && buf[i] == '\n')
										&& !((AllowedSpecChars == null && "!@#$%^&*() .-,\"\';|\\?/".Contains(buf[i])) || (AllowedSpecChars != null && AllowedSpecChars.Contains(buf[i]))))
									{
										buf.Remove(i, 1);
										i--;
									}
								}
								insertChar = buf.ToString();
							}

							break;
					}
				}

				// A-Z
				else if (key >= Keys.A && key <= Keys.Z) // alpha
				{
					if (AllowAlpha)
					{
						insertChar = shiftDown
							? ((char)key).ToString()
							: ((char)(key - Keys.A + 'a')).ToString();
					}
				}

				// 0-9
				else if (key >= Keys.D0 && key <= Keys.D9)
				{
					bool usedSpec = false;
					if (shiftDown)
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

						if (isSpecCharAllowed(ch))
						{
							usedSpec = true;
							insertChar = ch.ToString();
						}
					}

					if (!usedSpec && AllowNumeric)
						insertChar = ((char)(key - Keys.D0 + '0')).ToString();
				}
				else
				{
					switch (key)
					{
						// [space]
						case Keys.Space:
							if (isSpecCharAllowed(' '))
								insertChar = ((char)key).ToString();
							break;

						// . or >
						case Keys.OemPeriod:
							if (shiftDown && isSpecCharAllowed('>'))
								insertChar = ">";
							else if (isSpecCharAllowed('.'))
								insertChar = ".";
							break;

						// - or _
						case Keys.OemMinus:
							if (shiftDown && isSpecCharAllowed('_'))
								insertChar = "_";
							else if (isSpecCharAllowed('-'))
								insertChar = "-";
							break;

						// , or <
						case Keys.OemComma:
							if (shiftDown && isSpecCharAllowed('<'))
								insertChar = "<";
							else if (isSpecCharAllowed(','))
								insertChar = ",";
							break;

						// ' or "
						case Keys.OemQuotes:
							if (shiftDown && isSpecCharAllowed('\"'))
								insertChar = "\"";
							else if (isSpecCharAllowed('\''))
								insertChar = "\'";
							break;

						// ; or :
						case Keys.OemSemicolon:
							if (shiftDown && isSpecCharAllowed(':'))
								insertChar = ":";
                            else if (isSpecCharAllowed(';'))
								insertChar = ";";
							break;

						// \ or |
						case Keys.OemPipe:
							if (shiftDown && isSpecCharAllowed('|'))
								insertChar = "|";
							else if (isSpecCharAllowed('\\'))
								insertChar = "\\";
							break;

						// / or ?
						case Keys.OemQuestion:
							if (shiftDown && isSpecCharAllowed('?'))
								insertChar = "?";
							else if (isSpecCharAllowed('/'))
								insertChar = "/";
							break;

						// [ or {
						case Keys.OemOpenBrackets:
							if (shiftDown && isSpecCharAllowed('{'))
								insertChar = "{";
							else if (isSpecCharAllowed('['))
								insertChar = "[";
							break;

						// ] or }
						case Keys.OemCloseBrackets:
							if (shiftDown && isSpecCharAllowed('}'))
								insertChar = "}";
							else if (isSpecCharAllowed(']'))
								insertChar = "]";
							break;

						// = or +
						case Keys.OemPlus:
							if (shiftDown && isSpecCharAllowed('+'))
								insertChar = "+";
							else if (isSpecCharAllowed('='))
								insertChar = "=";
							break;

						// ` or ~
						case Keys.OemTilde:
							if (shiftDown && isSpecCharAllowed('~'))
								insertChar = "~";
							else if (isSpecCharAllowed('`'))
								insertChar = "`";
							break;

						// [left]
						case Keys.Left:
							if (selectedFromIndex != -1 && selectedFromIndex != editIndex && !shiftDown)
								editIndex = Math.Min(editIndex, selectedFromIndex) + 1;

							refreshSelectedFromIndex(input);

							if (editIndex > 0)
							{
								editIndex--;
								updateEditPosition();
								refreshDesiredEditPositionX();
                            }
							break;

						// [right]
						case Keys.Right:
							if (selectedFromIndex != -1 && selectedFromIndex != editIndex && !shiftDown)
								editIndex = Math.Max(editIndex, selectedFromIndex) - 1;

							refreshSelectedFromIndex(input);
							
							if (editIndex < text.Length)
							{
								editIndex++;
								updateEditPosition();
								refreshDesiredEditPositionX();
							}
							break;

						// [up]
						case Keys.Up:
							{
								if (selectedFromIndex != -1 && selectedFromIndex != editIndex && !shiftDown)
								{
									editIndex = Math.Min(editIndex, selectedFromIndex);
									refreshDesiredEditPositionX();
								}
								refreshSelectedFromIndex(input);

								int begPrev = 0;
								int begThis = -1;
								for (int i = editIndex - 1; i > 0; i--)
								{
									if (text[i] == '\n')
									{
										if (begThis == -1)
											begThis = i + 1;
										else
										{
											begPrev = i + 1;
											break;
										}
									}
								}

								if (begThis != -1)
								{
									float lWidth = desiredEditPositionX;

									editIndex = begThis - 1;

									for (int i = begPrev; i < begThis - 1; i++)
									{
										float tWidth = Font.MeasureString(text.ToString(begPrev, i - begPrev + 1)).X;

										if (tWidth > lWidth)
										{
											float cWidth = Font.MeasureString(text[i].ToString()).X;
											editIndex = (lWidth > tWidth - cWidth / 2f) ? (i + 1) : i;
											break;
										}
									}
								}
								else
								{
									editIndex = 0;
								}

								updateEditPosition();
							}
							break;

						// [down]
						case Keys.Down:
							{
								if (selectedFromIndex != -1 && selectedFromIndex != editIndex && !shiftDown)
								{
									editIndex = Math.Max(editIndex, selectedFromIndex);
									refreshDesiredEditPositionX();
								}
								refreshSelectedFromIndex(input);

								int begNext = -1;
								int endNext = text.Length - 1;
								int begThis = 0;
								for (int i = editIndex - 1; i > 0; i--)
								{
									if (text[i] == '\n')
									{
										begThis = i + 1;
										break;
									}
								}

								for (int i = editIndex; i < text.Length; i++)
								{
									if (text[i] == '\n')
									{
										if (begNext == -1)
										{
											begNext = i + 1;
										}
										else
										{
											endNext = i - 1;
											break;
										}
									}
								}

								if (begNext != -1)
								{
									float lWidth = desiredEditPositionX;

									editIndex = endNext + 1;

									for (int i = begNext; i <= endNext; i++)
									{
										float tWidth = Font.MeasureString(text.ToString(begNext, i - begNext + 1)).X;

										if (tWidth > lWidth)
										{
											float cWidth = Font.MeasureString(text[i].ToString()).X;
											editIndex = (lWidth > tWidth - cWidth / 2f) ? (i + 1) : i;
											break;
										}
									}
								}
								else
								{
									editIndex = text.Length;
								}

								updateEditPosition();

							}
							break;

						// [backspace]
						case Keys.Back:
							{
								bool successful = false;
								if (selectedFromIndex != -1)
								{
									int from = Math.Min(editIndex, selectedFromIndex);
									int to = Math.Max(editIndex, selectedFromIndex);

									text.Remove(from, to - from);
									editIndex = from;
									selectedFromIndex = -1;

									successful = true;
								}
								else if (editIndex > 0)
								{
									text.Remove(editIndex - 1, 1);
									editIndex--;

									successful = true;
								}

								if (successful)
								{
									updateEditPosition();

									if (Font.MeasureString(text).X < (float)Width)
										ScrollValueH = 0f;
									else
										keepCursorInView();

									refreshDesiredEditPositionX();
								}
								break;
							}

						// [delete]
						case Keys.Delete:
							if (selectedFromIndex != -1)
							{
								int from = Math.Min(editIndex, selectedFromIndex);
								int to = Math.Max(editIndex, selectedFromIndex);

								text.Remove(from, to - from);
								editIndex = from;
								selectedFromIndex = -1;

								updateEditPosition();

								if (Font.MeasureString(text).X < (float)Width)
									ScrollValueH = 0f;
								else
									keepCursorInView();

								refreshDesiredEditPositionX();
							}
							else if (editIndex < text.Length)
							{
								text.Remove(editIndex, 1);

								if (Font.MeasureString(text).X < (float)Width)
									ScrollValueH = 0f;

								refreshDesiredEditPositionX();
							}
							break;

						// [home]
						case Keys.Home:
							refreshSelectedFromIndex(input);

							if (editIndex > 0)
							{
								for (; editIndex > 0 && text[editIndex - 1] != '\n'; editIndex--) { }
								updateEditPosition();
								refreshDesiredEditPositionX();
							}
							break;

						// [end]
						case Keys.End:
							refreshSelectedFromIndex(input);

							if (editIndex < text.Length)
							{
								for (; editIndex < text.Length && text[editIndex] != '\n'; editIndex++) { }
								updateEditPosition();
								refreshDesiredEditPositionX();
							}
							break;

						// [enter]
						case Keys.Enter:
							if (AllowMultiLine)
								insertChar = "\n";
							break;

						// [tab]
						case Keys.Tab:
							if (!StopOnTab)
							{
								if (selectedFromIndex != -1 && selectedFromIndex != editIndex)
								{
									bool editIndexFirst = editIndex <= selectedFromIndex;

									int index = editIndexFirst ? editIndex : selectedFromIndex;
									for (; index > 0 && text[index - 1] != '\n'; index--) { }

									int spaceCount = 0;
									for (int i = index; i < text.Length && text[i] == ' '; i++)
									{
										spaceCount++;
									}

									int from;
									int to;
									if (editIndexFirst)
									{
										from = editIndex;
										to = selectedFromIndex;
									}
									else
									{
										from = selectedFromIndex;
										to = editIndex;
									}

									if (shiftDown) // shift back
									{
										int toRemove = 0;

										if (spaceCount > 0)
										{
											toRemove = ((spaceCount - 1) % 3) + 1;
											text.Remove(index, toRemove);

											editIndex -= toRemove;
											selectedFromIndex -= toRemove;
											from -= toRemove;
											to -= toRemove;
										}

										for (; to < text.Length && text[to] != '\n'; to++) { }

										for (int i = from + 1; i < to; i++)
										{
											if (text[i - 1] == '\n')
											{
												spaceCount = 0;
												int j = i;
												for (; j < text.Length && text[j] == ' '; j++) { }
												spaceCount = j - i;

												if (spaceCount > 0)
												{
													toRemove = ((spaceCount - 1) % 3) + 1;
													text.Remove(i, toRemove);

													if (editIndexFirst)
														selectedFromIndex -= toRemove;
													else
														editIndex -= toRemove;

													to -= toRemove;
												}
											}
										}

										updateEditPosition();

										if (Font.MeasureString(text).X < (float)Width)
											ScrollValueH = 0f;
										else
											keepCursorInView();

										refreshDesiredEditPositionX();
									}
									else // shift forward
									{
										string toInsert = "".PadRight(3 - (spaceCount % 3));

										text.Insert(index, toInsert);

										editIndex += toInsert.Length;
										selectedFromIndex += toInsert.Length;
										from += toInsert.Length;
										to += toInsert.Length;

										for (; to < text.Length && text[to] != '\n'; to++) { }

										for (int i = from + 1; i < to; i++)
										{
											if (text[i - 1] == '\n')
											{
												spaceCount = 0;
												int j = i;
												for (; j < text.Length && text[j] == ' '; j++) { }
												spaceCount = j - i;

												toInsert = "".PadRight(3 - (spaceCount % 3));
												text.Insert(i, toInsert);

												if (editIndexFirst)
													selectedFromIndex += toInsert.Length;
												else
													editIndex += toInsert.Length;

												to += toInsert.Length;
											}
										}

										updateEditPosition();
										refreshDesiredEditPositionX();
									}
								}
								else
								{
									int lineIndex = 0;
									for (int i = editIndex - 1; i >= 0 && text[i] != '\n'; i--)
									{
										lineIndex++;
									}

									insertChar = "".PadRight(3 - (lineIndex % 3));
								}
							}
							break;
					}
				}

				// insert character
				if (insertChar != null)
				{
					if (selectedFromIndex != -1 && selectedFromIndex != editIndex)
					{
						int from = Math.Min(editIndex, selectedFromIndex);
						int to = Math.Max(editIndex, selectedFromIndex);

						text.Remove(from, to - from);
						editIndex = from;
					}
					
					text.Insert(editIndex, insertChar);
					editIndex += insertChar.Length;
					updateEditPosition();

					if (selectedFromIndex != -1)
					{
						if (Font.MeasureString(text).X < (float)Width)
							ScrollValueH = 0f;
						else
							keepCursorInView();
					}

					refreshDesiredEditPositionX();

					selectedFromIndex = -1;
				}

				keepCursorInView();
				refreshHighlights();

				lastKeyEvent = now;
			}

			return true;
		}

		/// <summary>Determines whether or not the provided special character is allowed.</summary>
		/// <param name="ch">The character to test.</param>
		private bool isSpecCharAllowed(char ch)
		{
			return AllowedSpecChars == null || AllowedSpecChars.Contains(ch);
        }

		/// <summary>To be called at the beginning of certain key event.</summary>
		/// <param name="input">The current input state.</param>
		private void refreshSelectedFromIndex(InpState input)
		{
			if (!input.Key.IsKeyDown(Keys.LeftShift) && !input.Key.IsKeyDown(Keys.RightShift))
				selectedFromIndex = -1;
			else if (selectedFromIndex == -1)
				selectedFromIndex = editIndex;
		}

		/// <summary>Refreshes the list of highlight rectangles.</summary>
		private void refreshHighlights()
		{
			if (selectedFromIndex == -1)
			{
				highlights = null;
			}
			else
			{
				highlights = new List<Rectangle>();

				int from = Math.Min(editIndex, selectedFromIndex);
				int to = Math.Max(editIndex, selectedFromIndex);

				int begRow = 0;
				int endRow = 0;
				int begIndex = 0;
				int endIndex = 0;
				int begEnd = to;

				Point upLeft = new Point();
				Point siz = new Point();

				for (int i = 0; i < from; i++)
				{
					if (text[i] == '\n')
					{
						begRow++;
						begIndex = i + 1;
					}
				}

				endRow = begRow;
				for (int i = from; i < to; i++)
				{
					if (text[i] == '\n')
					{
						if (begEnd == to)
							begEnd = i;
						else
						{
							upLeft = new Point(0, (int)Math.Round(endRow * FontHeight));
							siz = new Point((int)Math.Round(Font.MeasureString(text.ToString(endIndex, i - endIndex)).X), (int)Math.Round(FontHeight));
							highlights.Add(new Rectangle(upLeft, siz));
						}
						endRow++;
						endIndex = i + 1;
					}
				}

				upLeft = new Point((int)Math.Round(Font.MeasureString(text.ToString(begIndex, from - begIndex)).X), (int)Math.Round((float)begRow * FontHeight));
				siz = new Point((int)Math.Round(Font.MeasureString(text.ToString(from, begEnd - from)).X), (int)Math.Round(FontHeight));
				highlights.Add(new Rectangle(upLeft, siz));

				if (endRow != begRow)
				{
					upLeft = new Point(0, (int)Math.Round(endRow * FontHeight));
					siz = new Point((int)Math.Round(Font.MeasureString(text.ToString(endIndex, to - endIndex)).X), (int)Math.Round(FontHeight));
					highlights.Add(new Rectangle(upLeft, siz));
				}
			}
		}

		/// <summary>Updates the edit position.</summary>
		private void updateEditPosition()
		{
			int rows = 0;
			int beg;
			for (beg = editIndex; beg > 0 && text[beg - 1] != '\n'; beg--) { }

			if (beg != 0)
			{
				rows = 1;
				for (int i = beg - 2; i >= 0; i--)
				{
					if (text[i] == '\n')
						rows++;
				}
			}
			
			editPositionX = (float)EditPositionOffsetX + ((beg == editIndex) ? 0 : Font.MeasureString(text.ToString(beg, editIndex - beg)).X);
			editPositionY = (float)EditPositionOffsetY + (float)rows * FontHeight;
		}

		/// <summary>Updates the text position.</summary>
		private void updateTextPos()
		{
			textPos = new Vector2(-ScrollValueH + SidePadding, -ScrollValueV + VertPadding);
		}

		/// <summary>Scrolls, if necessary, to keep the cursor in view.</summary>
		private void keepCursorInView()
		{
			if (editPositionX < ScrollValueH)
				ScrollValueH = editPositionX;
			else if (editPositionX + SidePadding * 2f > ScrollValueH + (float)Width)
				ScrollValueH = editPositionX - (float)Width + SidePadding * 2f;

			if (editPositionY < ScrollValueV)
				ScrollValueV = editPositionY;
			else if (editPositionY + FontHeight + VertPadding * 2f > ScrollValueV + (float)Height)
				ScrollValueV = editPositionY + FontHeight - (float)Height + VertPadding * 2f;
		}

		/// <summary>Refreshes the font height.</summary>
		private void refreshFontHeight()
		{
			FontHeight = font.MeasureString("I").Y;
		}

		/// <summary>Refreshes the desired horizontal position of the edit cursor.</summary>
		private void refreshDesiredEditPositionX()
		{
			int beg = 0;
			for (int i = editIndex - 1; i >= 0; i--)
			{
				if (text[i] == '\n')
				{
					beg = i + 1;
					break;
				}
			}

			desiredEditPositionX = Font.MeasureString(text.ToString(beg, editIndex - beg)).X;
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

			// draw selection
			if (highlights != null)
			{
				foreach (Rectangle rect in highlights)
				{
					batch.Draw(TopParent.TBack, new Rectangle((int)textP.X + rect.X, (int)textP.Y + rect.Y, rect.Width, rect.Height), HighlightColor);
				}
			}

			// draw pipe
			if (HasFocus && DateTime.Now.AddMilliseconds(-cursBlinkOffset).Millisecond < 500)
				batch.DrawString(Font, "|", new Vector2((float)Math.Round(textP.X + editPositionX), textP.Y + editPositionY), ForeColor);
		}

		#endregion Methods
	}
}
