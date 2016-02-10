using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class CostCompare : IComparer<int>
	{
		#region Members

		/// <summary>The table of costs to compare.</summary>
		public double[] Costs { get; set; }

		#endregion Members

		#region Constructors

		public CostCompare(double[] costs)
		{
			Costs = costs;
		}

		#endregion Constructors

		#region Methods

		int IComparer<int>.Compare(int x, int y)
		{
			return Costs[x].CompareTo(Costs[y]);
		}

		#endregion Methods
	}
}
