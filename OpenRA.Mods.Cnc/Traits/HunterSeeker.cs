﻿#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits
{
	sealed class HunterSeekerInfo : TraitInfo
	{
		[Desc("Valid target relationships.")]
		public readonly PlayerRelationship TargetRelationships = PlayerRelationship.Enemy;
		public override object Create(ActorInitializer init) { return new HunterSeeker(this); }
	}

	sealed class HunterSeeker : INotifyAddedToWorld, INotifyBecomingIdle, ITick
	{
		Actor target;
		readonly HunterSeekerInfo info;

		public HunterSeeker(HunterSeekerInfo info)
		{
			this.info = info;
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			SelectNewTarget(self);
		}

		void INotifyBecomingIdle.OnBecomingIdle(Actor self)
		{
			if (target == null)
				return;

			if (TargetIsInvalid())
				SelectNewTarget(self);
		}

		bool TargetIsInvalid()
		{
			return target.Disposed || target.IsDead || !target.IsInWorld;
		}

		void SelectNewTarget(Actor self)
		{
			target = self.World.Actors.Where(x => info.TargetRelationships.HasFlag(self.Owner.RelationshipWith(x.Owner)) && x.IsTargetableBy(self)).RandomOrDefault(self.World.SharedRandom);
			if (target != null)
				self.QueueActivity(false, new FlyAttack(self, Common.Traits.AttackSource.AutoTarget, Target.FromTrackedActor(target), forceAttack: false, null));
		}

		void ITick.Tick(Actor self)
		{
			if (self.IsDead)
				return;

			if (target == null || TargetIsInvalid())
				SelectNewTarget(self);
		}
	}
}
