#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Send actor to link to repair cursor.")]
	public class LinkOnRepairCursorInfo : TraitInfo, Requires<ILinkClientManagerInfo>
	{
		[Desc("Linking type")]
		public readonly BitSet<LinkType> Type = new("Repair");

		public override object Create(ActorInitializer init) { return new LinkOnRepairCursor(init.Self, this); }
	}

	public class LinkOnRepairCursor
	{
		protected readonly LinkClientManager Manager;
		protected readonly LinkOnRepairCursorInfo Info;

		public LinkOnRepairCursor(Actor self, LinkOnRepairCursorInfo info)
		{
			Info = info;
			Manager = self.Trait<LinkClientManager>();
		}

		public virtual Order GetDockOrder(Actor self, MouseInput mi)
		{
			if (Manager.LinkingPossible(Info.Type))
			{
				var linkHost = Manager.ClosestLinkHost(null, Info.Type, false, true);
				if (linkHost != null)
					return new Order("Link", self, Target.FromActor(linkHost.Value.Actor), Target.FromActor(self), mi.Modifiers.HasModifier(Modifiers.Shift));
			}

			return null;
		}
	}
}
