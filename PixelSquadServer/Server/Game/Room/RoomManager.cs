﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Room
{
	public class RoomManager
	{
		public static RoomManager Instance { get; } = new RoomManager();

		object _lock = new object();
		Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
		public int _roomId = 1;

		//TODO : 게임룸 업데이트 관리

		public GameRoom Add(GameRoomInfo info)
		{
			GameRoom gameRoom = new GameRoom();
			Program.TickRoom(gameRoom, 50);
			gameRoom.Push(gameRoom.Init);

			lock (_lock)
			{
				info.Id = _roomId;
				gameRoom.Info = info;
				_rooms.Add(_roomId, gameRoom);
                Console.WriteLine($"Room[{gameRoom.Info.Name}] 생성");
				_roomId++;
			}

			return gameRoom;
		}

        public bool Remove(int roomId)
        {
            lock (_lock)
            {
                Program.StopTickRoom(_rooms[roomId]);
                return _rooms.Remove(roomId);
            }
        }

        public GameRoom Find(int roomId)
		{
			lock (_lock)
			{
				GameRoom room = null;
				if (_rooms.TryGetValue(roomId, out room))
					return room;

				return null;
			}
		}
	}
}
