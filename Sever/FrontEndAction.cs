using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class FrontEndAction
	{
		/// <summary>The types of front-end actions.</summary>
		public enum ActionType { Message, LoadMap }

		/// <summary></summary>
		public ActionType Action { get; set; }

		/// <summary>The action's parameters.</summary>
		public object[] Params { get; set; }

		/// <summary>Creates a new instance of FrontEndAction</summary>
		/// <param name="action">The action to perform.</param>
		/// <param name="parameters">The action's parameters.</param>
		public FrontEndAction(ActionType action, params object[] parameters)
		{
			Action = action;
			Params = parameters;
		}
	}
}
