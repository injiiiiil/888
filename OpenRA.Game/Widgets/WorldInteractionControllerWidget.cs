#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Effects;
using OpenRA.Graphics;
using OpenRA.Orders;
using OpenRA.Traits;

namespace OpenRA.Widgets
{
	public class WorldInteractionControllerWidget : Widget
	{
		protected readonly World World;
		readonly WorldRenderer worldRenderer;
		int2 lastMousePosition, dragOrigin;
		bool isDragging = false;

		[ObjectCreator.UseCtor]
		public WorldInteractionControllerWidget(World world, WorldRenderer worldRenderer)
		{
			this.World = world;
			this.worldRenderer = worldRenderer;
		}

		public override void Draw()
		{
			if (isDragging)
			{
				Game.Renderer.WorldLineRenderer.DrawRect(dragOrigin.ToFloat2(), lastMousePosition.ToFloat2(), Color.White);
				foreach (var u in WorldUtils.SelectActorsInBoxWithDeadzone(World, dragOrigin, lastMousePosition))
					worldRenderer.DrawRollover(u);
			}
			else
			{
				// Render actors under the mouse pointer
				foreach (var u in WorldUtils.SelectActorsInBoxWithDeadzone(World, lastMousePosition, lastMousePosition))
					worldRenderer.DrawRollover(u);
			}
		}

		public override string GetCursor(int2 screenPos)
		{
			return Sync.CheckSyncUnchanged(World, () =>
			{
				// Always show an arrow while selecting
				if (isDragging)
					return null;

				var cell = worldRenderer.Viewport.ViewToWorld(screenPos);

				var mi = new MouseInput
				{
					Location = screenPos,
					Button = Game.Settings.Game.MouseButtonPreference.Action,
					Modifiers = Game.GetModifierKeys()
				};

				return World.OrderGenerator.GetCursor(World, cell, mi);
			});
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			var xy = worldRenderer.Viewport.ViewToWorldPx(mi.Location);

			var useClassicMouseStyle = Game.Settings.Game.UseClassicMouseStyle;

			var multiClick = mi.MultiTapCount >= 2;

			if (mi.Button == MouseButton.Left && mi.Event == MouseInputEvent.Down)
			{
				if (!TakeMouseFocus(mi))
					return false;

				dragOrigin = xy;
				isDragging = true;

				// Place buildings, use support powers, and other non-unit things
				if (!(World.OrderGenerator is UnitOrderGenerator))
				{
					ApplyOrders(World, mi);
					isDragging = false;
					YieldMouseFocus(mi);
					lastMousePosition = xy;
					return true;
				}
			}

			if (mi.Button == MouseButton.Left && mi.Event == MouseInputEvent.Up)
			{
				if (World.OrderGenerator is UnitOrderGenerator)
				{
					if (useClassicMouseStyle && HasMouseFocus)
					{
						if (!isDragging && World.Selection.Actors.Any() && !multiClick)
						{
							if (!(World.ScreenMap.ActorsAt(xy).Any(x => x.Info.HasTraitInfo<SelectableInfo>() &&
								(x.Owner.IsAlliedWith(World.RenderPlayer) || !World.FogObscures(x))) && !mi.Modifiers.HasModifier(Modifiers.Ctrl) &&
								!mi.Modifiers.HasModifier(Modifiers.Alt) && UnitOrderGenerator.InputOverridesSelection(World, xy, mi)))
							{
								// Order units instead of selecting
								ApplyOrders(World, mi);
								isDragging = false;
								YieldMouseFocus(mi);
								lastMousePosition = xy;
								return true;
							}
						}
					}

					if (multiClick)
					{
						var unit = World.ScreenMap.ActorsAt(xy)
							.WithHighestSelectionPriority();

						if (unit != null && unit.Owner == (World.RenderPlayer ?? World.LocalPlayer))
						{
							var s = unit.TraitOrDefault<Selectable>();
							if (s != null)
							{
								// Select actors on the screen that have the same selection class as the actor under the mouse cursor
								var newSelection = WorldUtils.SelectActorsOnScreen(World, worldRenderer.Viewport, new HashSet<string> { s.Class }, unit.Owner);

								World.Selection.Combine(World, newSelection, true, false);
							}
						}
					}
					else if (isDragging)
					{
						// Select actors in the dragbox
						var newSelection = WorldUtils.SelectActorsInBoxWithDeadzone(World, dragOrigin, xy);
						World.Selection.Combine(World, newSelection, mi.Modifiers.HasModifier(Modifiers.Shift), dragOrigin == xy);
					}
				}

				isDragging = false;
				YieldMouseFocus(mi);
			}

			if (mi.Button == MouseButton.Right && mi.Event == MouseInputEvent.Up)
			{
				// Don't do anything while selecting
				if (!isDragging)
				{
					if (useClassicMouseStyle)
						World.Selection.Clear();

					ApplyOrders(World, mi);
				}
			}

			lastMousePosition = xy;

			return true;
		}

