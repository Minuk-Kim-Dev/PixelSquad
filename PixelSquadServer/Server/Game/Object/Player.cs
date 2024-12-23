﻿using Google.Protobuf.Protocol;
using Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Object
{
	public class Player : GameObject
	{
		public ClientSession Session { get; set; }
		public bool Ready { get; set; } = false;
		public Authority Authority { get; set; } = Authority.Client;

		public Player()
		{
			ObjectType = GameObjectType.Player;
		}

		public override void OnDamaged(GameObject attacker, float damage)
		{
			base.OnDamaged(attacker, damage);
		}

		public override void OnDead(GameObject attacker)
		{
			if (Room == null)
				return;

			State = ActionState.Dead;

			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			diePacket.Rank = Room.GetRank();
			diePacket.AttackerId = attacker.Id;
			Room.Broadcast(diePacket);

			Room.AlivePlayers.Remove(Id);
			if (Room.AlivePlayers.Count == 1)
				Room.EndGame();
		}
	}
}
