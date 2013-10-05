#region Copyright & License Information
/*
 * Copyright 2007-2012 The OpenRA Developers (see AUTHORS)
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
using OpenRA.Mods.RA.Buildings;
using OpenRA.Network;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA.Widgets.Logic
{
	public class ObserverStatsLogic
	{
		ContainerWidget basicStatsHeaders;
		ContainerWidget economyStatsHeaders;
		ContainerWidget productionStatsHeaders;
		ContainerWidget combatStatsHeaders;
		ContainerWidget earnedThisMinuteGraphHeaders;
		ScrollPanelWidget playerStatsPanel;
		ScrollItemWidget basicPlayerTemplate;
		ScrollItemWidget economyPlayerTemplate;
		ScrollItemWidget productionPlayerTemplate;
		ScrollItemWidget combatPlayerTemplate;
		ContainerWidget earnedThisMinuteGraphTemplate;
		ScrollItemWidget teamTemplate;
		DropDownButtonWidget statsDropDown;
		IEnumerable<Player> players;
		World world;
		WorldRenderer worldRenderer;

		[ObjectCreator.UseCtor]
		public ObserverStatsLogic(World world, WorldRenderer worldRenderer, Widget widget)
		{
			this.world = world;
			this.worldRenderer = worldRenderer;
			players = world.Players.Where(p => !p.NonCombatant);

			basicStatsHeaders = widget.Get<ContainerWidget>("BASIC_STATS_HEADERS");
			economyStatsHeaders = widget.Get<ContainerWidget>("ECONOMY_STATS_HEADERS");
			productionStatsHeaders = widget.Get<ContainerWidget>("PRODUCTION_STATS_HEADERS");
			combatStatsHeaders = widget.Get<ContainerWidget>("COMBAT_STATS_HEADERS");
			earnedThisMinuteGraphHeaders = widget.Get<ContainerWidget>("EARNED_THIS_MIN_GRAPH_HEADERS");

			playerStatsPanel = widget.Get<ScrollPanelWidget>("PLAYER_STATS_PANEL");
			playerStatsPanel.Layout = new GridLayout(playerStatsPanel);

			basicPlayerTemplate = playerStatsPanel.Get<ScrollItemWidget>("BASIC_PLAYER_TEMPLATE");
			economyPlayerTemplate = playerStatsPanel.Get<ScrollItemWidget>("ECONOMY_PLAYER_TEMPLATE");
			productionPlayerTemplate = playerStatsPanel.Get<ScrollItemWidget>("PRODUCTION_PLAYER_TEMPLATE");
			combatPlayerTemplate = playerStatsPanel.Get<ScrollItemWidget>("COMBAT_PLAYER_TEMPLATE");
			earnedThisMinuteGraphTemplate = playerStatsPanel.Get<ContainerWidget>("EARNED_THIS_MIN_GRAPH_TEMPLATE");

			teamTemplate = playerStatsPanel.Get<ScrollItemWidget>("TEAM_TEMPLATE");

			statsDropDown = widget.Get<DropDownButtonWidget>("STATS_DROPDOWN");
			statsDropDown.GetText = () => "Basic";
			statsDropDown.OnMouseDown = _ =>
			{
				var options = new List<StatsDropDownOption>
				{
					new StatsDropDownOption 
					{
						Title = "Basic",
						IsSelected = () => basicStatsHeaders.Visible,
						OnClick = () =>
						{
							ClearStats();
							statsDropDown.GetText = () => "Basic";
							DisplayStats(BasicStats);
						}
					},
					new StatsDropDownOption 
					{
						Title = "Economy",
						IsSelected = () => economyStatsHeaders.Visible,
						OnClick = () => 
						{
							ClearStats();
							statsDropDown.GetText = () => "Economy";
							DisplayStats(EconomyStats);
						}
					},
					new StatsDropDownOption 
					{
						Title = "Production",
						IsSelected = () => productionStatsHeaders.Visible,
						OnClick = () => 
						{
							ClearStats();
							statsDropDown.GetText = () => "Production";
							DisplayStats(ProductionStats);
						}
					},
					new StatsDropDownOption 
					{
						Title = "Combat",
						IsSelected = () => combatStatsHeaders.Visible,
						OnClick = () => 
						{
							ClearStats();
							statsDropDown.GetText = () => "Combat";
							DisplayStats(CombatStats);
						}
					},
					new StatsDropDownOption
					{
						Title = "Earnings (graph)",
						IsSelected = () => earnedThisMinuteGraphHeaders.Visible,
						OnClick = () => 
						{
							ClearStats();
							statsDropDown.GetText = () => "Earnings (graph)";
							EarnedThisMinuteGraph();
						}
					}
				};
				Func<StatsDropDownOption, ScrollItemWidget, ScrollItemWidget> setupItem = (option, template) =>
				{
					var item = ScrollItemWidget.Setup(template, option.IsSelected, option.OnClick);
					item.Get<LabelWidget>("LABEL").GetText = () => option.Title;
					return item;
				};
				statsDropDown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 150, options, setupItem);
			};

			ClearStats();
			DisplayStats(BasicStats);
		}

		void ClearStats()
		{
			playerStatsPanel.Children.Clear();
			basicStatsHeaders.Visible = false;
			economyStatsHeaders.Visible = false;
			productionStatsHeaders.Visible = false;
			combatStatsHeaders.Visible = false;
			earnedThisMinuteGraphHeaders.Visible = false;
		}

		void EarnedThisMinuteGraph()
		{
			earnedThisMinuteGraphHeaders.Visible = true;
			var template = earnedThisMinuteGraphTemplate.Clone();

			var graph = template.Get<LineGraphWidget>("EARNED_THIS_MIN_GRAPH");
			graph.GetSeries = () =>
				players.Select(p => new LineGraphSeries(
					p.PlayerName,
					p.Color.RGB,
					(p.PlayerActor.TraitOrDefault<PlayerStatistics>() ?? new PlayerStatistics(p.PlayerActor)).EarnedSamples.Select(s => (float)s)
					));

			playerStatsPanel.AddChild(template);
		}

		void DisplayStats(Func<Player, ScrollItemWidget> createItem)
		{
			var teams = players.GroupBy(p => (world.LobbyInfo.ClientWithIndex(p.ClientIndex) ?? new Session.Client()).Team).OrderBy(g => g.Key);
			foreach (var t in teams)
			{
				var team = t;
				var tt = ScrollItemWidget.Setup(teamTemplate, () => false, () => { });
				tt.IgnoreMouseOver = true;
				tt.Get<LabelWidget>("TEAM").GetText = () => team.Key == 0 ? "No Team" : "Team " + team.Key;
				playerStatsPanel.AddChild(tt);
				foreach (var p in team)
				{
					var player = p;
					playerStatsPanel.AddChild(createItem(player));
				}
			}
		}

		ScrollItemWidget CombatStats(Player player)
		{
			combatStatsHeaders.Visible = true;
			var template = SetupPlayerScrollItemWidget(combatPlayerTemplate, player);

			LobbyUtils.AddPlayerFlagAndName(template, player);

			var stats = player.PlayerActor.TraitOrDefault<PlayerStatistics>();
			if (stats == null) return template;
			template.Get<LabelWidget>("CONTROL").GetText = () => MapControl(stats.MapControl);
			template.Get<LabelWidget>("KILLS_COST").GetText = () => "$" + stats.KillsCost;
			template.Get<LabelWidget>("DEATHS_COST").GetText = () => "$" + stats.DeathsCost;
			template.Get<LabelWidget>("UNITS_KILLED").GetText = () => stats.UnitsKilled.ToString();
			template.Get<LabelWidget>("UNITS_DEAD").GetText = () => stats.UnitsDead.ToString();
			template.Get<LabelWidget>("BUILDINGS_KILLED").GetText = () => stats.BuildingsKilled.ToString();
			template.Get<LabelWidget>("BUILDINGS_DEAD").GetText = () => stats.BuildingsDead.ToString();

			return template;
		}

		ScrollItemWidget ProductionStats(Player player)
		{
			productionStatsHeaders.Visible = true;
			var template = SetupPlayerScrollItemWidget(productionPlayerTemplate, player);

			LobbyUtils.AddPlayerFlagAndName(template, player);

			template.Get<ObserverProductionIconsWidget>("PRODUCTION_ICONS").GetPlayer = () => player;
			template.Get<ObserverSupportPowerIconsWidget>("SUPPORT_POWER_ICONS").GetPlayer = () => player;

			return template;
		}

		ScrollItemWidget EconomyStats(Player player)
		{
			economyStatsHeaders.Visible = true;
			var template = SetupPlayerScrollItemWidget(economyPlayerTemplate, player);

			LobbyUtils.AddPlayerFlagAndName(template, player);

			var res = player.PlayerActor.Trait<PlayerResources>();
			var stats = player.PlayerActor.TraitOrDefault<PlayerStatistics>();
			if (stats == null) return template;

			template.Get<LabelWidget>("CASH").GetText = () => "$" + (res.DisplayCash + res.DisplayOre);
			template.Get<LabelWidget>("EARNED_MIN").GetText = () => AverageEarnedPerMinute(res.Earned);
			template.Get<LabelWidget>("EARNED_THIS_MIN").GetText = () => "$" + stats.EarnedThisMinute;
			template.Get<LabelWidget>("EARNED").GetText = () => "$" + res.Earned;
			template.Get<LabelWidget>("SPENT").GetText = () => "$" + res.Spent;

			var assets = template.Get<LabelWidget>("ASSETS");
			assets.GetText = () => "$" + world.Actors
				.Where(a => a.Owner == player && !a.IsDead() && a.Info.Traits.WithInterface<ValuedInfo>().Any())
				.Sum(a => a.Info.Traits.WithInterface<ValuedInfo>().First().Cost);

			var harvesters = template.Get<LabelWidget>("HARVESTERS");
			harvesters.GetText = () => world.Actors.Count(a => a.Owner == player && !a.IsDead() && a.HasTrait<Harvester>()).ToString();

			return template;
		}

		ScrollItemWidget BasicStats(Player player)
		{
			basicStatsHeaders.Visible = true;
			var template = SetupPlayerScrollItemWidget(basicPlayerTemplate, player);

			LobbyUtils.AddPlayerFlagAndName(template, player);

			var res = player.PlayerActor.Trait<PlayerResources>();
			template.Get<LabelWidget>("CASH").GetText = () => "$" + (res.DisplayCash + res.DisplayOre);
			template.Get<LabelWidget>("EARNED_MIN").GetText = () => AverageEarnedPerMinute(res.Earned);

			var powerRes = player.PlayerActor.Trait<PowerManager>();
			var power = template.Get<LabelWidget>("POWER");
			power.GetText = () => powerRes.PowerDrained + "/" + powerRes.PowerProvided;
			power.GetColor = () => GetPowerColor(powerRes.PowerState);

			var stats = player.PlayerActor.TraitOrDefault<PlayerStatistics>();
			if (stats == null) return template;
			template.Get<LabelWidget>("KILLS").GetText = () => (stats.UnitsKilled + stats.BuildingsKilled).ToString();
			template.Get<LabelWidget>("DEATHS").GetText = () => (stats.UnitsDead + stats.UnitsDead).ToString();
			template.Get<LabelWidget>("ACTIONS_MIN").GetText = () => AverageOrdersPerMinute(stats.OrderCount);

			return template;
		}

		ScrollItemWidget SetupPlayerScrollItemWidget(ScrollItemWidget template, Player player)
		{
			return ScrollItemWidget.Setup(template, () => false, () =>
			{
				var playerBase = world.Actors.FirstOrDefault(a => !a.IsDead() && a.HasTrait<BaseBuilding>() && a.Owner == player);
				if (playerBase != null)
					worldRenderer.Viewport.Center(playerBase.CenterPosition);
			});
		}

		string MapControl(double control)
		{
			return (control * 100).ToString("F1") + "%";
		}

		string AverageOrdersPerMinute(double orders)
		{
			return (world.FrameNumber == 0 ? 0 : orders / (world.FrameNumber / 1500.0)).ToString("F1");
		}

		string AverageEarnedPerMinute(double earned)
		{
			return "$" + (world.FrameNumber == 0 ? 0 : earned / (world.FrameNumber / 1500.0)).ToString("F2");
		}

		static Color GetPowerColor(PowerState state)
		{
			if (state == PowerState.Critical) return Color.Red;
			if (state == PowerState.Low) return Color.Orange;
			return Color.LimeGreen;
		}

		class StatsDropDownOption
		{
			public string Title;
			public Func<bool> IsSelected;
			public Action OnClick;
		}
	}
}
