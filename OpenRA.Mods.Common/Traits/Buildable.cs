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

using System;
using System.Collections.Generic;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Traits
{
	public class BuildableInfo : TraitInfo<Buildable>
	{
		[Desc("The prerequisite names that must be available before this can be built.",
			"This can be prefixed with ! to invert the prerequisite (disabling production if the prerequisite is available)",
			"and/or ~ to hide the actor from the production palette if the prerequisite is not available.",
			"Prerequisites are granted by actors with the ProvidesPrerequisite trait.")]
		public readonly string[] Prerequisites = Array.Empty<string>();

		[Desc("Production queue(s) that can produce this.")]
		public readonly HashSet<string> Queue = new();

		[Desc("Override the production structure type (from the Production Produces list) that this unit should be built at.")]
		public readonly string BuildAtProductionType = null;

		[Desc("Disable production when there are more than this many of this actor on the battlefield. Set to 0 to disable.")]
		public readonly int BuildLimit = 0;

		[Desc("Force a specific faction variant, overriding the faction of the producing actor.")]
		public readonly string ForceFaction = null;

		[SequenceReference(hasFactionSuffix: true)]
		[Desc("Sequence of the actor that contains the icon.")]
		public readonly string Icon = "icon";

		[Desc("Should faction suffix be added for sequence " + nameof(Icon) + "?")]
		public readonly bool AddIconFactionSuffix = false;

		[PaletteReference(nameof(IconPaletteIsPlayerPalette))]
		[Desc("Palette used for the production icon.")]
		public readonly string IconPalette = "chrome";

		[Desc("Custom palette is a player palette BaseName")]
		public readonly bool IconPaletteIsPlayerPalette = false;

		[Desc("Base build time in frames (-1 indicates to use the unit's Value).")]
		public readonly int BuildDuration = -1;

		[Desc("Percentage modifier to apply to the build duration.")]
		public readonly int BuildDurationModifier = 60;

		[Desc("Sort order for the production palette. Smaller numbers are presented earlier.")]
		public readonly int BuildPaletteOrder = 9999;

		[Desc("Text shown in the production tooltip.")]
		public readonly string Description = "";

		public static string GetInitialFaction(ActorInfo ai, string defaultFaction)
		{
			return ai.TraitInfoOrDefault<BuildableInfo>()?.ForceFaction ?? defaultFaction;
		}

		public string GetIconSequence(World world, string faction, string image)
		{
			if (AddIconFactionSuffix)
			{
				if (ChromeMetrics.TryGet("FactionSuffix-" + faction, out string factionOverride))
					faction = factionOverride;

				var factionSequence = $"{Icon}.{faction}";
				if (world.Map.Sequences.HasSequence(image, factionSequence))
					return factionSequence;
			}

			return Icon;
		}
	}

	public class Buildable { }
}