		void ApplyOrders(World world, MouseInput mi)
		{
			if (world.OrderGenerator == null)
				return;

			var cell = worldRenderer.Viewport.ViewToWorld(mi.Location);
			var orders = world.OrderGenerator.Order(world, cell, mi).ToArray();
			world.PlayVoiceForOrders(orders);

			var flashed = false;
			foreach (var order in orders)
			{
				var o = order;
				if (o == null)
					continue;

				if (!flashed && !o.SuppressVisualFeedback)
				{
					if (o.TargetActor != null)
					{
						world.AddFrameEndTask(w => w.Add(new FlashTarget(o.TargetActor)));
						flashed = true;
					}
					else if (o.TargetLocation != CPos.Zero)
					{
						var pos = world.Map.CenterOfCell(cell);
						world.AddFrameEndTask(w => w.Add(new SpriteEffect(pos, world, "moveflsh", "moveflash")));
						flashed = true;
					}
				}

				world.IssueOrder(o);
			}
		}

		public override bool HandleKeyPress(KeyInput e)
		{
			if (e.Event == KeyInputEvent.Down)
			{
				var key = Hotkey.FromKeyInput(e);
				if (key == Game.Settings.Keys.PauseKey) return Pause();
				else if (key == Game.Settings.Keys.SelectAllUnitsKey) return SelectAllUnits();
				else if (key == Game.Settings.Keys.SelectUnitsByTypeKey) return SelectUnitsByType();
				else if (key == Game.Settings.Keys.ToggleStatusBarsKey) return ToggleStatusBars();
				else if (key == Game.Settings.Keys.TogglePixelDoubleKey) return TogglePixelDouble();
			}

			return false;
		}

		bool Pause()
		{
			var isSpectator = World.LocalPlayer == null;

			if (!isSpectator)
				World.SetPauseState(!World.Paused);

			return true;
		}

		bool SelectAllUnits()
		{
			if (World.IsGameOver)
				return false;

			var player = World.RenderPlayer ?? World.LocalPlayer;

			// Select actors on the screen which belong to the current player
			var ownUnitsOnScreen = WorldUtils.SelectActorsOnScreen(World, worldRenderer.Viewport, null, player).SubsetWithHighestSelectionPriority().ToList();

			// Check if selecting actors on the screen has selected new units
			if (ownUnitsOnScreen.Count > World.Selection.Actors.Count())
				Game.Debug("Selected across screen");
			else
			{
				// Select actors in the world that have highest selection priority
				ownUnitsOnScreen = WorldUtils.SelectActorsInWorld(World, null, player).SubsetWithHighestSelectionPriority().ToList();
				Game.Debug("Selected across map");
			}

			World.Selection.Combine(World, ownUnitsOnScreen, false, false);

			return true;
		}

		bool SelectUnitsByType()
		{
			if (World.IsGameOver || !World.Selection.Actors.Any())
				return false;

			var player = World.RenderPlayer ?? World.LocalPlayer;

			// Get all the selected actors' selection classes
			var selectedClasses = World.Selection.Actors
				.Where(x => !x.IsDead && x.Owner == player)
				.Select(a => a.Trait<Selectable>().Class)
				.ToHashSet();

			// Select actors on the screen that have the same selection class as one of the already selected actors
			var newSelection = WorldUtils.SelectActorsOnScreen(World, worldRenderer.Viewport, selectedClasses, player).ToList();

			// Check if selecting actors on the screen has selected new units
			if (newSelection.Count > World.Selection.Actors.Count())
				Game.Debug("Selected across screen");
			else
			{
				// Select actors in the world that have the same selection class as one of the already selected actors
				newSelection = WorldUtils.SelectActorsInWorld(World, selectedClasses, player).ToList();
				Game.Debug("Selected across map");
			}

			World.Selection.Combine(World, newSelection, true, false);

			return true;
		}

		bool ToggleStatusBars()
		{
			Game.Settings.Game.AlwaysShowStatusBars ^= true;
			return true;
		}

		bool TogglePixelDouble()
		{
			Game.Settings.Graphics.PixelDouble ^= true;
			worldRenderer.Viewport.Zoom = Game.Settings.Graphics.PixelDouble ? 2 : 1;
			return true;
		}
	}
}
