TimerColor = Player.GetPlayer("Spain").Color
TankPath = { waypoint12.Location, waypoint13.Location, waypoint1.Location, waypoint0.Location } 
AntPathN = { waypoint4.Location, waypoint18.Location, waypoint5.Location, waypoint15.Location} 
AntPathE = { waypoint20.Location, waypoint10.Location, waypoint2.Location }
AntPathW = { waypoint17.Location, waypoint1.Location }
AntPathS = { waypoint8.Location, waypoint9.Location, waypoint19.Location }
expireSeconds = 2
InsertionHelicopterType = "tran"
InsertionPath = { waypoint12.Location, waypoint0.Location }
baseDiscovered = false
AlliedBase = {Actor99, Actor100, Actor101, Actor102, Actor103, Actor104, Actor105, Actor106, Actor107, Actor129}
ValidForces = {"proc", "powr", "tent", "silo", "weap", "dome"}
WorldLoaded = function()
        allies = Player.GetPlayer("Spain")
        ussr = Player.GetPlayer("USSR")
        creeps = Player.GetPlayer("Creeps")
        InitObjectives()
end

ants = {"ant"}
fireAnts = {"fireant"}
AlliedForces = {"1tnk","2tnk","2tnk","mcv"}
ChooperTeam = {"e1r1","e1r1","e2","e2","e1r1"}
LightArmor = {"jeep","e1r1", "e1r1"}
AtEndGame = false
TimerTicks = DateTime.Minutes(30)

ticks = TimerTicks

Tick = function() 
  if SurviveObjective ~= nil then
	if ticks % DateTime.Seconds(1) == 0 then 
		if CheckBase() then 
			allies.MarkFailedObjective(SurviveObjective)
		end  
	end
    if ticks > 0 then
	  if DateTime.Minutes(29) == ticks then
		SendAnts("north",1)
	  elseif DateTime.Minutes(28) == ticks then 
		SendAnts("east",1)
		SendAnts("west",1)
      elseif DateTime.Minutes(27) == ticks then 
        SendAnts("west",1)
        SendAnts("north",2)
      elseif DateTime.Minutes(26) == ticks then
        SendAnts("south",2)
		SendAnts("west",2)
        Trigger.AfterDelay(DateTime.Seconds(1), function() SendInsertionHelicopter() end)

      elseif DateTime.Minutes(25) == ticks then
        SendFireAnts("north",1)
		SendAnts("west",1)
      elseif DateTime.Minutes(23) == ticks then
		SendAnts("east",2)
		Trigger.AfterDelay(DateTime.Seconds(30),function() SendAnts("west",3) end)
      elseif DateTime.Minutes(20) == ticks then
        SendAnts("west",3)
		SendAnts("east",3)
		SendAnts("north",2)
        Media.PlaySpeechNotification(allies, "TwentyMinutesRemaining")
      elseif DateTime.Minutes(18) == ticks then
        SendAnts("west",2)
		SendAnts("south",2)
		Trigger.AfterDelay(DateTime.Minutes(2), function()
			SendAnts("west",1)
			SendAnts("south", 2)
		end)
      elseif DateTime.Minutes(15) == ticks then
			SendAnts("west",2) 
			SendFireAnts("south",2)
		Trigger.AfterDelay(DateTime.Minutes(1),function() SendAnts("east",2) end)
		Trigger.AfterDelay(DateTime.Minutes(2),function() 
			SendAnts("east",2) 
			SendAnts("north",2) 
			SendAnts("west",3) 
		end)
      elseif DateTime.Minutes(11) == ticks then
        SendFireAnts("west",1)
		SendAnts("north",1)
		SendAnts("east",1)
      elseif DateTime.Minutes(10) == ticks then
        Media.PlaySpeechNotification(allies, "TenMinutesRemaining")
		Trigger.AfterDelay(DateTime.Minutes(1), function()
			SendAnts("west",2)
			SendAnts("north",2)
			SendAnts("east",2)
		end)
		Trigger.AfterDelay(DateTime.Minutes(2), function()
			SendAnts("west",3)
		end)
      elseif DateTime.Minutes(7) == ticks then
        SendAnts("south",2)
		SendAnts("west",3)
		SendFireAnts("north",1)
      elseif DateTime.Minutes(5) == ticks then
        Media.PlaySpeechNotification(allies, "WarningFiveMinutesRemaining")
        SendAnts("south",5)
		SendFireAnts("west",1)
      elseif DateTime.Minutes(4) == ticks then
        Media.PlaySpeechNotification(allies, "WarningFourMinutesRemaining")
		SendAnts("west",1)
		SendAnts("north",3)
      elseif DateTime.Minutes(3) == ticks then
        Media.PlaySpeechNotification(allies, "WarningThreeMinutesRemaining")
        SendAnts("north",2)
		SendFireAnts("east",2)
        SendAnts("south",4)
        SendAnts("west",4)
      elseif DateTime.Minutes(2) == ticks then
        Media.PlaySpeechNotification(allies, "WarningTwoMinutesRemaining")
      elseif DateTime.Minutes(1) == ticks then
        Media.PlaySpeechNotification(allies, "WarningOneMinuteRemaining")
		SendAnts("east",2)
        SendAnts("west",3)
        SendAnts("north",3)
        SendAnts("south",3)
		SendFireAnts("east", 2)
      elseif DateTime.Seconds(5) == ticks then
        Media.PlaySpeechNotification(allies, "AlliedForcesApproaching")
      end

      ticks = ticks - 1;
      UserInterface.SetMissionText("Reinforcements arrive in " .. Utils.FormatTime(ticks), TimerColor)
    else
		if not AtEndGame then
			Media.PlaySpeechNotification(allies, "SecondObjectiveMet")
			FinishTimer()
			AtEndGame = true
			Camera.Position = waypoint13.CenterPosition
			SendTanks()
			Trigger.AfterDelay(DateTime.Seconds(2), function() TimerExpired() end)
		end
		ticks = ticks - 1
    end
  end
