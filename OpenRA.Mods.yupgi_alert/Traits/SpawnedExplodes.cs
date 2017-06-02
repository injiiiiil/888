#region Copyright & License Information
/*
 * CnP of Explodes.cs by BoolBada of OP Mod
 * I could modify Explodes.cs and add exceptions there but then Mods.Common gets dependency on Mods.yupgi_alert.
 * 
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Yupgi_alert.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("This actor explodes when killed and the kill XP goes to the Spawner.")]
	public class SpawnedExplodesInfo : ITraitInfo, IRulesetLoaded, Requires<HealthInfo>, Requires<SpawnedInfo>
	{
		[WeaponReference, FieldLoader.Require, Desc("Default weapon to use for explosion if ammo/payload is loaded.")]
		public readonly string Weapon = "UnitExplode";

		[WeaponReference, Desc("Fallback weapon to use for explosion if empty (no ammo/payload).")]
		public readonly string EmptyWeapon = "UnitExplode";

		[Desc("Chance that the explosion will use Weapon instead of EmptyWeapon when exploding, provided the actor has ammo/payload.")]
		public readonly int LoadedChance = 100;

		[Desc("Chance that this actor will explode at all.")]
		public readonly int Chance = 100;

		[Desc("Health level at which actor will explode.")]
		public readonly int DamageThreshold = 0;

		[Desc("DeathType(s) that trigger the explosion. Leave empty to always trigger an explosion.")]
		public readonly HashSet<string> DeathTypes = new HashSet<string>();

		[Desc("Possible values are CenterPosition (explosion at the actors' center) and ",
			"Footprint (explosion on each occupied cell).")]
		public readonly ExplosionType Type = ExplosionType.CenterPosition;

		public WeaponInfo WeaponInfo { get; private set; }
		public WeaponInfo EmptyWeaponInfo { get; private set; }

		public object Create(ActorInitializer init) { return new SpawnedExplodes(this, init.Self); }
		public void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			WeaponInfo = string.IsNullOrEmpty(Weapon) ? null : rules.Weapons[Weapon.ToLowerInvariant()];
			EmptyWeaponInfo = string.IsNullOrEmpty(EmptyWeapon) ? null : rules.Weapons[EmptyWeapon.ToLowerInvariant()];
		}
	}

	public class SpawnedExplodes : INotifyKilled, INotifyDamage, INotifyCreated
	{
		readonly SpawnedExplodesInfo info;
		readonly Health health;
		BuildingInfo buildingInfo;

		public SpawnedExplodes(SpawnedExplodesInfo info, Actor self)
		{
			this.info = info;
			health = self.Trait<Health>();
		}

		void INotifyCreated.Created(Actor self)
		{
			buildingInfo = self.Info.TraitInfoOrDefault<BuildingInfo>();
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			if (!self.IsInWorld)
				return;

			if (self.World.SharedRandom.Next(100) > info.Chance)
				return;

			if (info.DeathTypes.Count > 0 && !e.Damage.DamageTypes.Overlaps(info.DeathTypes))
				return;

			var weapon = ChooseWeaponForExplosion(self);
			if (weapon == null)
				return;

			if (weapon.Report != null && weapon.Report.Any())
				Game.Sound.Play(SoundType.World, weapon.Report.Random(e.Attacker.World.SharedRandom), self.CenterPosition);

			if (info.Type == ExplosionType.Footprint && buildingInfo != null)
			{
				var cells = FootprintUtils.UnpathableTiles(self.Info.Name, buildingInfo, self.Location);
				foreach (var c in cells)
					weapon.Impact(Target.FromPos(self.World.Map.CenterOfCell(c)), e.Attacker, Enumerable.Empty<int>());

				return;
			}

			// Use .FromPos since this actor is killed. Cannot use Target.FromActor
			weapon.Impact(Target.FromPos(self.CenterPosition), self.Trait<Spawned>().Master, Enumerable.Empty<int>());
		}

		WeaponInfo ChooseWeaponForExplosion(Actor self)
		{
			var shouldExplode = self.TraitsImplementing<IExplodeModifier>().All(a => a.ShouldExplode(self));
			var useFullExplosion = self.World.SharedRandom.Next(100) <= info.LoadedChance;
			return (shouldExplode && useFullExplosion) ? info.WeaponInfo : info.EmptyWeaponInfo;
		}

		void INotifyDamage.Damaged(Actor self, AttackInfo e)
		{
			if (info.DamageThreshold == 0)
				return;

			if (health.HP * 100 < info.DamageThreshold * health.MaxHP)
				self.World.AddFrameEndTask(w => self.Kill(e.Attacker));
		}
	}
}
