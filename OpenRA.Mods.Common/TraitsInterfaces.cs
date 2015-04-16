#region Copyright & License Information
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
using OpenRA.Activities;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public interface ISpriteBody
	{
		void PlayCustomAnimation(Actor self, string newAnimation, Action after);
		void PlayCustomAnimationRepeating(Actor self, string name);
		void PlayCustomAnimationBackwards(Actor self, string name, Action after);
	}

	public interface INotifyResourceClaimLost
	{
		void OnNotifyResourceClaimLost(Actor self, ResourceClaim claim, Actor claimer);
	}

	public interface IPlaceBuildingDecoration
	{
		IEnumerable<IRenderable> Render(WorldRenderer wr, World w, ActorInfo ai, WPos centerPosition);
	}

	public interface INotifyAttack { void Attacking(Actor self, Target target, Armament a, Barrel barrel); }
	public interface INotifyChat { bool OnChat(string from, string message); }
	public interface INotifyParachuteLanded { void OnLanded(); }
	public interface IRenderActorPreviewInfo { IEnumerable<IActorPreview> RenderPreview(ActorPreviewInitializer init); }
	public interface ICruiseAltitudeInfo { WRange GetCruiseAltitude(); }

	public interface IUpgradable
	{
		IEnumerable<string> UpgradeTypes { get; }
		bool AcceptsUpgradeLevel(Actor self, string type, int level);
		void UpgradeLevelChanged(Actor self, string type, int oldLevel, int newLevel);
	}

	public interface INotifyHarvesterAction
	{
		void MovingToResources(Actor self, CPos targetCell, Activity next);
		void MovingToRefinery(Actor self, CPos targetCell, Activity next);
		void MovementCancelled(Actor self);
		void Harvested(Actor self, ResourceType resource);
	}

	public interface ITechTreePrerequisite
	{
		IEnumerable<string> ProvidesPrerequisites { get; }
	}

	public interface ITechTreeElement
	{
		void PrerequisitesAvailable(string key);
		void PrerequisitesUnavailable(string key);
		void PrerequisitesItemHidden(string key);
		void PrerequisitesItemVisible(string key);
	}

	public interface INotifyTransform { void BeforeTransform(Actor self); void OnTransform(Actor self); void AfterTransform(Actor toActor); }

	public interface IAcceptResources
	{
		void OnDock(Actor harv, DeliverResources dockOrder);
		void GiveResource(int amount);
		bool CanGiveResource(int amount);
		CVec DeliveryOffset { get; }
		bool AllowDocking { get; }
	}

	public interface IProvidesAssetBrowserPalettes
	{
		IEnumerable<string> PaletteNames { get; }
	}
}
