using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	class MinPQCost : MinPQ<int>
	{
		#region Members

		private int[] Srch;

		private double[] Costs;

		private double[] Heuristics;

		#endregion Members

		#region Constructors

		public MinPQCost(double[] costs, double[] heuristics)
			: base(costs.Length, (heuristics == null) ? (IComparer<int>)new CostCompare(costs) : (IComparer<int>)new CostCompareHeur(costs, heuristics))
		{
			Srch = new int[costs.Length];
			Costs = costs;
			Heuristics = heuristics;
		}

		#endregion Constructors

		#region Methods

		protected override void Exch(int firstIndex, int secondIndex)
		{
			int first = GetPQItem(firstIndex);
			int second = GetPQItem(secondIndex);

			int temp = Srch[first];
			Srch[first] = Srch[second];
			Srch[second] = temp;

			base.Exch(firstIndex, secondIndex);
		}

		public override int Dequeue()
		{
			Srch[GetPQItem(1)] = 0;

			if (Count > 1)
				Srch[GetPQItem(Count)] = 1;

			return base.Dequeue();
		}

		public override void Enqueue(int item)
		{
			Srch[item] = Count + 1;

			base.Enqueue(item);
		}

		public void costShrunk(int node)
		{
			Swim(Srch[node]);
		}

		#endregion Methods
	}
}
