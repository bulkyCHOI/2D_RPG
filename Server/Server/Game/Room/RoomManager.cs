﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();
        object _lock = new object();
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public GameRoom Add(int mapId)
        {
            GameRoom room = new GameRoom();
            //room.Init(mapId); //기존의 lock 방식
            room.Push(room.Init, mapId);    //Job 방식으로 push

            lock (_lock)
            {
                room.RoomId = _roomId++;
                _rooms.Add(room.RoomId, room);
            }

            return room;
        }

        public bool Remove(int roomId)
        {
            lock (_lock)
            {
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
