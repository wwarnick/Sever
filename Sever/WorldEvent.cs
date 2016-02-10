using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class WorldEvent
	{
		#region Members

		/// <summary>The various types of events that may occur in the world.</summary>
		public enum EventType { AddSeg, RemSeg, SegChangeState, AddNode, RemNode, NodeChangeState }
		
		/// <summary>The event that took place.</summary>
		public EventType WEvent { get; set; }

		/// <summary>The operands in this player action.</summary>
		public object[] Arguments { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of WorldEvent.</summary>
		public WorldEvent()
		{
			clear();
		}

		/// <summary>Creates a new instance of WorldEvent.</summary>
		/// <param name="wEvent">The event that took place.</param>
		/// <param name="arguments">The operands in this player action.</param>
		public WorldEvent(EventType wEvent, params object[] arguments)
			: this()
		{
			WEvent = wEvent;
			Arguments = arguments;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			WEvent = EventType.AddSeg;
			Arguments = null;
		}

		#endregion Methods
	}
}