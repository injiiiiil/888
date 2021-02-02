#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Modifies the production cost of this actor for a specific queue or when a prerequisite is granted.")]
	public class ProductionCostMultiplierInfo : TraitInfo<ProductionCostMultiplier>, IProductionCostModifierInfo
	{
		[Desc("Percentage modifier to apply.")]
		public readonly int Multiplier = 100;

		[Desc("Only apply this cost change if the owner has these prerequisites.")]
		public readonly string[] Prerequisites = { };

		[Desc("Production queues that this cost will apply to.")]
		public readonly HashSet<string> Queues = new HashSet<string>();

		int IProductionCostModifierInfo.GetProductionCostModifier(World world, ActorInfo actorInfo, TechTree techTree, string queue)
		{
			if ((Queues.Count == 0 || Queues.Contains(queue)) && (Prerequisites.Length == 0 || techTree.HasPrerequisites(Prerequisites)))
				return Multiplier;

			return 100;
		}
	}

	public class ProductionCostMultiplier { }
}
