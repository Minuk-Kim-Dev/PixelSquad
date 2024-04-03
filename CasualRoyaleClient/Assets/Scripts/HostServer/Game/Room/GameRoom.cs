﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace HostServer.Game
{
	public class GameRoom : JobSerializer
	{
		public int RoomId { get; set; }

		Dictionary<int, Player> _players = new Dictionary<int, Player>();
		public Dictionary<int, Player> Players { get { return _players; } }

		Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
		public Dictionary<int, Projectile> Projectiles { get { return _projectiles; } }

		public Map Map { get; private set; } = new Map();

		public void Init()
		{

		}

		// 누군가 주기적으로 호출해줘야 한다
		public void Update()
		{
			foreach (Projectile projectile in _projectiles.Values)
			{
				projectile.Update();
			}

			Flush();
		}

		public void EnterGame(GameObject gameObject)
		{
			if (gameObject == null)
				return;

			GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

			if (type == GameObjectType.Player)
			{
				Player player = gameObject as Player;
				_players.Add(gameObject.Id, player);
				player.Room = this;

				Map.ApplyMove(player, new Vector2(player.Pos.x, player.Pos.y));

				// 본인한테 정보 전송
				{
					HC_EnterGame enterPacket = new HC_EnterGame();
					enterPacket.Player = player.Info;
					player.Session.Send(enterPacket);

					HC_Spawn spawnPacket = new HC_Spawn();
					foreach (Player p in _players.Values)
					{
						if (player != p)
							spawnPacket.Objects.Add(p.Info);
					}

					foreach (Projectile p in _projectiles.Values)
						spawnPacket.Objects.Add(p.Info);

					player.Session.Send(spawnPacket);
				}
			}
			else if (type == GameObjectType.Projectile)
			{
				Projectile projectile = gameObject as Projectile;
				_projectiles.Add(gameObject.Id, projectile);
				projectile.Room = this;

				Map.ApplyMove(projectile, new Vector2(projectile.Pos.x, projectile.Pos.y));
			}

			// 타인한테 정보 전송
			{
				HC_Spawn spawnPacket = new HC_Spawn();
				spawnPacket.Objects.Add(gameObject.Info);
				foreach (Player p in _players.Values)
				{
					if (p.Id != gameObject.Id)
						p.Session.Send(spawnPacket);
				}
			}
		}

		public void LeaveGame(int objectId)
		{
			GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

			if (type == GameObjectType.Player)
			{
				Player player = null;
				if (_players.Remove(objectId, out player) == false)
					return;

				Map.ApplyLeave(player);
				player.Room = null;

				// 본인한테 정보 전송
				{
					HC_LeaveGame leavePacket = new HC_LeaveGame();
					player.Session.Send(leavePacket);
				}
			}
			else if (type == GameObjectType.Projectile)
			{
				Projectile projectile = null;
				if (_projectiles.Remove(objectId, out projectile) == false)
					return;

				projectile.Room = null;
			}

			// 타인한테 정보 전송
			{
				HC_Despawn despawnPacket = new HC_Despawn();
				despawnPacket.ObjectIds.Add(objectId);
				foreach (Player p in _players.Values)
				{
					if (p.Id != objectId)
						p.Session.Send(despawnPacket);
				}
			}
		}

		public void HandleMove(Player player, CH_Move movePacket)
		{
			if (player == null)
				return;

			PositionInfo movePosInfo = movePacket.PosInfo;
			ObjectInfo info = player.Info;

			// 다른 좌표로 이동할 경우, 갈 수 있는지 체크
			if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
			{
				if (Map.CanGo(new Vector2(movePosInfo.PosX, movePosInfo.PosY)) == false)
					return;
			}

			info.PosInfo = movePosInfo;
			Map.ApplyMove(player, new Vector2(movePosInfo.PosX, movePosInfo.PosY));

			// 다른 플레이어한테 알려준다
			HC_Move resMovePacket = new HC_Move();
			resMovePacket.ObjectId = player.Info.ObjectId;
			resMovePacket.PosInfo = movePacket.PosInfo;

			Broadcast(resMovePacket);
		}

		public void HandleShoot(Player player, CH_Shoot shootPacket)
		{
			if (player == null)
				return;

			ObjectInfo info = player.Info;

			//사격 가능 여부 체크
			HC_Shoot shoot = new HC_Shoot();
			shoot.ObjectId = info.ObjectId;

			Broadcast(shoot);

			switch (shootPacket.WeaponType)
			{
				case WeaponType.Hg:
					{
						Bullet bullet = ObjectManager.Instance.Add<Bullet>();
						if (bullet == null)
							return;

						bullet.Owner = player;
						bullet.PosInfo.State = ActionState.Run;
						bullet.PosInfo.DirX = shootPacket.PosInfo.DirX;
						bullet.PosInfo.DirY = shootPacket.PosInfo.DirY;
						bullet.PosInfo.PosX = shootPacket.PosInfo.WeaponPosX;
						bullet.PosInfo.PosY = shootPacket.PosInfo.WeaponPosY;
						bullet.Speed = 20f;
						bullet.WeaponType = WeaponType.Hg;

						Push(EnterGame, bullet);
					}
					break;
			}
		}

		public Player FindPlayer(Func<GameObject, bool> condition)
		{
			foreach (Player player in _players.Values)
			{
				if (condition.Invoke(player))
					return player;
			}

			return null;
		}

		public void Broadcast(IMessage packet)
		{
			foreach (Player p in _players.Values)
			{
				p.Session.Send(packet);
			}
		}
	}
}