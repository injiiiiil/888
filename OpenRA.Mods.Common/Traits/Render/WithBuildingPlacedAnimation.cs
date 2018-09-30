#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
	[Desc("Changes the animation when the actor constructed a building.")]
	public class WithBuildingPlacedAnimationInfo : ConditionalTraitInfo, Requires<WithSpriteBodyInfo>
	{
		[Desc("Sequence name to use"), SequenceReference]
		public readonly string Sequence = "build";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init) { return new WithBuildingPlacedAnimation(init.Self, this); }
	}

	public class WithBuildingPlacedAnimation : ConditionalTrait<WithBuildingPlacedAnimationInfo>, INotifyBuildingPlaced, INotifyBuildComplete, INotifySold, INotifyTransform
	{
		readonly WithSpriteBody wsb;
		bool buildComplete;

		public WithBuildingPlacedAnimation(Actor self, WithBuildingPlacedAnimationInfo info)
			: base(info)
		{
			wsb = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
			buildComplete = !self.Info.HasTraitInfo<BuildingInfo>();
		}

		void INotifyBuildComplete.BuildingComplete(Actor self)
		{
			buildComplete = true;
		}

		void INotifySold.Sold(Actor self) { }
		void INotifySold.Selling(Actor self)
		{
			buildComplete = false;
		}

		void INotifyTransform.BeforeTransform(Actor self)
		{
			buildComplete = false;
		}

		void INotifyTransform.OnTransform(Actor self) { }
		void INotifyTransform.AfterTransform(Actor self) { }

		void INotifyBuildingPlaced.BuildingPlaced(Actor self)
		{
			if (!IsTraitDisabled && buildComplete)
				wsb.PlayCustomAnimation(self, Info.Sequence);
		}
	}
}