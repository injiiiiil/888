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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class IntroductionPromptLogic : ChromeLogic
	{
		// Increment the version number when adding new stats
		const int IntroductionVersion = 1;

		[TranslationReference]
		const string Classic = "options-control-scheme.classic";

		[TranslationReference]
		const string Modern = "options-control-scheme.modern";

		public class IntroductionPromptLogicDynamicWidgets : DynamicWidgets
		{
			public override ISet<string> WindowWidgetIds { get; } = EmptySet;
			public override IReadOnlyDictionary<string, string> ParentWidgetIdForChildWidgetId { get; } = EmptyDictionary;
			public override IReadOnlyDictionary<string, IReadOnlyCollection<string>> ParentDropdownWidgetIdsFromPanelWidgetId { get; } =
				new Dictionary<string, IReadOnlyCollection<string>>
				{
					{ "COLOR_CHOOSER", new[] { "PLAYERCOLOR" } },
					{ "LABEL_DROPDOWN_TEMPLATE", new[] { "MOUSE_CONTROL_DROPDOWN", "BATTLEFIELD_CAMERA_DROPDOWN", "UI_SCALE_DROPDOWN" } },
				};
		}

		readonly IntroductionPromptLogicDynamicWidgets dynamicWidgets = new();

		readonly string classic;
		readonly string modern;

		public static bool ShouldShowPrompt()
		{
			return Game.Settings.Game.IntroductionPromptVersion < IntroductionVersion;
		}

		[ObjectCreator.UseCtor]
		public IntroductionPromptLogic(Widget widget, ModData modData, WorldRenderer worldRenderer, Action onComplete)
		{
			var ps = Game.Settings.Player;
			var ds = Game.Settings.Graphics;
			var gs = Game.Settings.Game;

			classic = TranslationProvider.GetString(Classic);
			modern = TranslationProvider.GetString(Modern);

			var escPressed = false;
			var nameTextfield = widget.Get<TextFieldWidget>("PLAYERNAME");
			nameTextfield.IsDisabled = () => worldRenderer.World.Type != WorldType.Shellmap;
			nameTextfield.Text = Settings.SanitizedPlayerName(ps.Name);

			var itchIntegration = modData.Manifest.Get<ItchIntegration>();
			itchIntegration.GetPlayerName(name => nameTextfield.Text = Settings.SanitizedPlayerName(name));

			nameTextfield.OnLoseFocus = () =>
			{
				if (escPressed)
				{
					escPressed = false;
					return;
				}

				nameTextfield.Text = nameTextfield.Text.Trim();
				if (nameTextfield.Text.Length == 0)
					nameTextfield.Text = Settings.SanitizedPlayerName(ps.Name);
				else
				{
					nameTextfield.Text = Settings.SanitizedPlayerName(nameTextfield.Text);
					ps.Name = nameTextfield.Text;
				}
			};

			nameTextfield.OnEnterKey = _ => { nameTextfield.YieldKeyboardFocus(); return true; };
			nameTextfield.OnEscKey = _ =>
			{
				nameTextfield.Text = Settings.SanitizedPlayerName(ps.Name);
				escPressed = true;
				nameTextfield.YieldKeyboardFocus();
				return true;
			};

			var mouseControlDescClassic = widget.Get("MOUSE_CONTROL_DESC_CLASSIC");
			mouseControlDescClassic.IsVisible = () => gs.UseClassicMouseStyle;

			var mouseControlDescModern = widget.Get("MOUSE_CONTROL_DESC_MODERN");
			mouseControlDescModern.IsVisible = () => !gs.UseClassicMouseStyle;

			var mouseControlDropdown = widget.Get<DropDownButtonWidget>("MOUSE_CONTROL_DROPDOWN");
			mouseControlDropdown.OnMouseDown = _ => InputSettingsLogic.ShowMouseControlDropdown(dynamicWidgets, mouseControlDropdown, gs);
			mouseControlDropdown.GetText = () => gs.UseClassicMouseStyle ? classic : modern;

			foreach (var container in new[] { mouseControlDescClassic, mouseControlDescModern })
			{
				var classicScrollRight = container.Get("DESC_SCROLL_RIGHT");
				classicScrollRight.IsVisible = () => gs.UseClassicMouseStyle ^ gs.UseAlternateScrollButton;

				var classicScrollMiddle = container.Get("DESC_SCROLL_MIDDLE");
				classicScrollMiddle.IsVisible = () => !gs.UseClassicMouseStyle ^ gs.UseAlternateScrollButton;

				var zoomDesc = container.Get("DESC_ZOOM");
				zoomDesc.IsVisible = () => gs.ZoomModifier == Modifiers.None;

				var zoomDescModifier = container.Get<LabelWidget>("DESC_ZOOM_MODIFIER");
				zoomDescModifier.IsVisible = () => gs.ZoomModifier != Modifiers.None;

				var zoomDescModifierTemplate = zoomDescModifier.GetText();
				var zoomDescModifierLabel = new CachedTransform<Modifiers, string>(
					mod => zoomDescModifierTemplate.Replace("MODIFIER", mod.ToString()));
				zoomDescModifier.GetText = () => zoomDescModifierLabel.Update(gs.ZoomModifier);

				var edgescrollDesc = container.Get<LabelWidget>("DESC_EDGESCROLL");
				edgescrollDesc.IsVisible = () => gs.ViewportEdgeScroll;
			}

			SettingsUtils.BindCheckboxPref(widget, "EDGESCROLL_CHECKBOX", gs, "ViewportEdgeScroll");

			var colorManager = modData.DefaultRules.Actors[SystemActors.World].TraitInfo<IColorPickerManagerInfo>();

			var colorDropdown = widget.Get<DropDownButtonWidget>("PLAYERCOLOR");
			colorDropdown.IsDisabled = () => worldRenderer.World.Type != WorldType.Shellmap;
			colorDropdown.OnMouseDown = _ => colorManager.ShowColorDropDown(
				dynamicWidgets, colorDropdown, ps.Color, null, worldRenderer, color =>
			{
				ps.Color = color;
				Game.Settings.Save();
			});
			colorDropdown.Get<ColorBlockWidget>("COLORBLOCK").GetColor = () => ps.Color;

			var viewportSizes = modData.Manifest.Get<WorldViewportSizes>();
			var battlefieldCameraDropDown = widget.Get<DropDownButtonWidget>("BATTLEFIELD_CAMERA_DROPDOWN");
			var battlefieldCameraLabel = new CachedTransform<WorldViewport, string>(vs => DisplaySettingsLogic.GetViewportSizeName(modData, vs));
			battlefieldCameraDropDown.OnMouseDown = _ => DisplaySettingsLogic.ShowBattlefieldCameraDropdown(
				dynamicWidgets, modData, battlefieldCameraDropDown, viewportSizes, ds);
			battlefieldCameraDropDown.GetText = () => battlefieldCameraLabel.Update(ds.ViewportDistance);

			var uiScaleDropdown = widget.Get<DropDownButtonWidget>("UI_SCALE_DROPDOWN");
			var uiScaleLabel = new CachedTransform<float, string>(s => $"{(int)(100 * s)}%");
			uiScaleDropdown.OnMouseDown = _ => DisplaySettingsLogic.ShowUIScaleDropdown(dynamicWidgets, uiScaleDropdown, ds);
			uiScaleDropdown.GetText = () => uiScaleLabel.Update(ds.UIScale);

			var minResolution = viewportSizes.MinEffectiveResolution;
			var resolution = Game.Renderer.Resolution;
			var disableUIScale = worldRenderer.World.Type != WorldType.Shellmap ||
				resolution.Width * ds.UIScale < 1.25f * minResolution.Width ||
				resolution.Height * ds.UIScale < 1.25f * minResolution.Height;

			uiScaleDropdown.IsDisabled = () => disableUIScale;

			SettingsUtils.BindCheckboxPref(widget, "CURSORDOUBLE_CHECKBOX", ds, "CursorDouble");

			widget.Get<ButtonWidget>("CONTINUE_BUTTON").OnClick = () =>
			{
				Game.Settings.Game.IntroductionPromptVersion = IntroductionVersion;
				Game.Settings.Save();
				Ui.CloseWindow();
				onComplete();
			};

			SettingsUtils.AdjustSettingsScrollPanelLayout(widget.Get<ScrollPanelWidget>("SETTINGS_SCROLLPANEL"));
		}
	}
}
