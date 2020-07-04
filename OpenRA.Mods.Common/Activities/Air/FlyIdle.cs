#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Activities
{
	public class FlyIdle : Activity
	{
		readonly Aircraft aircraft;
		readonly INotifyIdle[] tickIdles;
		readonly bool idleTurn;
		int remainingTicks;

		public FlyIdle(Actor self, int ticks = -1, bool idleTurn = true)
		{
			aircraft = self.Trait<Aircraft>();
			remainingTicks = ticks;
			this.idleTurn = idleTurn;

			if (idleTurn)
				tickIdles = self.TraitsImplementing<INotifyIdle>().ToArray();
		}

		public override bool Tick(Actor self)
		{
			if (remainingTicks == 0 || (NextActivity != null && remainingTicks < 0))
				return true;

			if (aircraft.ForceLanding || IsCanceling)
				return true;

			if (remainingTicks > 0)
				remainingTicks--;

			if (tickIdles != null)
				foreach (var tickIdle in tickIdles)
					tickIdle.TickIdle(self);

			// We can't possibly turn this fast
			var desiredFacing = aircraft.IdleSpeed > 0 ? aircraft.FlightFacing + new WAngle(256) : aircraft.FlightFacing;
			Fly.FlyTick(self, aircraft, desiredFacing, aircraft.Info.CruiseAltitude, idleTurn: idleTurn,
				desiredBodyFacing: aircraft.FlightFacing, desiredSpeed: aircraft.IdleSpeed);

			return false;
		}
	}
}
