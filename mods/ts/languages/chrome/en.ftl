## assetbrowser.yaml
label-assetbrowser-model-scale = Scale:
label-voxel-selector-roll = Roll
label-voxel-selector-pitch = Pitch
label-voxel-selector-yaw = Yaw

## ingame-debug.yaml
checkbox-debug-panel-show-depth-preview = Show Depth Data

## ingame-observer.yaml
button-observer-widgets-options = Options (Esc)
button-replay-player-pause-tooltip = Pause
button-replay-player-play-tooltip = Play

button-replay-player-slow =
   .tooltip = Slow speed
   .label = 50%

button-replay-player-regular =
   .tooltip = Regular speed
   .label = 100%

button-replay-player-fast =
   .tooltip = Fast speed
   .label = 200%

button-replay-player-maximum =
   .tooltip = Maximum speed
   .label = MAX

label-basic-stats-player-header = Player
label-basic-stats-cash-header = Cash
label-basic-stats-power-header = Power
label-basic-stats-kills-header = Kills
label-basic-stats-deaths-header = Deaths
label-basic-stats-assets-destroyed-header = Destroyed
label-basic-stats-assets-lost-header = Lost
label-basic-stats-experience-header = Score
label-basic-stats-actions-min-header = APM
label-economy-stats-player-header = Player
label-economy-stats-cash-header = Cash
label-economy-stats-income-header = Income
label-economy-stats-assets-header = Assets
label-economy-stats-earned-header = Earned
label-economy-stats-spent-header = Spent
label-economy-stats-harvesters-header = Harvesters
label-production-stats-player-header = Player
label-production-stats-header = Production
label-support-powers-player-header = Player
label-support-powers-header = Support Powers
label-army-player-header = Player
label-army-header = Army
label-combat-stats-player-header = Player
label-combat-stats-assets-destroyed-header = Destroyed
label-combat-stats-assets-lost-header = Lost
label-combat-stats-units-killed-header = U. Killed
label-combat-stats-units-dead-header = U. Lost
label-combat-stats-buildings-killed-header = B. Killed
label-combat-stats-buildings-dead-header = B. Lost
label-combat-stats-army-value-header = Army Value
label-combat-stats-vision-header = Vision

## ingame-observer.yaml, ingame-player.yaml
label-mute-indicator = Audio Muted

## ingame-player.yaml
supportpowers-support-powers-palette =
   .ready = READY
   .hold = ON HOLD

button-command-bar-attack-move =
   .tooltip = Attack Move
   .tooltipdesc = Selected units move to the target location and attack any enemies they encounter en route

    Hold <(Ctrl)> while targeting to issue an Assault Move,
    that attacks any units or structures encountered en route

    Left-click the icon, then right-click on the target

button-command-bar-force-move =
   .tooltip = Force Move
   .tooltipdesc = Selected units move to the target location
     - Default activity for the target is suppressed
     - Vehicles attempt to crush enemies
     - Deployed units undeploy and move to the target location
     - Helicopters land at the target location

    Left-click the icon, then right-click on the target
    Hold <(Alt)> to temporarily activate the command

button-command-bar-force-attack =
   .tooltip = Force Attack
   .tooltipdesc = Selected units attack the targeted unit or location
     - The default activity of the target is suppressed
     - Allows targeting of friendly or allied forces
     - Long-range artillery units always target the
       location, ignoring units and buildings

    Left-click the icon, then right-click on the target
    Hold <(Ctrl)> to temporarily activate the command

button-command-bar-guard =
   .tooltip = Guard
   .tooltipdesc = Selected units follow the targeted unit

    Left-click the icon, then right-click on the target

button-command-bar-deploy =
   .tooltip = Deploy
   .tooltipdesc = Selected units perform their default deploy activity
     - MCVs deploy into Construction Yards
     - Construction Yards repack into MCVs
     - Transports unload their passengers
     - Tick Tanks, Artillery, Juggernauts,
       and Mobile Sensor arrays deploy
     - Aircraft return to their base

    Acts immediately on selected units

button-command-bar-scatter =
   .tooltip = Scatter
   .tooltipdesc = Selected units stop their current activity
    and scatter to a nearby location

    Acts immediately on selected units

button-command-bar-stop =
   .tooltip = Stop
   .tooltipdesc = Selected units stop their current activity
    Selected buildings reset their rally point

    Acts immediately on selected units

button-command-bar-queue-orders =
   .tooltip = Waypoint Mode
   .tooltipdesc = Use Waypoint Mode to issue a series of linked commands
    to selected units. Units execute their commands
    immediately upon receiving them

    Left-click the icon, then issue commands in the game world
    Hold <(Shift)> to temporarily activate the command while ordering units

button-stance-bar-attackanything =
   .tooltip = Attack Anything Stance
   .tooltipdesc = Set the selected units to Attack Anything stance:
     - Units attack enemy units and structures on sight
     - Units pursue attackers across the battlefield

button-stance-bar-defend =
   .tooltip = Defend Stance
   .tooltipdesc = Set the selected units to Defense stance:
     - Units attack enemy units on sight
     - Units do not move or pursue enemies

button-stance-bar-returnfire =
   .tooltip = Return Fire Stance
   .tooltipdesc = Set the selected units to Return Fire stance:
     - Units retaliate against enemies that attack them
     - Units do not move or pursue enemies

button-stance-bar-holdfire =
   .tooltip = Hold Fire Stance
   .tooltipdesc = Set the selected units to Hold Fire stance:
     - Units do not fire upon enemies
     - Units do not move or pursue enemies

button-top-buttons-repair-tooltip = Repair
button-top-buttons-sell-tooltip = Sell
button-top-buttons-beacon-tooltip = Place Beacon
button-top-buttons-power-tooltip = Power Down
button-top-buttons-options-tooltip = Options

productionpalette-sidebar-production-palette =
   .ready = READY
   .hold = ON HOLD

button-production-types-building-tooltip = Buildings
button-production-types-defense-tooltip = Support
button-production-types-infantry-tooltip = Infantry
button-production-types-vehicle-tooltip = Vehicles
button-production-types-aircraft-tooltip = Aircraft
button-production-types-scroll-up-tooltip = Scroll up
button-production-types-scroll-down-tooltip = Scroll down

## mainmenu-prerelease-notification.yaml
label-mainmenu-prerelease-notification-prompt-title = Tiberian Sun developer preview
label-mainmenu-prerelease-notification-prompt-text-a = This pre-alpha build of OpenRA's Tiberian Sun mod is made available
label-mainmenu-prerelease-notification-prompt-text-b = for the community to follow development and serve as an example for modders.
label-mainmenu-prerelease-notification-prompt-text-c = Please note that many features are still missing or incomplete, performance has not been
label-mainmenu-prerelease-notification-prompt-text-d = optimized, and that game balance will not be addressed until a future beta.
button-mainmenu-prerelease-notification-continue = I Understand.
