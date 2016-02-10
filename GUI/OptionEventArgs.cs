using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	class IndexEventArgs : EventArgs
	{
		#region Members

		public int Index { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of IndexEventArgs</summary>
		public IndexEventArgs(int index)
		{
			Index = index;
		}

		#endregion Constructors
	}
}
