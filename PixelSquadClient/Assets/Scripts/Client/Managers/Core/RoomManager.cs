﻿using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class RoomManager
{
    public Dictionary<int, RoomInfo> RoomInfo { get; set; } = new Dictionary<int, RoomInfo>();
    public Dictionary<int, Room> Room { get; set; } = new Dictionary<int, Room>();

    public Room _selected {  get; set; }

    public bool UpdateRoomList()
    {
        Transform parent = null;

        if (Managers.Scene.CurrentScene.SceneType == Scene.Lobby)
        {
            parent = UnityEngine.GameObject.Find("Content").transform;
        }

        if (parent == null)
            return false;

        List<int> removeRoom = new List<int>();
        foreach(Room room in Room.Values)
        {
            if (!RoomInfo.ContainsKey(room.RoomId))
            {
                removeRoom.Add(room.RoomId);
                Managers.Resource.Destroy(room.gameObject);
            }
        }
        foreach(int roomId in removeRoom)
        {
            Room.Remove(roomId);
        }

        foreach (RoomInfo room in RoomInfo.Values)
        {
            if (!Room.ContainsKey(room.RoomId))
            {
                UnityEngine.GameObject go = Managers.Resource.Instantiate("UI/Room", parent);
                go.GetComponent<Room>().Init(room);
                Room.Add(room.RoomId, go.GetComponent<Room>());
            }
        }

        return true;
    }

    public void Clear()
    {
        RoomInfo.Clear();
        Room.Clear();
    }
}
