﻿#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.Traits
{
	[Desc("Wanders around aimlesly when idle.")]
	abstract class WandersInfo : ITraitInfo
	{
		public readonly int WanderMoveRadius = 10;

		[Desc("Number of ticks to wait until decreasing the effective move radius.")]
		public readonly int MoveReductionRadiusScale = 5;

		public abstract object Create(ActorInitializer init);
	}

	class Wanders : INotifyAddedToWorld, INotifyBecomingIdle
	{
		readonly Actor self;
		readonly WandersInfo info;

		int ticksIdle;
		int effectiveMoveRadius;

		public Wanders(Actor self, WandersInfo info)
		{
			this.self = self;
			this.info = info;
			effectiveMoveRadius = info.WanderMoveRadius;
		}

		public void AddedToWorld(Actor self)
		{
			OnBecomingIdle(self);
		}

		public void OnBecomingIdle(Actor self)
		{
			var targetPos = PickTargetLocation();
			if (targetPos != CPos.Zero)
				DoAction(self, targetPos);
		}

		CPos PickTargetLocation()
		{
			var target = self.CenterPosition + new WVec(0, -1024 * effectiveMoveRadius, 0).Rotate(WRot.FromFacing(self.World.SharedRandom.Next(255)));
			var targetCell = self.World.Map.CellContaining(target);

			if (!self.World.Map.Contains(targetCell))
			{
				// If MoveRadius is too big there might not be a valid cell to order the attack to (if actor is on a small island and can't leave)
				if (++ticksIdle % info.MoveReductionRadiusScale == 0)
					effectiveMoveRadius--;

				return CPos.Zero;  // We'll be back the next tick; better to sit idle for a few seconds than prolong this tick indefinitely with a loop
			}

			ticksIdle = 0;
			effectiveMoveRadius = info.WanderMoveRadius;

			return targetCell;
		}

		public virtual void DoAction(Actor self, CPos targetPos)
		{
			throw new NotImplementedException("Base class Wanders does not implement method DoAction!");
		}
	}
}
