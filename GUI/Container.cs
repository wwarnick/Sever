using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class Container : Control
	{
		#region Members

		/// <summary>The controls contained in this Container.</summary>
		public ControlList Controls { get; private set; }

		/// <summary>The top-level GUI that contains this control and its parents.</summary>
		private Desktop topParent;

		/// <summary>The top-level Desktop that contains this control and its parents.</summary>
		public override Desktop TopParent
		{
			get { return topParent; }
			set
			{
				if (Controls != null)
					Controls.TopOwner = value;

				topParent = value;
			}
		}

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Container.</summary>
		public Container()
			: base()
		{
			TopParent = null;
			Controls = new ControlList(this);
		}

		/// <summary>Creates a new instance of Container.</summary>
		/// <param name="toClone">The Container to clone.</param>
		public Container(Container toClone)
			: base(toClone)
		{
			TopParent = null;
			Controls = new ControlList(this);
		}

		#endregion Constructors

		#region Methods

		/// <summary>Gets the control that is at the specified point.</summary>
		/// <param name="location">The point to test.</param>
		/// <returns>The control that is at the specified point.</returns>
		public override Control getControlAt(Point location)
		{
			if (!inBounds(location))
				return null;
			
			Point rel = location - Location;

			foreach (Control c in Controls)
			{
				Control cc = c.getControlAt(rel);

				if (cc != null)
					return cc;
			}

			return this;
		}

		/// <summary>Performs the specified mouse event on this control.</summary>
		/// <param name="evnt">The event to perform.</param>
		/// <param name="cursorPos">The position of the cursor at the time of the event.</param>
		/// <param name="input">The current input state.</param>
		/// <returns>Whether or not the event was performed.</returns>
		public override bool PerformMouseEvent(Desktop.Event evnt, Point cursorPos, Input.InpState input)
		{
			Point pos = cursorPos - Location;

			foreach (Control c in Controls)
			{
				if (c.inBounds(pos))
					return c.PerformMouseEvent(evnt, pos, input);
			}

			return base.PerformMouseEvent(evnt, cursorPos, input);
		}

		/// <summary>Draws this control.</summary>
		/// <param name="batch">The sprite batch used to draw this control.</param>
		public override void Draw(SpriteBatch batch)
		{
			base.Draw(batch);

			IEnumerator<Control> enm = Controls.GetEnumeratorReverse();
			while (enm.MoveNext())
			{
				if (enm.Current.Visible)
					enm.Current.Draw(batch);
			}
		}

		/// <summary>Sets the height of this container to fit the controls within it.</summary>
		public void AutoHeight()
		{
			int bottomEdge = 0;

			foreach (Control c in Controls)
			{
				bottomEdge = Math.Max(bottomEdge, c.Bottom);
			}

			Height = bottomEdge;
		}

		#endregion Methods
	}
}
