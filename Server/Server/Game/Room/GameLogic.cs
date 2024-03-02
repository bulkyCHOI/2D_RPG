﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class GameLogic : JobSerializer
    {
        public static GameLogic Instance { get; } = new GameLogic();
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            Flush();

            foreach (GameRoom room in _rooms.Values)
            {
                room.Update();
            }
        }

        public GameRoom Add(int mapId)
        {
            GameRoom room = new GameRoom();
            //room.Init(mapId); //기존의 lock 방식
            room.Push(room.Init, mapId, 10);    //Job 방식으로 push

            room.RoomId = _roomId;
            _rooms.Add(room.RoomId, room);
            _roomId++;

            return room;
        }

        public bool Remove(int roomId)
        {
           return _rooms.Remove(roomId);
        }

        public GameRoom Find(int roomId)
        {
            GameRoom room = null;

            if (_rooms.TryGetValue(roomId, out room))
                return room;
            return null;
        }
    }
}
