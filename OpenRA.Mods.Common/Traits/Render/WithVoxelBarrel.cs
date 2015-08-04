#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class WithVoxelBarrelInfo : ITraitInfo, IRenderActorPreviewVoxelsInfo, Requires<RenderVoxelsInfo>, Requires<ArmamentInfo>, Requires<TurretedInfo>
	{
		[Desc("Voxel sequence name to use")]
		public readonly string Sequence = "barrel";
		[Desc("Armament to use for recoil")]
		public readonly string Armament = "primary";
		[Desc("Visual offset")]
		public readonly WVec LocalOffset = WVec.Zero;

		public object Create(ActorInitializer init) { return new WithVoxelBarrel(init.Self, this); }

		public IEnumerable<VoxelAnimation> RenderPreviewVoxels(ActorPreviewInitializer init, RenderVoxelsInfo rv, string image, WRot orientation, int facings, PaletteReference p)
		{
			var body = init.Actor.TraitInfo<BodyOrientationInfo>();
			var armament = init.Actor.Traits.WithInterface<ArmamentInfo>()
				.First(a => a.Name == Armament);
			var t = init.Actor.Traits.WithInterface<TurretedInfo>()
				.First(tt => tt.Turret == armament.Turret);

			var voxel = VoxelProvider.GetVoxel(image, Sequence);

			var turretFacing = init.Contains<TurretFacingInit>() ? init.Get<TurretFacingInit, int>() : t.InitialFacing;
			var turretOrientation = body.QuantizeOrientation(new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(turretFacing) - orientation.Yaw), facings);
			var turretOffset = body.LocalToWorld(t.Offset.Rotate(orientation));

			yield return new VoxelAnimation(voxel, () => turretOffset, () => new[] { turretOrientation, orientation },
				() => false, () => 0);
		}
	}

	public class WithVoxelBarrel
	{
		readonly WithVoxelBarrelInfo info;
		readonly Actor self;
		readonly Armament armament;
		readonly Turreted turreted;
		readonly IBodyOrientation body;

		public WithVoxelBarrel(Actor self, WithVoxelBarrelInfo info)
		{
			this.self = self;
			this.info = info;
			body = self.Trait<IBodyOrientation>();
			armament = self.Traits<Armament>()
				.First(a => a.Info.Name == info.Armament);
			turreted = self.Traits<Turreted>()
				.First(tt => tt.Name == armament.Info.Turret);

			var rv = self.Trait<RenderVoxels>();
			rv.Add(new VoxelAnimation(VoxelProvider.GetVoxel(rv.Image, info.Sequence),
				BarrelOffset, BarrelRotation,
				() => false, () => 0));
		}

		WVec BarrelOffset()
		{
			var localOffset = info.LocalOffset + new WVec(-armament.Recoil, WDist.Zero, WDist.Zero);
			var turretOffset = turreted != null ? turreted.Position(self) : WVec.Zero;
			var turretOrientation = turreted != null ? turreted.LocalOrientation(self) : WRot.Zero;

			var quantizedBody = body.QuantizeOrientation(self, self.Orientation);
			var quantizedTurret = body.QuantizeOrientation(self, turretOrientation);
			return turretOffset + body.LocalToWorld(localOffset.Rotate(quantizedTurret).Rotate(quantizedBody));
		}

		IEnumerable<WRot> BarrelRotation()
		{
			var b = self.Orientation;
			var qb = body.QuantizeOrientation(self, b);
			yield return turreted.LocalOrientation(self) + WRot.FromYaw(b.Yaw - qb.Yaw);
			yield return qb;
		}
	}
}
