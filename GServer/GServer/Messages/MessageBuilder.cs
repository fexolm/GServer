using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Messages
{
    class MessageBuilder
    {
        public Message MessageModeBuilder(MessageType _type, Mode _mode, ISerializable _body)
        {
            Message message = new Message(_type, _mode, _body);
            if (message.Header.Reliable)
                message.Header.MessageId++;
            if (message.Header.Sequensed)
                message.Header.TypeId++;
            return message;
        }
    }
}
