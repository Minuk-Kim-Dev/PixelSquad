﻿using Google.Protobuf.Protocol;
using Server.Define;
using Server.Room;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Object
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;

		public GameRoom Room { get; set; }

		public virtual BoxCollider2D Collider
		{
			get
			{
				return new BoxCollider2D(PosInfo.PosX, PosInfo.PosY, 1, 2);
			}
		}

		public BoxCollider2D AttackCollider
		{
			get
			{
				switch (ObjectType)
				{
					case GameObjectType.Player:
						//1사분면 위치
						if (LastDir.x >= 0 && LastDir.y > 0)
						{
							return new BoxCollider2D(PosInfo.PosX + 0.75f, PosInfo.PosY + 0.75f, 1.5f, 1.5f);
						}
						//2사분면 위치
						else if (LastDir.x < 0 && LastDir.y > 0)
						{
							return new BoxCollider2D(PosInfo.PosX - 0.75f, PosInfo.PosY + 0.75f, 1.5f, 1.5f);
						}
						//3사분면 위치
						else if (LastDir.x < 0 && LastDir.y <= 0)
						{
							return new BoxCollider2D(PosInfo.PosX - 0.75f, PosInfo.PosY - 0.75f, 1.5f, 1.5f);
						}
						//4사분면 위치
						else
						{
							return new BoxCollider2D(PosInfo.PosX + 0.75f, PosInfo.PosY - 0.75f, 1.5f, 1.5f);
						}
					default:
						return new BoxCollider2D(PosInfo.PosX, PosInfo.PosY, 0, 0);
				}
			}
		}

		#region ObjectInfo Info

		public ObjectInfo Info { get; set; } = new ObjectInfo();

		public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}

		public string Name
		{
			get { return Info.Name; }
			set { Info.Name = value; }
		}

		public JobType Class
		{
			get { return Info.Class; }
			set { Info.Class = value; }
		}

		#endregion

		#region PositionInfo PosInfo

		public PositionInfo PosInfo { get; private set; } = new PositionInfo();

		public ActionState State
		{
			get { return PosInfo.State; }
			set { PosInfo.State = value; }
		}

		public Vector2 Pos
		{
			get
			{
				return new Vector2(PosInfo.PosX, PosInfo.PosY);
			}

			set
			{
				PosInfo.PosX = value.x;
				PosInfo.PosY = value.y;
			}
		}

		public Vector2 LastDir
		{
			get
			{
				return new Vector2(PosInfo.LastDirX, PosInfo.LastDirY);
			}

			set
			{
				PosInfo.LastDirX = value.x;
				PosInfo.LastDirY = value.y;
			}
		}

		#endregion

		#region StatInfo StatInfo

		public StatInfo StatInfo { get; private set; } = new StatInfo();

		public float Hp
		{
			get { return StatInfo.Hp; }
			set { StatInfo.Hp = Math.Clamp(value, 0, StatInfo.MaxHp); }
		}

		public float Speed
		{
			get { return StatInfo.Speed; }
			set { StatInfo.Speed = value; }
		}

		public float Damage
		{
			get { return StatInfo.Damage; }
			set { StatInfo.Damage = value; }
		}

		public int FirstSkillId
        {
			get { return StatInfo.FirstSkillId; }
			set { StatInfo.FirstSkillId = value; }
        }

		public int SecondSkillId
		{
			get { return StatInfo.SecondSkillId; }
			set { StatInfo.SecondSkillId = value; }
		}

		#endregion

		public GameObject()
		{
			Info.PosInfo = PosInfo;
			Info.StatInfo = StatInfo;
		}

		public virtual void Update()
		{

		}

        public Vector2 GetFrontPos(Vector2 destPos, float speed)
        {
			Vector2 direction = destPos - Pos;
			float distance = direction.magnitude;

			if (distance <= speed)
				return destPos;

			Vector2 normalizedDirection = direction / distance;

			Vector2 newPos = Pos + normalizedDirection * speed;

			return newPos;
		}

        public virtual void OnDamaged(GameObject attacker, float damage)
		{
			if (Room == null)
				return;

			StatInfo.Hp = Math.Max(StatInfo.Hp - damage, 0);

			S_ChangeHp changePacket = new S_ChangeHp();
			changePacket.ObjectId = Id;
			changePacket.Hp = StatInfo.Hp;
			Room.Broadcast(changePacket);

			if (StatInfo.Hp <= 0)
			{
				OnDead(attacker);
			}
		}

		public virtual void OnDead(GameObject attacker)
		{
			if (Room == null)
				return;

			State = ActionState.Dead;

			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			diePacket.Rank = 0;
			diePacket.AttackerId = attacker.Id;
			Room.Broadcast(diePacket);

			if(ObjectType != GameObjectType.Player)
            {
				GameRoom room = Room;
				room.LeaveGame(Id);
			}
		}
	}
}
