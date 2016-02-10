using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Input
{
	public class InpState
	{
		#region Members

		/// <summary>The current state of the mouse.</summary>
		public MouseState Mse { get; private set; }

		/// <summary>The previous state of the mouse.</summary>
		public MouseState OldMse { get; private set; }

		/// <summary>The current state of the keyboard.</summary>
		public KeyboardState Key { get; private set; }

		/// <summary>The previous state of the keyboard.</summary>
		public KeyboardState OldKey { get; private set; }

		#endregion Members

		/// <summary>Creates a new instance of Input.</summary>
		public InpState()
		{
			clear();
		}

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			Mse = new MouseState();
			OldMse = new MouseState();
			Key = new KeyboardState();
			OldKey = new KeyboardState();
		}

		/// <summary>Refreshes the states of all inputs.</summary>
		public void refreshStates()
		{
			OldMse = Mse;
			OldKey = Key;
			Mse = Mouse.GetState();
			Key = Keyboard.GetState();
		}

		#endregion Methods
	}
}
