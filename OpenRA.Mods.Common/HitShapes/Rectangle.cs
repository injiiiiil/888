﻿#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.HitShapes
{
	public class RectangleShape : IHitShape
	{
		public WDist OuterRadius { get; private set; }

		[FieldLoader.Require]
		public readonly int2 TopLeft;

		[FieldLoader.Require]
		public readonly int2 BottomRight;

		[Desc("Defines the top offset relative to the actor's target point.")]
		public readonly int VerticalTopOffset = 0;

		[Desc("Defines the bottom offset relative to the actor's target point.")]
		public readonly int VerticalBottomOffset = 0;

		int2 quadrantSize;
		int2 center;

		WVec[] combatOverlayVertsTop;
		WVec[] combatOverlayVertsBottom;

		public RectangleShape() { }

		public RectangleShape(int2 tl, int2 br)
		{
			TopLeft = tl;
			BottomRight = br;
		}

		public void Initialize()
		{
			if (TopLeft.X >= BottomRight.X || TopLeft.Y >= BottomRight.Y)
				throw new YamlException("TopLeft and BottomRight points are invalid.");

			if (VerticalTopOffset < VerticalBottomOffset)
				throw new YamlException("VerticalTopOffset must be equal to or higher than VerticalBottomOffset.");

			quadrantSize = (BottomRight - TopLeft) / 2;
			center = TopLeft + quadrantSize;

			OuterRadius = new WDist(Math.Max(TopLeft.Length, BottomRight.Length));

			combatOverlayVertsTop = new WVec[]
			{
				new WVec(TopLeft.X, TopLeft.Y, VerticalTopOffset),
				new WVec(BottomRight.X, TopLeft.Y, VerticalTopOffset),
				new WVec(BottomRight.X, BottomRight.Y, VerticalTopOffset),
				new WVec(TopLeft.X, BottomRight.Y, VerticalTopOffset)
			};

			combatOverlayVertsBottom = new WVec[]
			{
				new WVec(TopLeft.X, TopLeft.Y, VerticalBottomOffset),
				new WVec(BottomRight.X, TopLeft.Y, VerticalBottomOffset),
				new WVec(BottomRight.X, BottomRight.Y, VerticalBottomOffset),
				new WVec(TopLeft.X, BottomRight.Y, VerticalBottomOffset)
			};
		}

		public WDist DistanceFromEdge(WVec v)
		{
			var r = new int2(
				Math.Max(Math.Abs(v.X - center.X) - quadrantSize.X, 0),
				Math.Max(Math.Abs(v.Y - center.Y) - quadrantSize.Y, 0));

			return new WDist(r.Length);
		}

		public WDist DistanceFromEdge(WPos pos, Actor actor)
		{
			var actorPos = actor.CenterPosition;
			var attackablePositions = actor.TraitsImplementing<IAttackablePositions>();
			if (attackablePositions.Any())
			{
				var positions = attackablePositions.SelectMany(ap => ap.AttackablePositions(actor));
				actorPos = positions.PositionClosestTo(actorPos);
			}

			if (pos.Z > actorPos.Z + VerticalTopOffset)
				return DistanceFromEdge((pos - (actorPos + new WVec(0, 0, VerticalTopOffset))).Rotate(-actor.Orientation));

			if (pos.Z < actorPos.Z + VerticalBottomOffset)
				return DistanceFromEdge((pos - (actorPos + new WVec(0, 0, VerticalBottomOffset))).Rotate(-actor.Orientation));

			return DistanceFromEdge((pos - new WPos(actorPos.X, actorPos.Y, pos.Z)).Rotate(-actor.Orientation));
		}

		public void DrawCombatOverlay(WorldRenderer wr, RgbaColorRenderer wcr, Actor actor)
		{
			var actorPos = actor.CenterPosition;
			var attackablePositions = actor.TraitsImplementing<IAttackablePositions>();
			if (attackablePositions.Any())
			{
				var positions = attackablePositions.SelectMany(ap => ap.AttackablePositions(actor));
				actorPos = positions.PositionClosestTo(actorPos);
			}

			var vertsTop = combatOverlayVertsTop.Select(v => wr.ScreenPosition(actorPos + v.Rotate(actor.Orientation)));
			var vertsBottom = combatOverlayVertsBottom.Select(v => wr.ScreenPosition(actorPos + v.Rotate(actor.Orientation)));
			wcr.DrawPolygon(vertsTop.ToArray(), 1, Color.Yellow);
			wcr.DrawPolygon(vertsBottom.ToArray(), 1, Color.Yellow);
		}
	}
}
