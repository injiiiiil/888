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
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Markup;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class FirePort
	{
		public WVec Offset;
		public WAngle Yaw;
		public WAngle Cone;
	}

	[Desc("Cargo can fire their weapons out of fire ports.")]
	public class AttackGarrisonedInfo : AttackFollowInfo, IRulesetLoaded, Requires<CargoInfo>
	{
		[FieldLoader.Require]
		[Desc("Fire port offsets in local coordinates.")]
		public readonly WVec[] PortOffsets = null;

		[FieldLoader.Require]
		[Desc("Fire port yaw angles.")]
		public readonly WAngle[] PortYaws = null;

		[FieldLoader.Require]
		[Desc("Fire port yaw cone angle.")]
		public readonly WAngle[] PortCones = null;

		public FirePort[] Ports { get; private set; }

		[PaletteReference] public readonly string MuzzlePalette = "effect";

		public override object Create(ActorInitializer init) { return new AttackGarrisoned(init.Self, this); }
		public void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (PortOffsets.Length == 0)
				throw new YamlException("PortOffsets must have at least one entry.");

			if (PortYaws.Length != PortOffsets.Length)
				throw new YamlException("PortYaws must define an angle for each port.");

			if (PortCones.Length != PortOffsets.Length)
				throw new YamlException("PortCones must define an angle for each port.");

			Ports = new FirePort[PortOffsets.Length];

			for (var i = 0; i < PortOffsets.Length; i++)
			{
				Ports[i] = new FirePort
				{
					Offset = PortOffsets[i],
					Yaw = PortYaws[i],
					Cone = PortCones[i],
				};
			}
		}
	}

	public class AttackGarrisoned : AttackFollow, INotifyPassengerEntered, INotifyPassengerExited, IRender
	{
		public readonly new AttackGarrisonedInfo Info;
		Lazy<BodyOrientation> coords;
		List<Armament> armaments;
		List<AnimationWithOffset> muzzles;
		Dictionary<Actor, IFacing> paxFacing;
		Dictionary<Actor, IPositionable> paxPos;
		Dictionary<Actor, RenderSprites> paxRender;

		public AttackGarrisoned(Actor self, AttackGarrisonedInfo info)
			: base(self, info)
		{
			Info = info;
			coords = Exts.Lazy(() => self.Trait<BodyOrientation>());
			armaments = new List<Armament>();
			muzzles = new List<AnimationWithOffset>();
			paxFacing = new Dictionary<Actor, IFacing>();
			paxPos = new Dictionary<Actor, IPositionable>();
			paxRender = new Dictionary<Actor, RenderSprites>();

			getArmaments = () => armaments;
		}

		public void PassengerEntered(Actor self, Actor passenger)
		{
			paxFacing.Add(passenger, passenger.Trait<IFacing>());
			paxPos.Add(passenger, passenger.Trait<IPositionable>());
			paxRender.Add(passenger, passenger.Trait<RenderSprites>());
			armaments.AddRange(
				passenger.TraitsImplementing<Armament>()
				.Where(a => Info.Armaments.Contains(a.Info.Name)));
		}

		public void PassengerExited(Actor self, Actor passenger)
		{
			paxFacing.Remove(passenger);
			paxPos.Remove(passenger);
			paxRender.Remove(passenger);
			armaments.RemoveAll(a => a.Actor == passenger);
		}

		FirePort SelectFirePort(Actor self, WAngle targetYaw)
		{
			// Pick a random port that faces the target
			var bodyYaw = facing.Value != null ? WAngle.FromFacing(facing.Value.Facing) : WAngle.Zero;
			var indices = Enumerable.Range(0, Info.Ports.Length).Shuffle(self.World.SharedRandom);
			foreach (var i in indices)
			{
				var yaw = bodyYaw + Info.Ports[i].Yaw;
				var leftTurn = (yaw - targetYaw).Angle;
				var rightTurn = (targetYaw - yaw).Angle;
				if (Math.Min(leftTurn, rightTurn) <= Info.Ports[i].Cone.Angle)
					return Info.Ports[i];
			}

			return null;
		}

		WVec PortOffset(Actor self, FirePort p)
		{
			var bodyOrientation = coords.Value.QuantizeOrientation(self, self.Orientation);
			return coords.Value.LocalToWorld(p.Offset.Rotate(bodyOrientation));
		}

		public override void DoAttack(Actor self, Target target, IEnumerable<Armament> armaments = null)
		{
			if (!CanAttack(self, target))
				return;

			var pos = self.CenterPosition;
			var targetYaw = (target.CenterPosition - self.CenterPosition).Yaw;

			foreach (var a in Armaments)
			{
				var port = SelectFirePort(self, targetYaw);
				if (port == null)
					return;

				var muzzleFacing = targetYaw.Angle / 4;
				paxFacing[a.Actor].Facing = muzzleFacing;
				paxPos[a.Actor].SetVisualPosition(a.Actor, pos + PortOffset(self, port));

				var barrel = a.CheckFire(a.Actor, facing.Value, target);
				if (barrel != null && a.Info.MuzzleSequence != null)
				{
					// Muzzle facing is fixed once the firing starts
					var muzzleAnim = new Animation(self.World, paxRender[a.Actor].GetImage(a.Actor), () => muzzleFacing);
					var sequence = a.Info.MuzzleSequence;

					if (a.Info.MuzzleSplitFacings > 0)
						sequence += Util.QuantizeFacing(muzzleFacing, a.Info.MuzzleSplitFacings).ToString();

					var muzzleFlash = new AnimationWithOffset(muzzleAnim,
						() => PortOffset(self, port),
						() => false,
						p => RenderUtils.ZOffsetFromCenter(self, p, 1024));

					muzzles.Add(muzzleFlash);
					muzzleAnim.PlayThen(sequence, () => muzzles.Remove(muzzleFlash));
				}

				foreach (var npa in self.TraitsImplementing<INotifyAttack>())
					npa.Attacking(self, target, a, barrel);
			}
		}

		public IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
		{
			var pal = wr.Palette(Info.MuzzlePalette);

			// Display muzzle flashes
			foreach (var m in muzzles)
				foreach (var r in m.Render(self, wr, pal, 1f))
					yield return r;
		}

		public override void Tick(Actor self)
		{
			base.Tick(self);

			// Take a copy so that Tick() can remove animations
			foreach (var m in muzzles.ToArray())
				m.Animation.Tick();
		}
	}
}
