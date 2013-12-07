﻿#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Effects;
using OpenRA.Graphics;

namespace OpenRA.Mods.RA.Effects
{
	public class Corpse : IEffect
	{
		readonly World world;
		readonly WPos pos;
		readonly CPos cell;
		readonly string paletteName;
		readonly Animation anim;

		public Corpse(World world, WPos pos, string image, string sequence, string paletteName, Action onComplete)
		{
			this.world = world;
			this.pos = pos;
			this.cell = pos.ToCPos();
			this.paletteName = paletteName;
			anim = new Animation(image);
			anim.PlayThen(sequence, () =>
			{
				world.AddFrameEndTask(w =>
				{
					w.Remove(this);
					if (onComplete != null)
						onComplete();
				});
			});
		}

		public void Tick(World world) { anim.Tick(); }

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			if (world.FogObscures(cell))
				return SpriteRenderable.None;

			return anim.Render(pos, wr.Palette(paletteName));
		}
	}
}
