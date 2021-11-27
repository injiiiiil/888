#region Copyright & License Information
/*
 * Copyright 2007-2021 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Pathfinder
{
	public sealed class PathSearch : IDisposable
	{
		/// <summary>
		/// When searching for paths, use a default weight of 125% to reduce
		/// computation effort - even if this means paths may be sub-optimal.
		/// </summary>
		public const int DefaultHeuristicWeightPercentage = 125;

		/// <summary>
		/// When searching for paths, use a lane bias to guide units into
		/// "lanes" whilst moving, to promote smooth unit flow when groups of
		/// units are moving.
		/// </summary>
		public const bool DefaultLaneBias = true;

		// PERF: Maintain a pool of layers used for paths searches for each world. These searches are performed often
		// so we wish to avoid the high cost of initializing a new search space every time by reusing the old ones.
		static readonly ConditionalWeakTable<World, CellInfoLayerPool> LayerPoolTable = new ConditionalWeakTable<World, CellInfoLayerPool>();
		static readonly ConditionalWeakTable<World, CellInfoLayerPool>.CreateValueCallback CreateLayerPool = world => new CellInfoLayerPool(world.Map);

		static CellInfoLayerPool LayerPoolForWorld(World world)
		{
			return LayerPoolTable.GetValue(world, CreateLayerPool);
		}

		public static PathSearch ToTargetCellByPredicate(
			World world, Locomotor locomotor, Actor self, IEnumerable<CPos> froms, Func<CPos, bool> targetPredicate, BlockedByActor check,
			Func<CPos, int> customCost = null)
		{
			var graph = new PathGraph(LayerPoolForWorld(world), locomotor, self, world, check, customCost, null, false, DefaultLaneBias);
			var search = new PathSearch(graph, loc => 0, DefaultHeuristicWeightPercentage, targetPredicate);

			foreach (var sl in froms)
				if (world.Map.Contains(sl))
					search.AddInitialCell(sl);

			return search;
		}

		public static PathSearch ToTargetCell(
			World world, Locomotor locomotor, Actor self, CPos from, CPos target, BlockedByActor check,
			Func<CPos, int> customCost = null,
			Actor ignoreActor = null,
			bool inReverse = false,
			bool laneBias = DefaultLaneBias)
		{
			return ToTargetCell(world, locomotor, self, new[] { from }, target, check, customCost, ignoreActor, inReverse, laneBias);
		}

		public static PathSearch ToTargetCell(
			World world, Locomotor locomotor, Actor self, IEnumerable<CPos> froms, CPos target, BlockedByActor check,
			Func<CPos, int> customCost = null,
			Actor ignoreActor = null,
			bool inReverse = false,
			bool laneBias = DefaultLaneBias,
			Func<CPos, int> heuristic = null,
			int heuristicWeightPercentage = DefaultHeuristicWeightPercentage)
		{
			var graph = new PathGraph(LayerPoolForWorld(world), locomotor, self, world, check, customCost, ignoreActor, inReverse, laneBias);

			heuristic = heuristic ?? DefaultCostEstimator(locomotor, target);
			var search = new PathSearch(graph, heuristic, heuristicWeightPercentage, loc => loc == target);

			foreach (var sl in froms)
				if (world.Map.Contains(sl))
					search.AddInitialCell(sl);

			return search;
		}

		/// <summary>
		/// Default: Diagonal distance heuristic. More information:
		/// http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
		/// Layers are ignored and incur no additional cost.
		/// </summary>
		/// <param name="locomotor">Locomotor used to provide terrain costs.</param>
		/// <param name="destination">The cell for which costs are to be given by the estimation function.</param>
		/// <returns>A delegate that calculates the cost estimation between the <paramref name="destination"/> and the given cell.</returns>
		public static Func<CPos, int> DefaultCostEstimator(Locomotor locomotor, CPos destination)
		{
			var estimator = DefaultCostEstimator(locomotor);
			return here => estimator(here, destination);
		}

		/// <summary>
		/// Default: Diagonal distance heuristic. More information:
		/// http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
		/// Layers are ignored and incur no additional cost.
		/// </summary>
		/// <param name="locomotor">Locomotor used to provide terrain costs.</param>
		/// <returns>A delegate that calculates the cost estimation between the given cells.</returns>
		public static Func<CPos, CPos, int> DefaultCostEstimator(Locomotor locomotor)
		{
			// Determine the minimum possible cost for moving horizontally between cells based on terrain speeds.
			// The minimum possible cost diagonally is then Sqrt(2) times more costly.
			var cellCost = locomotor.Info.TerrainSpeeds.Values.Min(ti => ti.Cost);
			var diagonalCellCost = Exts.MultiplyBySqrtTwo(cellCost);
			return (here, destination) =>
			{
				var diag = Math.Min(Math.Abs(here.X - destination.X), Math.Abs(here.Y - destination.Y));
				var straight = Math.Abs(here.X - destination.X) + Math.Abs(here.Y - destination.Y);

				// According to the information link, this is the shape of the function.
				// We just extract factors to simplify.
				// Possible simplification: var h = Constants.CellCost * (straight + (Constants.Sqrt2 - 2) * diag);
				return cellCost * straight + (diagonalCellCost - 2 * cellCost) * diag;
			};
		}

		public IPathGraph Graph { get; }
		readonly Func<CPos, int> heuristic;
		readonly int heuristicWeightPercentage;
		public Func<CPos, bool> TargetPredicate { get; set; }
		readonly IPriorityQueue<GraphConnection> openQueue;

		/// <summary>
		/// Initialize a new search.
		/// </summary>
		/// <param name="graph">Graph over which the search is conducted.</param>
		/// <param name="heuristic">Provides an estimation of the distance between the given cell and the target.</param>
		/// <param name="heuristicWeightPercentage">
		/// The search will aim for the shortest path when given a weight of 100%.
		/// We can allow the search to find paths that aren't optimal by changing the weight.
		/// The weight limits the worst case length of the path,
		/// e.g. a weight of 110% will find a path no more than 10% longer than the shortest possible.
		/// The benefit of allowing the search to return suboptimal paths is faster computation time.
		/// The search can skip some areas of the search space, meaning it has less work to do.
		/// </param>
		/// <param name="targetPredicate">Determines if the given cell is the target.</param>
		PathSearch(IPathGraph graph, Func<CPos, int> heuristic, int heuristicWeightPercentage, Func<CPos, bool> targetPredicate)
		{
			Graph = graph;
			this.heuristic = heuristic;
			this.heuristicWeightPercentage = heuristicWeightPercentage;
			TargetPredicate = targetPredicate;
			openQueue = new PriorityQueue<GraphConnection>(GraphConnection.ConnectionCostComparer);
		}

		void AddInitialCell(CPos location)
		{
			var estimatedCost = heuristic(location) * heuristicWeightPercentage / 100;
			Graph[location] = new CellInfo(CellStatus.Open, 0, estimatedCost, location);
			var connection = new GraphConnection(location, estimatedCost);
			openQueue.Add(connection);
		}

		/// <summary>
		/// Determines if there are more reachable cells and the search can be continued.
		/// If false, <see cref="Expand"/> can no longer be called.
		/// </summary>
		public bool CanExpand => !openQueue.Empty;

		/// <summary>
		/// This function analyzes the neighbors of the most promising node in the pathfinding graph
		/// using the A* algorithm (A-star) and returns that node
		/// </summary>
		/// <returns>The most promising node of the iteration</returns>
		public CPos Expand()
		{
			var currentMinNode = openQueue.Pop().Destination;

			var currentInfo = Graph[currentMinNode];
			Graph[currentMinNode] = new CellInfo(CellStatus.Closed, currentInfo.CostSoFar, currentInfo.EstimatedTotalCost, currentInfo.PreviousNode);

			foreach (var connection in Graph.GetConnections(currentMinNode))
			{
				// Calculate the cost up to that point
				var costSoFarToNeighbor = currentInfo.CostSoFar + connection.Cost;

				var neighbor = connection.Destination;
				var neighborInfo = Graph[neighbor];

				// Cost is even higher; next direction:
				if (neighborInfo.Status == CellStatus.Closed ||
					(neighborInfo.Status == CellStatus.Open && costSoFarToNeighbor >= neighborInfo.CostSoFar))
					continue;

				// Now we may seriously consider this direction using heuristics. If the cell has
				// already been processed, we can reuse the result (just the difference between the
				// estimated total and the cost so far)
				int estimatedRemainingCostToTarget;
				if (neighborInfo.Status == CellStatus.Open)
					estimatedRemainingCostToTarget = neighborInfo.EstimatedTotalCost - neighborInfo.CostSoFar;
				else
					estimatedRemainingCostToTarget = heuristic(neighbor) * heuristicWeightPercentage / 100;

				var estimatedTotalCostToTarget = costSoFarToNeighbor + estimatedRemainingCostToTarget;
				Graph[neighbor] = new CellInfo(CellStatus.Open, costSoFarToNeighbor, estimatedTotalCostToTarget, currentMinNode);

				if (neighborInfo.Status != CellStatus.Open)
					openQueue.Add(new GraphConnection(neighbor, estimatedTotalCostToTarget));
			}

			return currentMinNode;
		}

		/// <summary>
		/// Determines if <paramref name="location"/> is the target of the search.
		/// </summary>
		public bool IsTarget(CPos location)
		{
			return TargetPredicate(location);
		}

		public void Dispose()
		{
			Graph.Dispose();
		}
	}
}
