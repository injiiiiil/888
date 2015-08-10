﻿#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Activities
{
	public class Parachute : Activity
	{
		readonly UpgradeManager um;
		readonly IPositionable pos;
		readonly ParachutableInfo para;
		readonly WVec fallVector;

		WPos dropPosition;
		WPos currentPosition;
		bool triggered = false;

		public Parachute(Actor self, WPos dropPosition)
		{
			um = self.TraitOrDefault<UpgradeManager>();
			pos = self.TraitOrDefault<IPositionable>();

			// Parachutable trait is a prerequisite for running this activity
			para = self.Info.TraitInfo<ParachutableInfo>();
			fallVector = new WVec(0, 0, para.FallRate);
			this.dropPosition = dropPosition;
		}

		Activity FirstTick(Actor self)
		{
			triggered = true;

			if (um != null)
				foreach (var u in para.ParachuteUpgrade)
					um.GrantUpgrade(self, u, this);

			// Place the actor and retrieve its visual position (CenterPosition)
			pos.SetPosition(self, dropPosition);
			currentPosition = self.CenterPosition;

			return this;
		}

		Activity LastTick(Actor self)
		{
			pos.SetPosition(self, currentPosition - new WVec(0, 0, currentPosition.Z));

			if (um != null)
				foreach (var u in para.ParachuteUpgrade)
					um.RevokeUpgrade(self, u, this);

			foreach (var npl in self.Traits<INotifyParachuteLanded>())
				npl.OnLanded();

			return NextActivity;
		}

		public override Activity Tick(Actor self)
		{
			// If this is the first tick
			if (!triggered)
				return FirstTick(self);

			currentPosition -= fallVector;

			// If the unit has landed, this will be the last tick
			if (currentPosition.Z <= 0)
				return LastTick(self);

			pos.SetVisualPosition(self, currentPosition);

			return this;
		}

		// Only the last queued activity (given order) is kept
		public override void Queue(Activity activity)
		{
			NextActivity = activity;
		}

		// Cannot be cancelled
		public override void Cancel(Actor self) { }
	}
}
