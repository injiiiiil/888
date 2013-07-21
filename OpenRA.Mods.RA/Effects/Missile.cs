﻿#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Effects;
using OpenRA.FileFormats;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.Effects
{
	class MissileInfo : IProjectileInfo
	{
		public readonly int Speed = 1;
		public readonly int Arm = 0;
		[Desc("Check for whether an actor with Wall: trait blocks fire")]
		public readonly bool High = false;
		public readonly bool Shadow = true;
		public readonly bool Proximity = false;
		public readonly string Trail = null;
		public readonly float Inaccuracy = 0;
		public readonly string Image = null;
		[Desc("Rate of Turning")]
		public readonly int ROT = 5;
		[Desc("Explode when following the target longer than this.")]
		public readonly int RangeLimit = 0;
		public readonly bool TurboBoost = false;
		public readonly int TrailInterval = 2;
		public readonly int ContrailLength = 0;
		public readonly Color ContrailColor = Color.White;
		public readonly bool ContrailUsePlayerColor = false;
		public readonly int ContrailDelay = 1;
		public readonly bool Jammable = true;

		public IEffect Create(ProjectileArgs args) { return new Missile(this, args); }
	}

	class Missile : IEffect
	{
		readonly MissileInfo Info;
		readonly ProjectileArgs Args;

		PVecInt offset;
		public PSubPos SubPxPosition;
		public PPos PxPosition { get { return SubPxPosition.ToPPos(); } }

		readonly Animation anim;
		int Facing;
		int t;
		int Altitude;
		ContrailRenderable Trail;

		public Missile(MissileInfo info, ProjectileArgs args)
		{
			Info = info;
			Args = args;

			SubPxPosition = Args.src.ToPSubPos();
			Altitude = Args.srcAltitude;
			Facing = Args.facing;

			if (info.Inaccuracy > 0)
				offset = (PVecInt)(info.Inaccuracy * args.firedBy.World.SharedRandom.Gauss2D(2)).ToInt2();

			if (Info.Image != null)
			{
				anim = new Animation(Info.Image, () => Facing);
				anim.PlayRepeating("idle");
			}

			if (Info.ContrailLength > 0)
			{
				var color = Info.ContrailUsePlayerColor ? ContrailRenderable.ChooseColor(args.firedBy) : Info.ContrailColor;
				Trail = new ContrailRenderable(args.firedBy.World, color, Info.ContrailLength, Info.ContrailDelay, 0);
			}
		}

		// In pixels
		const int MissileCloseEnough = 7;
		int ticksToNextSmoke;

		public void Tick(World world)
		{
			t += 40;

			// In pixels
			var dist = Args.target.CenterLocation + offset - PxPosition;

			var targetAltitude = 0;
			if (Args.target.IsValid)
				targetAltitude = Args.target.CenterPosition.Z * Game.CellSize / 1024;

			var jammed = Info.Jammable && world.ActorsWithTrait<JamsMissiles>().Any(tp =>
				(tp.Actor.CenterLocation - PxPosition).ToCVec().Length <= tp.Trait.Range

				&& (tp.Actor.Owner.Stances[Args.firedBy.Owner] != Stance.Ally
				|| (tp.Actor.Owner.Stances[Args.firedBy.Owner] == Stance.Ally && tp.Trait.AlliedMissiles))

				&& world.SharedRandom.Next(100 / tp.Trait.Chance) == 0);

			if (!jammed)
			{
				Altitude += Math.Sign(targetAltitude - Altitude);
				if (Args.target.IsValid)
					Facing = Traits.Util.TickFacing(Facing,
						Traits.Util.GetFacing(dist, Facing),
						Info.ROT);
			}
			else
			{
				Altitude += world.SharedRandom.Next(-1, 2);
				Facing = Traits.Util.TickFacing(Facing,
					Facing + world.SharedRandom.Next(-20, 21),
					Info.ROT);
			}

			anim.Tick();

			if (dist.LengthSquared < MissileCloseEnough * MissileCloseEnough && Args.target.IsValid)
				Explode(world);

			// TODO: Replace this with a lookup table
			var dir = (-float2.FromAngle((float)(Facing / 128f * Math.PI))).ToPSubVec();

			var move = Info.Speed * dir;
			if (targetAltitude > 0 && Info.TurboBoost)
				move = (move * 3) / 2;
			move = move / 5;

			SubPxPosition += move;

			if (Info.Trail != null)
			{
				var sp = ((SubPxPosition - (move * 3) / 2)).ToPPos() - new PVecInt(0, Altitude);

				if (--ticksToNextSmoke < 0)
				{
					world.AddFrameEndTask(w => w.Add(new Smoke(w, sp.ToWPos(0), Info.Trail)));
					ticksToNextSmoke = Info.TrailInterval;
				}
			}

			if (Info.RangeLimit != 0 && t > Info.RangeLimit * 40)
				Explode(world);

			if (!Info.High)		// check for hitting a wall
			{
				var cell = PxPosition.ToCPos();
				if (world.ActorMap.GetUnitsAt(cell).Any(a => a.HasTrait<IBlocksBullets>()))
					Explode(world);
			}

			if (Info.ContrailLength > 0)
				Trail.Update(PxPosition.ToWPos(Altitude));
		}

		void Explode(World world)
		{
			world.AddFrameEndTask(w => w.Remove(this));
			Args.dest = PxPosition;
			if (t > Info.Arm * 40)	/* don't blow up in our launcher's face! */
				Combat.DoImpacts(Args);
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			if (Info.ContrailLength > 0)
				yield return Trail;

			if (!Args.firedBy.World.FogObscures(PxPosition.ToCPos()))
				yield return new SpriteRenderable(anim.Image, PxPosition.ToFloat2() - new float2(0, Altitude),
				                                  wr.Palette(Args.weapon.Underwater ? "shadow" : "effect"), PxPosition.Y);
		}
	}
}
