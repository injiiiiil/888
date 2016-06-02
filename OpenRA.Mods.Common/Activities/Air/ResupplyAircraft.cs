#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Activities
{
	public class ResupplyAircraft : Activity
	{
		readonly Aircraft aircraft;

		public ResupplyAircraft(Actor self)
		{
			aircraft = self.Trait<Aircraft>();
		}

		public override Activity Tick(Actor self)
		{
			var host = aircraft.GetActorBelow();

			if (host == null)
				return NextActivity;

			if (aircraft.IsPlane)
				return ActivityUtils.SequenceActivities (
					aircraft.GetResupplyActivities(host).
					Append(new Execute(() => aircraft.UnReserve())).
					Append(new WaitFor(() => Reservable.IsReserved(host))).
					Append(new TakeOff(self)).
					Append(NextActivity).ToArray());

			// If is helicopter move alway as soon as the resupply ends
			return ActivityUtils.SequenceActivities(
				aircraft.GetResupplyActivities(host).Append(new TakeOff(self)).Append(NextActivity).ToArray());
		}
	}
}
