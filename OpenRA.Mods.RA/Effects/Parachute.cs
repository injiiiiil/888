#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Effects;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.Effects
{
	public class Parachute : IEffect
	{
		readonly Animation paraAnim;
		readonly WVec parachuteOffset;
		readonly Actor cargo;
		readonly Animation paraShadow;
		WPos pos;
		WPos dropPosition;
		WVec fallRate = new WVec(0, 0, 13);

		public Parachute(Actor cargo, WPos dropPosition)
		{
			this.cargo = cargo;

			var pai = cargo.Info.Traits.GetOrDefault<ParachuteAttachmentInfo>();
			cargo.Trait<ParachuteAttachment>().Activated = true;
			paraAnim = new Animation(cargo.World, pai != null ? pai.ParachuteSprite : "parach");
			paraAnim.PlayThen("open", () => paraAnim.PlayRepeating("idle"));

			paraShadow = new Animation(cargo.World, pai != null ? pai.ShadowSprite : "parach-shadow");
			paraShadow.PlayRepeating("idle");

			if (pai != null)
				parachuteOffset = pai.Offset;

			// Adjust x,y to match the target subcell
			cargo.Trait<IPositionable>().SetPosition(cargo, dropPosition.ToCPos());
			this.dropPosition = cargo.CenterPosition;
			pos = new WPos(cargo.CenterPosition.X, cargo.CenterPosition.Y, dropPosition.Z);
		}

		public void Tick(World world)
		{
			paraAnim.Tick();

			pos -= fallRate;

			if (pos.Z <= 0)
			{
				world.AddFrameEndTask(w =>
				{
					w.Remove(this);
					cargo.Trait<ParachuteAttachment>().Activated = false;
					cargo.CancelActivity();
					w.Add(cargo);

					foreach (var npl in cargo.TraitsImplementing<INotifyParachuteLanded>())
						npl.OnLanded();
				});
			}
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			var rc = cargo.Render(wr);

			// Don't render anything if the cargo is invisible (e.g. under fog)
			if (!rc.Any())
				yield break;

			foreach (var c in rc)
				yield return c.OffsetBy(pos - dropPosition);

			foreach (var r in paraShadow.Render(dropPosition, wr.Palette("shadow")))
				yield return r;

			foreach (var r in paraAnim.Render(pos, parachuteOffset, 1, rc.First().Palette, 1f))
				yield return r;
		}
	}
}
