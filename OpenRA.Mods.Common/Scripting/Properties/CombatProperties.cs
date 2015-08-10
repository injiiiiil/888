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
using System.Linq;
using Eluant;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Scripting
{
	[ScriptPropertyGroup("Combat")]
	public class CombatProperties : ScriptActorProperties, Requires<AttackBaseInfo>, Requires<IMoveInfo>
	{
		readonly IMove move;
		readonly AttackBase attackBase;

		public CombatProperties(ScriptContext context, Actor self)
			: base(context, self)
		{
			move = self.Trait<IMove>();
			attackBase = self.Trait<AttackBase>();
		}

		[ScriptActorPropertyActivity]
		[Desc("Seek out and attack nearby targets.")]
		public void Hunt()
		{
			Self.QueueActivity(new Hunt(Self));
		}

		[ScriptActorPropertyActivity]
		[Desc("Move to a cell, but stop and attack anything within range on the way. " +
			"closeEnough defines an optional range (in cells) that will be considered " +
			"close enough to complete the activity.")]
		public void AttackMove(CPos cell, int closeEnough = 0)
		{
			Self.QueueActivity(new AttackMoveActivity(Self, move.MoveTo(cell, closeEnough)));
		}

		[ScriptActorPropertyActivity]
		[Desc("Patrol along a set of given waypoints.  The action is repeated by default, " +
			"and the actor will wait for `wait` ticks at each waypoint.")]
		public void Patrol(CPos[] waypoints, bool loop = true, int wait = 0)
		{
			foreach (var wpt in waypoints)
			{
				Self.QueueActivity(new AttackMoveActivity(Self, move.MoveTo(wpt, 2)));
				Self.QueueActivity(new Wait(wait));
			}

			if (loop)
				Self.QueueActivity(new CallFunc(() => Patrol(waypoints, loop, wait)));
		}

		[ScriptActorPropertyActivity]
		[Desc("Patrol along a set of given waypoints until a condition becomes true. " +
			"The actor will wait for `wait` ticks at each waypoint.")]
		public void PatrolUntil(CPos[] waypoints, LuaFunction func, int wait = 0)
		{
			Patrol(waypoints, false, wait);

			var repeat = func.Call(Self.ToLuaValue(Context)).First().ToBoolean();
			if (repeat)
				using (var f = func.CopyReference() as LuaFunction)
					Self.QueueActivity(new CallFunc(() => PatrolUntil(waypoints, f, wait)));
		}

		[Desc("Attack the target actor. The target actor needs to be visible.")]
		public void Attack(Actor targetActor, bool allowMove = true)
		{
			var target = Target.FromActor(targetActor);
			if (!target.IsValidFor(Self) || target.Type == TargetType.FrozenActor)
				Log.Write("lua", "{1} is an invalid target for {0}!", Self, targetActor);

			if (!targetActor.Info.TraitInfosAny<FrozenUnderFogInfo>() && !Self.Owner.CanTargetActor(targetActor))
				Log.Write("lua", "{1} is not revealed for player {0}!", Self.Owner, targetActor);

			attackBase.AttackTarget(target, true, allowMove);
		}
	}
}
