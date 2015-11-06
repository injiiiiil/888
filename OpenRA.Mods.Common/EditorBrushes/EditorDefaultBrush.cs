﻿#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Orders;
using OpenRA.Primitives;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public interface IEditorBrush : IDisposable
	{
		bool HandleMouseInput(MouseInput mi);
		void Tick();
	}

	public sealed class EditorDefaultBrush : IEditorBrush
	{
		public readonly ActorInfo Actor;

		readonly WorldRenderer worldRenderer;
		readonly World world;
		readonly EditorViewportControllerWidget editorWidget;
		readonly EditorActorLayer editorLayer;
		readonly Dictionary<int, ResourceType> resources;

		public EditorDefaultBrush(EditorViewportControllerWidget editorWidget, WorldRenderer wr)
		{
			this.editorWidget = editorWidget;
			worldRenderer = wr;
			world = wr.World;

			editorLayer = world.WorldActor.Trait<EditorActorLayer>();
			resources = world.WorldActor.TraitsImplementing<ResourceType>()
				.ToDictionary(r => r.Info.ResourceType, r => r);
		}

		public bool HandleMouseInput(MouseInput mi)
		{
			// Exclusively uses mouse wheel and right mouse buttons, but nothing else
			// Mouse move events are important for tooltips, so we always allow these through
			if ((mi.Button != MouseButton.Right && mi.Event != MouseInputEvent.Move && mi.Event != MouseInputEvent.Scroll) ||
				mi.Event == MouseInputEvent.Down)
				return false;

			var cell = worldRenderer.Viewport.ViewToWorld(mi.Location);

			var underCursor = editorLayer.PreviewsAt(worldRenderer.Viewport.ViewToWorldPx(mi.Location))
				.FirstOrDefault();

			var mapResources = world.Map.MapResources.Value;
			ResourceType type;
			if (underCursor != null)
				editorWidget.SetTooltip(underCursor);
			else if (mapResources.Contains(cell) && resources.TryGetValue(mapResources[cell].Type, out type))
				editorWidget.SetTooltip(type.Info.Name);
			else
				editorWidget.RemoveTooltip();

			// Finished with mouse move events, so let them bubble up the widget tree
			if (mi.Event == MouseInputEvent.Move)
				return false;

			if (mi.Button == MouseButton.Right)
			{
				editorWidget.RemoveTooltip();

				if (underCursor != null)
					editorLayer.Remove(underCursor);

				if (mapResources.Contains(cell) && mapResources[cell].Type != 0)
					mapResources[cell] = new ResourceTile();
			}
			else if (mi.Event == MouseInputEvent.Scroll)
			{
				if (underCursor != null)
				{
					// Test case / demonstration of how to edit an existing actor
					var facing = underCursor.Init<FacingInit>();
					if (facing != null)
						underCursor.ReplaceInit(new FacingInit((facing.Value(world) + mi.ScrollDelta) % 256));
					else if (underCursor.Info.HasTraitInfo<UsesInit<FacingInit>>())
						underCursor.ReplaceInit(new FacingInit(mi.ScrollDelta));

					var turret = underCursor.Init<TurretFacingInit>();
					if (turret != null)
						underCursor.ReplaceInit(new TurretFacingInit((turret.Value(world) + mi.ScrollDelta) % 256));
					else if (underCursor.Info.HasTraitInfo<UsesInit<TurretFacingInit>>())
						underCursor.ReplaceInit(new TurretFacingInit(mi.ScrollDelta));
				}
			}

			return true;
		}

		public void Tick() { }
		public void Dispose() { }
	}
}
