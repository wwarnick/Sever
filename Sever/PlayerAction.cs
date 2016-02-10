using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class PlayerAction
	{
		#region Members

		/// <summary>The various types of actions that may be performed by a player.</summary>
		public enum ActionType { BuildSeg, SplitSeg, DestroyNode, ClaimNode }

		/// <summary>The player performing the action.</summary>
		public Player Actor { get; set; }

		/// <summary>The action to perform.</summary>
		public ActionType Action { get; set; }

		/// <summary>The operands in this player action.</summary>
		public object[] Arguments { get; set; }

		#endregion Members

		#region Constructors
		
		/// <summary>Creates a new instance of PlayerAction.</summary>
		public PlayerAction()
		{
			clear();
		}

		/// <summary>Creates a new instance of PlayerAction.</summary>
		/// <param name="actor">The player performing the action.</param>
		/// <param name="action">The action to perform.</param>
		/// <param name="arguments">The operands in this player action.</param>
		public PlayerAction(Player actor, ActionType action, params object[] arguments)
			: this()
		{
			Actor = actor;
			Action = action;
			Arguments = arguments;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			Actor = null;
			Action = ActionType.BuildSeg;
			Arguments = null;
		}

		#endregion Methods
	}
}