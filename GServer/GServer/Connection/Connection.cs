using GServer.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GServer
{
    public class Connection
    {
        public readonly IPEndPoint EndPoint;
        public readonly Token Token;
        public DateTime LastActivity { get; private set; }
        public Connection(IPEndPoint endPoint)
            : this(endPoint, Token.GenerateToken())
        {
        }
        public Connection(IPEndPoint endPoint, Token token)
        {
            EndPoint = endPoint;
            Token = token;
            LastActivity = DateTime.Now;
            _AckPerMsgType = new Dictionary<short, Ack>();
            _lastMessageNumPerType = new Dictionary<short, int>();
            _handlerQueue = new SortedList<int, Action>();
        }
        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }
        public event Action<Connection> Disconnected;
        public void Disconnect()
        {
            if (Disconnected != null)
                Disconnected.Invoke(this);
        }

        private readonly IDictionary<short, int> _lastMessageNumPerType;
        #region Reliable

        private readonly IDictionary<short, Ack> _AckPerMsgType;
        public Message GenerateAck(Message msg)
        {
            int bitField;
            lock (_AckPerMsgType)
            {
                if (_AckPerMsgType.ContainsKey((short)msg.Header.Type))
                {
                    bitField = _AckPerMsgType[(short)msg.Header.Type].GetStatistic(msg.Header.MessageId);
                }
                else
                {
                    _AckPerMsgType.Add((short)msg.Header.Type, new Ack(msg.Header.MessageId));
                    bitField = 1;
                }
            }
            return Message.Ack(msg.Header, bitField);
        }

        #endregion

        #region Sequenced
        public bool IsMessageInItsOrder(short type, int num)
        {
            lock (_lastMessageNumPerType)
            {
                if (_lastMessageNumPerType.ContainsKey(type))
                {
                    if (_lastMessageNumPerType[type] < num)
                    {
                        _lastMessageNumPerType[type] = num;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _lastMessageNumPerType.Add(type, num);
                    return true;
                }
            }
        }
        #endregion

        #region Ordered

        private SortedList<int, Action> _handlerQueue;
        public void InvokeOrdered(Message msg, Action callback)
        {
            List<KeyValuePair<int, Action>> actionsToInvoke = new List<KeyValuePair<int, Action>>();

            lock (_lastMessageNumPerType)
            {
                    if (!_lastMessageNumPerType.ContainsKey((short)msg.Header.Type))
                    {
                        _lastMessageNumPerType.Add((short)msg.Header.Type, 0);
                    }
            }
            lock (_handlerQueue)
            {
                _handlerQueue.Add(msg.Header.MessageId, callback);
                foreach (var element in _handlerQueue)
                {
                    lock (_lastMessageNumPerType)
                    {
                        if (element.Key == _lastMessageNumPerType[(short)msg.Header.Type] + 1)
                        {
                            actionsToInvoke.Add(element);
                            _lastMessageNumPerType[(short)msg.Header.Type]++;
                        }
                    }
                }
                foreach (var element in actionsToInvoke)
                {
                    _handlerQueue.Remove(element.Key);
                }
            }
            foreach (var action in actionsToInvoke)
            {
                action.Value.Invoke();
            }

        }
        #endregion
    }
}
