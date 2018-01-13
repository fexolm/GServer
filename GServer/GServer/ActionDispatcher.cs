using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GServer
{
    public static class ActionDispatcher
    {
        private static Queue<Action> _actions = new Queue<Action>();
        private static Timer timer;

        public static void Start(int timeout) {
            timer = new Timer((o) => {
                lock (_actions) {
                    if (_actions.Any()) {
                        _actions.Dequeue().Invoke();
                    }
                }
            });
            timer.Change(timeout, timeout);
        }

        public static void Stop() {
            timer.Dispose();
        }

        public static void Enqueue(Action action) {
            lock (_actions) {
                _actions.Enqueue(action);
            }
        }
    }
}