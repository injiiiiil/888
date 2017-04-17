--[[
   Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

HarkonnenBase = { HConyard, HPower1, HPower2, HBarracks }

HarkonnenReinforcements = { }
HarkonnenReinforcements["easy"] =
{
	{ "light_inf", "trike" },
	{ "light_inf", "trike" },
	{ "light_inf", "light_inf", "light_inf", "trike", "trike" }
}

HarkonnenReinforcements["normal"] =
{
	{ "light_inf", "trike" },
	{ "light_inf", "trike" },
	{ "light_inf", "light_inf", "light_inf", "trike", "trike" },
	{ "light_inf", "light_inf" },
	{ "light_inf", "light_inf", "light_inf" },
	{ "light_inf", "trike" },
}

HarkonnenReinforcements["hard"] =
{
	{ "trike", "trike" },
	{ "light_inf", "trike" },
	{ "light_inf", "trike" },
	{ "light_inf", "light_inf", "light_inf", "trike", "trike" },
	{ "light_inf", "light_inf" },
	{ "trike", "trike" },
	{ "light_inf", "light_inf", "light_inf" },
	{ "light_inf", "trike" },
	{ "trike", "trike" }
}

HarkonnenAttackPaths =
{
	{ HarkonnenEntry1.Location, HarkonnenRally1.Location },
	{ HarkonnenEntry1.Location, HarkonnenRally3.Location },
	{ HarkonnenEntry2.Location, HarkonnenRally2.Location },
	{ HarkonnenEntry2.Location, HarkonnenRally4.Location }
}

HarkonnenAttackDelay = { }
HarkonnenAttackDelay["easy"] = DateTime.Minutes(5)
HarkonnenAttackDelay["normal"] = DateTime.Minutes(2) + DateTime.Seconds(40)
HarkonnenAttackDelay["hard"] = DateTime.Minutes(1) + DateTime.Seconds(20)

HarkonnenAttackWaves = { }
HarkonnenAttackWaves["easy"] = 3
HarkonnenAttackWaves["normal"] = 6
HarkonnenAttackWaves["hard"] = 9

wave = 0
SendHarkonnen = function()
	Trigger.AfterDelay(HarkonnenAttackDelay[Map.LobbyOption("difficulty")], function()
		wave = wave + 1
		if wave > HarkonnenAttackWaves[Map.LobbyOption("difficulty")] then
			return
		end

		local path = Utils.Random(HarkonnenAttackPaths)
		local units = Reinforcements.ReinforceWithTransport(harkonnen, "carryall.reinforce", HarkonnenReinforcements[Map.LobbyOption("difficulty")][wave], path, { path[1] })[2]
		Utils.Do(units, IdleHunt)

		SendHarkonnen()
	end)
end

IdleHunt = function(unit)
	Trigger.OnIdle(unit, unit.Hunt)
end

Tick = function()
	if player.HasNoRequiredUnits() then
		harkonnen.MarkCompletedObjective(KillAtreides)
	end

	if harkonnen.HasNoRequiredUnits() and not player.IsObjectiveCompleted(KillHarkonnen) then
		Media.DisplayMessage("The Harkonnen have been annihilated!", "Mentat")
		player.MarkCompletedObjective(KillHarkonnen)
	end
end

WorldLoaded = function()
	harkonnen = Player.GetPlayer("Harkonnen")
	player = Player.GetPlayer("Atreides")

	InitObjectives()

	Camera.Position = AConyard.CenterPosition

	Trigger.OnAllKilled(HarkonnenBase, function()
		Utils.Do(harkonnen.GetGroundAttackers(), IdleHunt)
	end)

	SendHarkonnen()
	Trigger.AfterDelay(0, ActivateAI)
end

InitObjectives = function()
	Trigger.OnObjectiveAdded(player, function(p, id)
		Media.DisplayMessage(p.GetObjectiveDescription(id), "New " .. string.lower(p.GetObjectiveType(id)) .. " objective")
	end)

	KillAtreides = harkonnen.AddPrimaryObjective("Kill all Atreides units.")
	KillHarkonnen = player.AddPrimaryObjective("Destroy all Harkonnen forces.")

	Trigger.OnObjectiveCompleted(player, function(p, id)
		Media.DisplayMessage(p.GetObjectiveDescription(id), "Objective completed")
	end)
	Trigger.OnObjectiveFailed(player, function(p, id)
		Media.DisplayMessage(p.GetObjectiveDescription(id), "Objective failed")
	end)

	Trigger.OnPlayerLost(player, function()
		Trigger.AfterDelay(DateTime.Seconds(1), function()
			Media.PlaySpeechNotification(player, "Lose")
		end)
	end)
	Trigger.OnPlayerWon(player, function()
		Trigger.AfterDelay(DateTime.Seconds(1), function()
			Media.PlaySpeechNotification(player, "Win")
		end)
	end)
end
