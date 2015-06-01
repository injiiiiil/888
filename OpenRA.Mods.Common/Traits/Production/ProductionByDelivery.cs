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
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Deliver produced units from outside the map.")]
	public class ProductionByDeliveryInfo : ProductionInfo
	{
		[Desc("Delivering actor name. The actor needs to have either of the `Helicopter` or `Plane` traits.")]
		[ActorReference]
		public readonly string DeliveryActor = "c17";

		[Desc("Minimal interval (in ticks) between spawning new delivery actors.")]
		public readonly int MinimumInterval = 75;

		[Desc("Can the delivering actor fly away if this actor is destroyed during drop-off?")]
		public readonly bool DeliveryActorLives = false;

		public readonly string ReadyAudio = "Reinforce";

		public override object Create(ActorInitializer init) { return new ProductionByDelivery(init, this); }
	}

	class ProductionByDelivery : Production, ITick, INotifyKilled, INotifySold
	{
		readonly Actor self;
		readonly CPos startPos;
		readonly CPos endPos;
		readonly ProductionByDeliveryInfo info;
		readonly AircraftInfo aircraft;
		readonly bool isPlane;

		int timeLeft;
		Actor landedActor;
		List<Pair<ActorInfo, string>> production = new List<Pair<ActorInfo, string>>();

		public ProductionByDelivery(ActorInitializer init, ProductionByDeliveryInfo info)
			: base(init, info)
		{
			self = init.Self;

			// Start a fixed distance away: the width of the map.
			// This makes the production timing independent of spawnpoint
			startPos = self.Location + new CVec(self.Owner.World.Map.Bounds.Width, 0);
			endPos = new CPos(self.Owner.World.Map.Bounds.Left - 5, self.Location.Y);
			this.info = info;

			aircraft = self.World.Map.Rules.Actors[info.DeliveryActor].Traits.Get<AircraftInfo>();
			isPlane = aircraft is PlaneInfo;
		}

		public void Tick(Actor self)
		{
			if (timeLeft-- > 0 || !production.Any())
				return;

			StartDelivery();
			timeLeft = info.MinimumInterval;
		}

		public override bool Produce(Actor self, IEnumerable<Pair<ActorInfo, string>> actorsToProduce)
		{
			production.AddRange(actorsToProduce);
			return true;
		}

		void StartDelivery()
		{
			var deliveringActorType = info.DeliveryActor;
			var owner = self.Owner;
			var actorsToProduce = new List<Pair<ActorInfo, string>>(production);
			production = new List<Pair<ActorInfo, string>>();

			// Check if there is a valid drop-off point before sending the transport
			var exit = GetAvailableExit(self, actorsToProduce.First().First);
			if (exit == null)
				return;

			foreach (var trait in self.TraitsImplementing<INotifyDelivery>())
				trait.IncomingDelivery(self);

			owner.World.AddFrameEndTask(w =>
			{
				var deliveringActor = w.CreateActor(deliveringActorType, new TypeDictionary
				{
					new CenterPositionInit(w.Map.CenterOfCell(startPos) + new WVec(WRange.Zero, WRange.Zero, aircraft.CruiseAltitude)),
					new OwnerInit(owner),
					new FacingInit(64)
				});

				if (isPlane)
					deliveringActor.QueueActivity(new Fly(deliveringActor, Target.FromCell(w, self.Location + new CVec(9, 0))));
				else
					deliveringActor.QueueActivity(new HeliFly(deliveringActor, Target.FromCell(w, self.Location + new CVec(9, 0))));

				deliveringActor.QueueActivity(new CallFunc(() => TryToLand(deliveringActor, actorsToProduce)));
			});
		}

		void TryToLand(Actor deliveringActor, List<Pair<ActorInfo, string>> actorsToProduce)
		{
			// Check if there is a valid drop-off point before beginning to descend
			var exit = GetAvailableExit(self, actorsToProduce.First().First);

			// Abort the landing and refund the player
			if (exit == null || self.IsDead || !self.IsInWorld)
			{
				var value = actorsToProduce.Sum(actor => actor.First.Traits.Get<ValuedInfo>().Cost);
				self.Owner.PlayerActor.Trait<PlayerResources>().GiveCash(value);

				if (isPlane)
					deliveringActor.QueueActivity(new Fly(deliveringActor, Target.FromCell(self.World, endPos)));
				else
					deliveringActor.QueueActivity(new HeliFly(deliveringActor, Target.FromCell(self.World, endPos)));

				deliveringActor.QueueActivity(new RemoveSelf());

				return;
			}

			if (isPlane)
				deliveringActor.QueueActivity(new Land(deliveringActor, Target.FromActor(self)));
			else
			{
				deliveringActor.QueueActivity(new HeliFly(deliveringActor, Target.FromActor(self)));
				deliveringActor.QueueActivity(new HeliLand(deliveringActor, false));
			}

			deliveringActor.QueueActivity(new CallFunc(() => MakeDelivery(actorsToProduce.ToList(), deliveringActor, deliveringActor.World)));
		}

		/// <summary>
		/// Drop off any actors that the delivering actor is carrying.
		/// </summary>
		void MakeDelivery(List<Pair<ActorInfo, string>> actorInfos, Actor deliveringActor, World world)
		{
			landedActor = deliveringActor;

			if (!actorInfos.Any())
			{
				TakeOff(deliveringActor, world);
				landedActor = null;
			}
			else
			{
				var actorInfo = actorInfos.First();

				var chosenExit = GetAvailableExit(self, actorInfo.First);
				if (chosenExit == null)
				{
					deliveringActor.QueueActivity(new Wait(10));
					deliveringActor.QueueActivity(new CallFunc(() => MakeDelivery(actorInfos, deliveringActor, world)));
					return;
				}

				var exitLocation = self.Location + chosenExit.ExitCell;
				var targetLocation = RallyPoint.Value != null ? RallyPoint.Value.Location : exitLocation;

				var newActor = DoProduction(self, actorInfo.First, chosenExit, actorInfo.Second);

				actorInfos.Remove(actorInfo);

				world.AddFrameEndTask(w =>
				{
					MoveIntoWorld(w, newActor, chosenExit, exitLocation, targetLocation);
					NotifyProduction(self, newActor, exitLocation);

					deliveringActor.QueueActivity(new CallFunc(() => MakeDelivery(actorInfos, deliveringActor, world)));
					Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", info.ReadyAudio, self.Owner.Country.Race);
				});
			}
		}

		void TakeOff(Actor deliveringActor, World world)
		{
			if (isPlane)
				deliveringActor.QueueActivity(new Fly(deliveringActor, Target.FromCell(world, endPos)));
			else
				deliveringActor.QueueActivity(new HeliFly(deliveringActor, Target.FromCell(world, endPos)));

			deliveringActor.QueueActivity(new RemoveSelf());
			production = new List<Pair<ActorInfo, string>>();
		}

		public void Killed(Actor self, AttackInfo e)
		{
			if (landedActor == null)
				return;

			if (info.DeliveryActorLives)
			{
				TakeOff(landedActor, self.World);
			}
			else
			{
				landedActor.Kill(e.Attacker);
			}
		}

		public void Selling(Actor self)
		{
			if (landedActor != null)
				TakeOff(landedActor, self.World);
		}

		public void Sold(Actor self) { }
	}
}
