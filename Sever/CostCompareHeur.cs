using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class CostCompareHeur : IComparer<int>
	{
		#region Members

		/// <summary>The table of costs to compare.</summary>
		public double[] Costs { get; set; }

		/// <summary>The table of heuristics to compare.</summary>
		public double[] Heuristics { get; set; }

		#endregion Members

		#region Constructors

		public CostCompareHeur(double[] costs, double[] heuristics)
		{
			Costs = costs;
			Heuristics = heuristics;
		}

		#endregion Constructors

		#region Methods

		int IComparer<int>.Compare(int x, int y)
		{
			return (Costs[x] + Heuristics[x]).CompareTo(Costs[y] + Heuristics[y]);
		}

		#endregion Methods
	}
}
