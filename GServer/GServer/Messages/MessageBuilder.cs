using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    public class MessageBuilder
    {
        public int allMesssageCount { get; private set; }
        public IDictionary<MessageType, Int16> TypesCountsSend { get; private set; }
        public Message MessageToSend(MessageType _type, Mode _mode, Token _token, ISerializable _body)
        {
            Message message = new Message(_type, _mode, _token, _body);
            message.Header.MessageId = ++allMesssageCount;
            if(message.Header.Sequensed)
            {
                if(!TypesCountsSend.ContainsKey(_type))
                {
                    TypesCountsSend.Add(_type, 1);
                }                 
                message.Header.TypeId = ++TypesCountsSend[_type];
            }            
            return message;
        }
        public Message MessageToSend(MessageType _type, Mode _mode)
        {
            Message message = new Message(_type, _mode, null);
            message.Header.MessageId = ++allMesssageCount;
            if (message.Header.Sequensed)
            {
                if (!TypesCountsSend.ContainsKey(_type))
                {
                    TypesCountsSend.Add(_type, 1);
                }
                message.Header.TypeId = ++TypesCountsSend[_type];
            }
            return message;
        }
        public MessageBuilder()
        {
            allMesssageCount = 0;
        }
    }

}
