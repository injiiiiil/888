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

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.Common.Widgets
{
	public sealed class EditorResourceBrush : IEditorBrush
	{
		public readonly ResourceType ResourceType;

		readonly WorldRenderer worldRenderer;
		readonly World world;
		readonly EditorViewportControllerWidget editorWidget;
		readonly EditorActionManager editorActionManager;
		readonly EditorCursorLayer editorCursor;
		readonly IResourceLayer resourceLayer;
		readonly int cursorToken;

		AddResourcesEditorAction action;
		bool resourceAdded;

		public EditorResourceBrush(EditorViewportControllerWidget editorWidget, ResourceType resource, WorldRenderer wr)
		{
			this.editorWidget = editorWidget;
			ResourceType = resource;
			worldRenderer = wr;
			world = wr.World;
			editorActionManager = world.WorldActor.Trait<EditorActionManager>();
			editorCursor = world.WorldActor.Trait<EditorCursorLayer>();
			resourceLayer = world.WorldActor.Trait<IResourceLayer>();
			action = new AddResourcesEditorAction(world.Map, resourceLayer, resource);

			cursorToken = editorCursor.SetResource(wr, resource);
		}

		public bool HandleMouseInput(MouseInput mi)
		{
			// Exclusively uses left and right mouse buttons, but nothing else
			if (mi.Button != MouseButton.Left && mi.Button != MouseButton.Right)
				return false;

			if (mi.Button == MouseButton.Right)
			{
				if (mi.Event == MouseInputEvent.Up)
				{
					editorWidget.ClearBrush();
					return true;
				}

				return false;
			}

			if (editorCursor.CurrentToken != cursorToken)
				return false;

			var cell = worldRenderer.Viewport.ViewToWorld(mi.Location);

			if (mi.Button == MouseButton.Left && mi.Event != MouseInputEvent.Up && resourceLayer.CanAddResource(ResourceType, cell))
			{
				action.Add(new CellResource(cell, resourceLayer.GetResource(cell), ResourceType));
				resourceAdded = true;
			}
			else if (resourceAdded && mi.Button == MouseButton.Left && mi.Event == MouseInputEvent.Up)
			{
				editorActionManager.Add(action);
				action = new AddResourcesEditorAction(world.Map, resourceLayer, ResourceType);
				resourceAdded = false;
			}

			return true;
		}

		public void Tick() { }

		public void Dispose()
		{
			editorCursor.Clear(cursorToken);
		}
	}

	struct CellResource
	{
		public readonly CPos Cell;
		public readonly ResourceLayerContents OldResourceTile;
		public readonly ResourceType NewResourceType;

		public CellResource(CPos cell, ResourceLayerContents oldResourceTile, ResourceType newResourceType)
		{
			Cell = cell;
			OldResourceTile = oldResourceTile;
			NewResourceType = newResourceType;
		}
	}

	class AddResourcesEditorAction : IEditorAction
	{
		public string Text { get; private set; }

		readonly Map map;
		readonly IResourceLayer resourceLayer;
		readonly ResourceType resourceType;
		readonly List<CellResource> cellResources = new List<CellResource>();

		public AddResourcesEditorAction(Map map, IResourceLayer resourceLayer, ResourceType resourceType)
		{
			this.map = map;
			this.resourceLayer = resourceLayer;
			this.resourceType = resourceType;
		}

		public void Execute()
		{
		}

		public void Do()
		{
			foreach (var resourceCell in cellResources)
			{
				resourceLayer.ClearResources(resourceCell.Cell);
				resourceLayer.AddResource(resourceCell.NewResourceType, resourceCell.Cell, resourceCell.NewResourceType.Info.MaxDensity);
			}
		}

		public void Undo()
		{
			foreach (var resourceCell in cellResources)
			{
				resourceLayer.ClearResources(resourceCell.Cell);
				if (resourceCell.OldResourceTile.Type != null)
					resourceLayer.AddResource(resourceCell.OldResourceTile.Type, resourceCell.Cell, resourceCell.OldResourceTile.Density);
			}
		}

		public void Add(CellResource resourceCell)
		{
			resourceLayer.ClearResources(resourceCell.Cell);
			resourceLayer.AddResource(resourceCell.NewResourceType, resourceCell.Cell, resourceCell.NewResourceType.Info.MaxDensity);
			cellResources.Add(resourceCell);

			var cellText = cellResources.Count != 1 ? "cells" : "cell";
			Text = "Added {0} {1} of {2}".F(cellResources.Count, cellText, resourceType.Info.TerrainType);
		}
	}
}
