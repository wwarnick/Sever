using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class ScrollBar
		: Container
	{
		#region Members

		/// <summary>The Up button.</summary>
		private ButtonText btnUp;

		/// <summary>The down button.</summary>
		private ButtonText btnDown;

		/// <summary>The scroll bar.</summary>
		private ButtonMove btnBar;

		/// <summary>The current amount of scroll.</summary>
		private int scroll;

		/// <summary>The current amount of scroll.</summary>
		public int Scroll
		{
			get { return scroll; }
			set
			{
				if (value < 0)
					scroll = 0;
				else if (value > maxScroll)
					scroll = maxScroll;
				else
					scroll = value;
				locSizeChgd();
			}
		}

		/// <summary>The scroll range.</summary>
		private int range;

		/// <summary>The scroll range.</summary>
		public int Range {
			get { return range; }
			set
			{
				range = value;
				MaxScroll = Math.Max(0, Range - activeRange);
			}
		}

		/// <summary>The active range of this ScrollBar.</summary>
		private int activeRange;

		/// <summary>The active range of this ScrollBar.</summary>
		public int ActiveRange
		{
			get { return activeRange; }
			set
			{
				activeRange = value;
				MaxScroll = Math.Max(0, Range - activeRange);
			}
		}

		/// <summary>The maximum value for Scroll.</summary>
		private int maxScroll;

		/// <summary>The maximum value for Scroll.</summary>
		private int MaxScroll
		{
			get { return maxScroll; }
			set
			{
				maxScroll = value;
				scroll = Math.Min(maxScroll, scroll);
				locSizeChgd();
			}
		}

		/// <summary>The amount of pixels to move the scrollbar when a button is pressed.</summary>
		public int JumpAmount { get; set; }

		/// <summary>Whether or not this scrollbar is horizontal.</summary>
		private bool horizontal;

		/// <summary>Called when the scroll value changes.</summary>
		public event EventHandler ScrollChg;

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of ScrollBar</summary>
		public ScrollBar()
			: base()
		{
			btnUp = new ButtonText();
			btnUp.MouseLeftDown += new EventHandler(btnUp_MouseLeftDown);
			Controls.Add(btnUp);

			btnDown = new ButtonText();
			btnDown.MouseLeftDown += new EventHandler(btnDown_MouseLeftDown);
			Controls.Add(btnDown);

			btnBar = new ButtonMove();
			btnBar.MouseDrag += new EventHandler(btnBar_MouseDrag);
			Controls.Add(btnBar);

			scroll = 0;
			range = 0;
			activeRange = 0;
			maxScroll = 0;
			JumpAmount = Desktop.DefScrollBarJumpAmount;

			locSizeChgd();
		}

		/// <summary>Creates a new instance of ScrollBar.</summary>
		/// <param name="toClone">The ScrollBar to clone.</param>
		public ScrollBar(ScrollBar toClone)
			: base(toClone)
		{
			btnUp = new ButtonText();
			btnUp.MouseLeftDown += new EventHandler(btnUp_MouseLeftDown);
			Controls.Add(btnUp);

			btnDown = new ButtonText();
			btnDown.MouseLeftDown += new EventHandler(btnDown_MouseLeftDown);
			Controls.Add(btnDown);

			btnBar = new ButtonMove();
			btnBar.MouseDrag += new EventHandler(btnBar_MouseDrag);
			Controls.Add(btnBar);

			scroll = 0;
			range = toClone.range;
			activeRange = toClone.activeRange;
			maxScroll = toClone.maxScroll;
			JumpAmount = toClone.JumpAmount;

			locSizeChgd();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Called when the location or size of this control is changed.</summary>
		protected override void locSizeChgd()
		{
			base.locSizeChgd();

			if (Width > Height)
			{
				horizontal = true;

				btnUp.Bounds = new Rectangle(0, 0, 20, Height);
				btnDown.Bounds = new Rectangle(Width - 20, 0, 20, Height);

				btnUp.Text = "<";
				btnDown.Text = ">";

				if (ActiveRange > Range)
				{
					btnBar.Bounds = new Rectangle(20, 0, btnDown.Left - btnUp.Right, 20);
				}
				else
				{
					int barWidth = (int)Math.Round((double)ActiveRange / (double)Range * (double)(Width - 40));
					btnBar.Bounds = new Rectangle(20 + (int)Math.Round((double)Scroll / (double)MaxScroll * (double)(Width - 40 - barWidth)), 0, barWidth, Height);
				}
			}
			else
			{
				horizontal = false;

				btnUp.Text = "^";
				btnDown.Text = "v";

				btnUp.Bounds = new Rectangle(0, 0, Width, 20);
				btnDown.Bounds = new Rectangle(0, Height - 20, Width, 20);

				if (ActiveRange > Range)
				{
					btnBar.Bounds = new Rectangle(0, 20, 20, btnDown.Top - btnUp.Bottom);
				}
				else
				{
					int barHeight = (int)Math.Round((double)ActiveRange / (double)Range * (double)(Height - 40));
					btnBar.Bounds = new Rectangle(0, 20 + (int)Math.Round((double)Scroll / (double)MaxScroll * (double)(Height - 40 - barHeight)), Width, barHeight);
				}
			}
		}

		#region Events

		/// <summary>Called when btnUp is clicked.</summary>
		private void btnUp_MouseLeftDown(object sender, EventArgs e)
		{
			if (ActiveRange < Range)
			{
				int newScroll = -1;
				if (horizontal)
				{
					int newLeft = Math.Max(btnUp.Right, btnBar.Left - JumpAmount);
					newScroll = Math.Min(scroll - 1, (int)Math.Round((double)(newLeft - btnUp.Right) / (double)(btnDown.Left - btnUp.Right) * (double)Range));
					newScroll = Math.Max(0, newScroll);
				}
				else
				{
					int newTop = Math.Max(btnUp.Bottom, btnBar.Top - JumpAmount);
					newScroll = Math.Min(scroll - 1, (int)Math.Round((double)(newTop - btnUp.Bottom) / (double)(btnDown.Top - btnUp.Bottom) * (double)Range));
					newScroll = Math.Max(0, newScroll);
				}

				if (newScroll != scroll)
				{
					Scroll = newScroll;
					OnScrollChg();
				}
			}
		}

		/// <summary>Called when btnDown is clicked.</summary>
		private void btnDown_MouseLeftDown(object sender, EventArgs e)
		{
			if (ActiveRange < Range)
			{
				int newScroll = -1;
				if (horizontal)
				{
					int newLeft = Math.Min(btnDown.Left - btnBar.Width, btnBar.Left + JumpAmount);
					newScroll = Math.Max(scroll + 1, (int)Math.Round((double)(newLeft - btnUp.Right) / (double)(btnDown.Left - btnUp.Right) * (double)Range));
					newScroll = Math.Min(maxScroll, newScroll);
				}
				else
				{
					int newTop = Math.Min(btnDown.Top - btnBar.Height, btnBar.Top + JumpAmount);
					newScroll = Math.Max(scroll + 1, (int)Math.Round((double)(newTop - btnUp.Bottom) / (double)(btnDown.Top - btnUp.Bottom) * (double)Range));
					newScroll = Math.Min(maxScroll, newScroll);
				}

				if (newScroll != scroll)
				{
					Scroll = newScroll;
					OnScrollChg();
				}
			}
		}

		/// <summary>Called when btnBar is dragged.</summary>
		private void btnBar_MouseDrag(object sender, EventArgs e)
		{
			if (ActiveRange < Range)
			{
				int newScroll = -1;
				if (horizontal)
				{
					Point newLoc = btnBar.Location;
					newLoc.X = Math.Max(newLoc.X, btnUp.Right);
					newLoc.X = Math.Min(newLoc.X, btnDown.Left - btnBar.Width);
					newLoc.Y = 0;

					if (newLoc != btnBar.Location)
						btnBar.Location = newLoc;

					newScroll = Math.Min(maxScroll, (int)Math.Round((double)(btnBar.Left - btnUp.Right) / (double)(btnDown.Left - btnUp.Right) * (double)Range));
					btnBar.Left = 20 + (int)Math.Round((double)newScroll / (double)MaxScroll * (double)(Width - 40 - btnBar.Width));
				}
				else
				{
					Point newLoc = btnBar.Location;
					newLoc.X = 0;
					newLoc.Y = Math.Max(newLoc.Y, btnUp.Bottom);
					newLoc.Y = Math.Min(newLoc.Y, btnDown.Top - btnBar.Height);

					if (newLoc != btnBar.Location)
						btnBar.Location = newLoc;

					newScroll = Math.Min(maxScroll, (int)Math.Round((double)(btnBar.Top - btnUp.Bottom) / (double)(btnDown.Top - btnUp.Bottom) * (double)Range));
					btnBar.Top = 20 + (int)Math.Round((double)newScroll / (double)MaxScroll * (double)(Height - 40 - btnBar.Height));
				}

				if (newScroll != scroll)
				{
					scroll = newScroll;
					OnScrollChg();
				}
			}
			else if (horizontal)
			{
				btnBar.Location = new Point(20, 0);
			}
			else
			{
				btnBar.Location = new Point(0, 20);
			}
		}

		/// <summary>Called when the scroll value changes.</summary>
		private void OnScrollChg()
		{
			if (ScrollChg != null)
				ScrollChg(this, new EventArgs());
		}

		/// <summary>Called when the left mouse button is pressed down on this control.</summary>
		protected override void OnMouseLeftDown()
		{
			Point p = TopParent.CursorPos - getScreenPos();
#if DEBUG
			if ((horizontal && (p.X < btnUp.Right || p.X >= btnDown.Left || (p.X >= btnBar.Left && p.X < btnBar.Right)))
				|| (!horizontal && (p.Y < btnUp.Bottom || p.Y >= btnDown.Top || (p.Y >= btnBar.Top && p.Y < btnBar.Bottom))))
				throw new Exception("MouseLeftDown called when cursor was above a child control.");
#endif
			if ((horizontal && p.X >= btnBar.Right) || (!horizontal && p.Y >= btnBar.Bottom))
			{
				if (scroll < maxScroll)
				{
					Scroll += ActiveRange;
					OnScrollChg();
				}
			}
			else
			{
				if (scroll > 0)
				{
					Scroll -= ActiveRange;
					OnScrollChg();
				}
			}

			base.OnMouseLeftDown();
		}

		#endregion Events

		#endregion Methods
	}
}
