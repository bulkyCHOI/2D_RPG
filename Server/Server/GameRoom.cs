using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;
using ServerCore;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();    //잡큐를 사용하게 되면서 여기서 굳이 락을 잡지 않아도 된다. 잡큐에서 잡고 있으므로.
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);
            //Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment) //특정 패킷이 아닌 모든 패킷을 보내기위해 수정해줌
        {
            _pendingList.Add(segment);  //리스트에 모으는 작업 >> 보내는것은 Main() - Program - while문 안쪽 - Flush를 실행
        }
        public void Enter(ClientSession session)
        {
            //플레이어 추가
            _sessions.Add(session);
            session.Room = this;

            //신규 접속 플레이어에게 기존 접속 플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();
            foreach (ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player() 
                { 
                    isSelf = (s==session),
                    playerId = s.SessionID,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            session.Send(players.Write());

            //신규 플레이어를 모두에게 알린다.
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionID;
            enter.posX = session.PosX;
            enter.posY = session.PosY;
            enter.posZ = session.PosZ;
            Broadcast(enter.Write());
        }
        
        public void Leave(ClientSession session)
        {
            //플레이어 제거
            _sessions.Remove(session);

            //플레이어가 나간것을 모두에게 알리자
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionID;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            //좌표를 세션에 넣어주고
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            //모두에게 변경된 좌표를 알린다.
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionID;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }
    }
}
