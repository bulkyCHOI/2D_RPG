using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class SessionManager
    {
        static SessionManager _instance = new SessionManager(); //간단하게 싱글톤을 만들고
        public static SessionManager instance { get { return _instance; } }

        int _sessionId = 0;
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        object _lock = new object();

        public ClientSession Generate()
        {
            lock (_lock)
            {
                int sessionId = ++_sessionId;

                ClientSession session = new ClientSession();
                session.SessionID = sessionId;
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        public ClientSession Find(int sessionId)
        {
            lock (_lock)
            {
                ClientSession session = null;
                _sessions.TryGetValue(sessionId, out session);
                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionID);
            }
        }
    }
}
