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
		readonly int turnSpeed;
		int remainingTicks;

		BodyOrientation bodyOrientation;
		IFacing facing;
		IPosture posture;
		int lastFacing;
		int currentFacing;
		bool firstCall;
		WAngle currentRoll;
		readonly WAngle rollStep;

		public FlyIdle(Actor self, int ticks = -1, bool tickIdle = true)
		{
			aircraft = self.Trait<Aircraft>();
			turnSpeed = aircraft.Info.IdleTurnSpeed > -1 ? aircraft.Info.IdleTurnSpeed : aircraft.TurnSpeed;
			remainingTicks = ticks;

			bodyOrientation = self.TraitOrDefault<BodyOrientation>();
			facing = self.TraitOrDefault<IFacing>();
			posture = self.TraitOrDefault<IPosture>();
			currentFacing = lastFacing;
			firstCall = true;
			rollStep = WAngle.FromDegrees(5);
			if (facing != null)
				lastFacing = facing.Facing;

			if (bodyOrientation != null)
				currentRoll = posture.Posture.Roll - bodyOrientation.InitialPosture.Roll;

			if (tickIdle)
				tickIdles = self.TraitsImplementing<INotifyIdle>().ToArray();
		}

		public override bool Tick(Actor self)
		{
			if (remainingTicks == 0 || (NextActivity != null && remainingTicks < 0))
				return true;

			if (aircraft.ForceLanding || IsCanceling)
				return true;

			if (aircraft.Info.TurnInclinationRatio != 0 && bodyOrientation != null && facing != null)
			{
				currentFacing = facing.Facing;
				var diffFacing = currentFacing - lastFacing;
				if (!firstCall)
				{
					currentRoll = WAngle.ChangeByStep(currentRoll, diffFacing < 2 && diffFacing > -2 ? WAngle.Zero : WAngle.FromFacing((diffFacing) * -aircraft.Info.TurnInclinationRatio), rollStep);
					posture.Posture = bodyOrientation.InitialPosture + new WRot(currentRoll, WAngle.Zero, WAngle.Zero);
				}

				firstCall = false;
				lastFacing = currentFacing;
			}

			if (remainingTicks > 0)
				remainingTicks--;

			if (tickIdles != null)
				foreach (var tickIdle in tickIdles)
					tickIdle.TickIdle(self);

			if (!aircraft.Info.CanHover)
			{
				// We can't possibly turn this fast
				var desiredFacing = aircraft.Facing + 64;

				// This override is necessary, otherwise aircraft with CanSlide would circle sideways
				var move = aircraft.FlyStep(aircraft.Facing);

				Fly.FlyTick(self, aircraft, desiredFacing, aircraft.Info.CruiseAltitude, move, turnSpeed);
			}

			return false;
		}
	}
}
