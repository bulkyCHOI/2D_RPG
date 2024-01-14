using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }
    public class JobQueue : IJobQueue
    {
        Queue<Action> _jobQueue = new Queue<Action>();
        object _lock = new object();
        bool _flush = false;

        public void Push(Action job)
        {
            bool flush = false;
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                if(_flush == false)
                    flush = _flush = true;
            }
            if (flush)  //처음 일감을 맡은 놈이 Flush(실행)까지 한다.
                Flush();
        }

        void Flush()
        {
            while (true)
            {
                Action action = Pop();
                if (action == null) //일이 쌓여있으면 계속함
                    return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock ( _lock)
            {
                if( _jobQueue.Count == 0 )
                {
                    _flush = false;
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}