end

SendAnts = function(type, amt)

  local index = 0
  local path = AntPathN

  if type == "east" then
	path = AntPathE
  elseif type == "west" then
	path = AntPathW
  elseif type == "south" then
	path = AntPathS
  else
	path = AntPathN
  end
  
  while index < amt do
	Reinforcements.Reinforce(ussr,ants,path,DateTime.Seconds(2))
	index = index + 1
  end

  
  Trigger.AfterDelay(DateTime.Seconds(4), function()
      for i,actor in pairs(ussr.GetActorsByType("ant")) do
        actor.AttackMove(CPos.New(65,65))
        actor.Hunt()
      end
  end)
end

SendFireAnts = function(type, amt)
	
  local index = 0
  local path = AntPathN

  if type == "east" then
	path = AntPathE
  elseif type == "west" then
	path = AntPathW
  elseif type == "south" then
	path = AntPathS
  else
	path = AntPathN
  end
  
  while index < amt do
	Reinforcements.Reinforce(ussr,fireAnts,path,DateTime.Seconds(2))
	index = index + 1
  end

  
  Trigger.AfterDelay(DateTime.Seconds(4), function()
      for i,actor in pairs(ussr.GetActorsByType("fireant")) do
        actor.AttackMove(CPos.New(65,65))
        actor.Hunt()
      end
  end)
end

SendTanks = function() 
  Media.PlaySpeechNotification(allies, "ReinforcementsArrived")
  Reinforcements.Reinforce(allies, AlliedForces, TankPath, DateTime.Seconds(1))
end

SendInsertionHelicopter = function()
        Media.PlaySpeechNotification(allies, "AlliedReinforcementsSouth")
        Reinforcements.ReinforceWithTransport(allies, InsertionHelicopterType, ChooperTeam, InsertionPath, { waypoint4.Location })
end

FinishTimer = function()
	for i = 0, 9, 1 do
		local c = TimerColor
		if i % 2 == 0 then
			c = HSLColor.White
		end

		Trigger.AfterDelay(DateTime.Seconds(i), function() UserInterface.SetMissionText("Allied forces have arrived!", c) end)
	end
	Trigger.AfterDelay(DateTime.Seconds(10), function() UserInterface.SetMissionText("") end)
end

TimerExpired = function()
    if not (CheckBase()) then
        allies.MarkCompletedObjective(SurviveObjective)
    else 
        allies.MarkFailedObjective(SurviveObjective)
    end
    expireSeconds = 0
end

DiscoveredAlliedBase = function(actor, discoverer)
  if (not baseDiscovered and discoverer.Owner == allies) then
    baseDiscovered = true  
    Media.PlaySpeechNotification(allies,"ObjectiveReached")
    Utils.Do(AlliedBase, function(building)
      building.Owner = allies
    end)
    UserInterface.SetMissionText("") 
    Media.DisplayMessage("Good job Commander, evaluating the situation now...","HQ", allies.Color) 
    Media.PlaySoundNotification(allies,"ChatLine")
      
    Trigger.AfterDelay(DateTime.Seconds(4), function()  
      Media.DisplayMessage("The damage seems considerable, support is en route to your position.","HQ", allies.Color) 
      SurviveObjective = allies.AddPrimaryObjective("Hold out until armored reinforcements arrive")
      Media.PlaySpeechNotification(allies, "MissionTimerInitialised")
      Trigger.AfterDelay(DateTime.Seconds(1), function() allies.MarkCompletedObjective(DiscoverObjective) end)
    end)
    
    creeps.GetActorsByType("harv")[1].FindResources()
    creeps.GetActorsByType("harv")[1].Owner = allies
  else
    return
  end
end

CheckBase = function()
	local validBuildings = 0
	Utils.Do(ValidForces, function(actorName)
		local count = #allies.GetActorsByType(actorName)
		validBuildings = validBuildings + count
	end)
	return validBuildings == 0 
end


InitObjectives = function() 
	Trigger.OnObjectiveAdded(allies, function(p, id)
		Media.DisplayMessage(p.GetObjectiveDescription(id), "New " .. string.lower(p.GetObjectiveType(id)) .. " objective")
	end)
  DiscoverObjective = allies.AddPrimaryObjective("Scout area and link up with Allied Outpost")

  Utils.Do(AlliedBase, function(actor)
    Trigger.OnEnteredProximityTrigger(actor.CenterPosition, WDist.FromCells(8), function(discoverer, id)
      DiscoveredAlliedBase(actor, discoverer)
    end)
  end)
  Trigger.AfterDelay(DateTime.Seconds(1), function()
    creeps.GetActorsByType("harv")[1].Stop()
  end)
	Trigger.OnObjectiveCompleted(allies, function(p, id)
    Media.DisplayMessage(p.GetObjectiveDescription(id), "Objective completed")
	end)
	Trigger.OnObjectiveFailed(allies, function(p, id)
		Media.DisplayMessage(p.GetObjectiveDescription(id), "Objective failed")
	end)

	Trigger.OnPlayerLost(allies, function()
		Media.PlaySpeechNotification(allies, "MissionFailed")
	end)
	Trigger.OnPlayerWon(allies, function()
		Trigger.AfterDelay(DateTime.Seconds(1), function() Media.PlaySpeechNotification(allies, "MissionAccomplished")  end)
	end)
  
  Camera.Position = Actor143.CenterPosition
end

