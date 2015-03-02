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

namespace OpenRA.Traits
{
	public class CreatesShroudInfo : ITraitInfo
	{
		public readonly WRange Range = WRange.Zero;
		public object Create(ActorInitializer init) { return new CreatesShroud(this); }
	}

	public class CreatesShroud : ITick, ISync
	{
		CreatesShroudInfo info;
		[Sync] CPos cachedLocation;
		[Sync] bool cachedDisabled;

		public CreatesShroud(CreatesShroudInfo info)
		{
			this.info = info;
		}

		public void Tick(Actor self)
		{
			var disabled = self.TraitsImplementing<IDisable>().Any(d => d.Disabled);
			if (cachedLocation != self.Location || cachedDisabled != disabled)
			{
				cachedLocation = self.Location;
				cachedDisabled = disabled;
				Shroud.UpdateShroudGeneration(self.World.Players.Select(p => p.Shroud), self);
			}
		}

		public WRange Range { get { return cachedDisabled ? WRange.Zero : info.Range; } }
	}
}