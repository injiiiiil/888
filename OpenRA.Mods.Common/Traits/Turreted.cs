#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class TurretedInfo : ITraitInfo, UsesInit<TurretFacingInit>, Requires<IBodyOrientationInfo>,
		InitializeAfter<IBodyOrientationInfo>, InitializeAfter<IFacingInfo>
	{
		public readonly string Turret = "primary";
		[Desc("Rate of Turning")]
		public readonly int ROT = 255;
		public readonly int InitialFacing = 128;

		[Desc("Number of ticks before turret is realigned. (-1 turns off realignment)")]
		public readonly int RealignDelay = 40;

		[Desc("Muzzle position relative to turret or body. (forward, right, up) triples")]
		public readonly WVec Offset = WVec.Zero;

		public virtual object Create(ActorInitializer init) { return new Turreted(init, this); }
	}

	public class Turreted : ITick, ISync, INotifyCreated
	{
		readonly TurretedInfo info;
		readonly IFacing facing;
		readonly IBodyOrientation body;
		AttackTurreted attack;

		[Sync] public int QuantizedFacings = 0;
		[Sync] public int TurretFacing = 0;
		public int? DesiredFacing;
		int realignTick = 0;

		// For subclasses that want to move the turret relative to the body
		protected WVec localOffset = WVec.Zero;

		public WVec Offset { get { return info.Offset + localOffset; } }
		public string Name { get { return info.Turret; } }

		public static int GetInitialTurretFacing(ActorInitializer init, int def)
		{
			if (init.Contains<TurretFacingInit>())
				return init.Get<TurretFacingInit, int>();

			if (init.Contains<FacingInit>())
				return init.Get<FacingInit, int>();

			return def;
		}

		public Turreted(ActorInitializer init, TurretedInfo info)
		{
			this.info = info;
			TurretFacing = GetInitialTurretFacing(init, info.InitialFacing);
			facing = init.Self.TraitOrDefault<IFacing>();
			body = init.Self.Trait<IBodyOrientation>();
		}

		public void Created(Actor self)
		{
			attack = self.TraitOrDefault<AttackTurreted>();
		}

		public virtual void Tick(Actor self)
		{
			// NOTE: FaceTarget is called in AttackTurreted.CanAttack if the turret has a target.
			if (attack != null)
			{
				if (!attack.IsAttacking)
				{
					if (realignTick < info.RealignDelay)
						realignTick++;
					else if (info.RealignDelay > -1)
						DesiredFacing = null;

					MoveTurret();
				}
			}
			else
			{
				realignTick = 0;
				MoveTurret();
			}
		}

		void MoveTurret()
		{
			var df = DesiredFacing ?? (facing != null ? facing.Facing : TurretFacing);
			TurretFacing = Util.TickFacing(TurretFacing, df, info.ROT);
		}

		public bool FaceTarget(Actor self, Target target)
		{
			DesiredFacing = Util.GetFacing(target.CenterPosition - self.CenterPosition, TurretFacing);
			MoveTurret();
			return TurretFacing == DesiredFacing.Value;
		}

		// Turret offset in world-space
		public WVec Position(Actor self)
		{
			var bodyOrientation = body.QuantizeOrientation(self, self.Orientation);
			return body.LocalToWorld(Offset.Rotate(bodyOrientation));
		}

		// Orientation in unit-space
		public WRot LocalOrientation(Actor self)
		{
			// Hack: turretFacing is relative to the world, so subtract the body yaw
			var local = WRot.FromYaw(WAngle.FromFacing(TurretFacing) - self.Orientation.Yaw);

			if (QuantizedFacings == 0)
				return local;

			// Quantize orientation to match a rendered sprite
			// Implies no pitch or yaw
			var facing = Util.QuantizeFacing(local.Yaw.Angle / 4, QuantizedFacings) * (256 / QuantizedFacings);
			return new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(facing));
		}
	}
}
