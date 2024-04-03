﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using UnityEngine;
using HostServer.Game;
using Host;
using HostServer;

namespace HostServer
{
	public class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		public void Send(IMessage packet)
		{
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
			msgName = SecondCharToLower(msgName);
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 4];
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);
			Send(new ArraySegment<byte>(sendBuffer));
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Debug.Log($"OnConnected : {endPoint}");

			MyPlayer = HostServer.Game.ObjectManager.Instance.Add<Player>();
			{
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
				MyPlayer.Info.PosInfo.State = ActionState.Idle;
				MyPlayer.Info.PosInfo.Dir = DirX.Right;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;
				MyPlayer.Info.PosInfo.DirX = 0;
				MyPlayer.Info.PosInfo.DirY = 0;

				MyPlayer.Info.StatInfo.MaxHp = 50;
				MyPlayer.Info.StatInfo.Hp = 50;
				MyPlayer.Info.StatInfo.Speed = 2f;

				MyPlayer.WeaponType = WeaponType.Hg;

				MyPlayer.Session = this;
			}

			GameRoom room = HostServer.Game.RoomManager.Instance.Find(1);
			room.Push(room.EnterGame, MyPlayer);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameRoom room = HostServer.Game.RoomManager.Instance.Find(1);
			room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);

			SessionManager.Instance.Remove(this);

			Debug.Log($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{

		}

		public static string SecondCharToLower(string input)
		{
			if (string.IsNullOrEmpty(input))
				return "";
			return input[0] + input[1].ToString().ToLower() + input.Substring(2);
		}
	}
}