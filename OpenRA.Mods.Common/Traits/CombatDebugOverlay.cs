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
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Effects;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Displays fireports, muzzle offsets, and hit areas in developer mode.")]
	public class CombatDebugOverlayInfo : ITraitInfo, InitializeAfter<AttackBaseInfo>, InitializeAfter<IBodyOrientationInfo>,
		InitializeAfter<HealthInfo>
	{
		public object Create(ActorInitializer init) { return new CombatDebugOverlay(init.Self); }
	}

	public class CombatDebugOverlay : IPostRender, INotifyDamage
	{
		readonly DeveloperMode devMode;

		readonly Health health;
		readonly AttackBase attack;
		readonly IBodyOrientation coords;

		public CombatDebugOverlay(Actor self)
		{
			health = self.TraitOrDefault<Health>();
			attack = self.TraitOrDefault<AttackBase>();
			coords = attack is AttackGarrisoned ? self.Trait<IBodyOrientation>() : null;

			var localPlayer = self.World.LocalPlayer;
			devMode = localPlayer != null ? localPlayer.PlayerActor.Trait<DeveloperMode>() : null;
		}

		public void RenderAfterWorld(WorldRenderer wr, Actor self)
		{
			if (devMode == null || !devMode.ShowCombatGeometry)
				return;

			if (health != null)
				wr.DrawRangeCircle(self.CenterPosition, health.Info.Radius, Color.Red);

			// No armaments to draw
			if (attack == null)
				return;

			var wlr = Game.Renderer.WorldLineRenderer;
			var c = Color.White;

			// Fire ports on garrisonable structures
			var garrison = attack as AttackGarrisoned;
			if (garrison != null)
			{
				var bodyOrientation = coords.QuantizeOrientation(self, self.Orientation);
				foreach (var p in garrison.Ports)
				{
					var pos = self.CenterPosition + coords.LocalToWorld(p.Offset.Rotate(bodyOrientation));
					var da = coords.LocalToWorld(new WVec(224, 0, 0).Rotate(WRot.FromYaw(p.Yaw + p.Cone)).Rotate(bodyOrientation));
					var db = coords.LocalToWorld(new WVec(224, 0, 0).Rotate(WRot.FromYaw(p.Yaw - p.Cone)).Rotate(bodyOrientation));

					var o = wr.ScreenPosition(pos);
					var a = wr.ScreenPosition(pos + da * 224 / da.Length);
					var b = wr.ScreenPosition(pos + db * 224 / db.Length);
					wlr.DrawLine(o, a, c);
					wlr.DrawLine(o, b, c);
				}

				return;
			}

			foreach (var a in attack.Armaments)
			{
				foreach (var b in a.Barrels)
				{
					var muzzle = self.CenterPosition + a.MuzzleOffset(self, b);
					var dirOffset = new WVec(0, -224, 0).Rotate(a.MuzzleOrientation(self, b));

					var sm = wr.ScreenPosition(muzzle);
					var sd = wr.ScreenPosition(muzzle + dirOffset);
					wlr.DrawLine(sm, sd, c);
					wr.DrawTargetMarker(c, sm);
				}
			}
		}

		public void Damaged(Actor self, AttackInfo e)
		{
			if (devMode == null || !devMode.ShowCombatGeometry || e.Damage == 0)
				return;

			if (health == null)
				return;

			var damageText = "{0} ({1}%)".F(-e.Damage, e.Damage * 100 / health.MaxHP);

			self.World.AddFrameEndTask(w => w.Add(new FloatingText(self.CenterPosition, e.Attacker.Owner.Color.RGB, damageText, 30)));
		}
	}
}
