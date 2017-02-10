using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    class ModeMessageBuilder
    {
        public byte b = (byte)2;
        public int i = 6;           
        public ModeType Mode { get; set; }
        public Message BuildedMessage = new Message();
        public List<Message> MessageBuilder(Message Mess, )
        {

            BitArray mode = new BitArray(8);
            mode.Set(7, Mess.Header.Reliable);
            mode.Set(6, Mess.Header.Sequensed);
            mode.Set(5, Mess.Header.Ordered);
            byte[] modeByte = new byte[1];
            mode.CopyTo(modeByte, 0);
            Mode = (ModeType);
            List<Message> messageArray = new List<Message>();
            List<Message> orderedArray = new List<Message>();
            if (Mess.Header.Reliable)
                messageArray.Add(new Message(, MessageType.Ack, 0, null));
            else
                messageArray.Add(null);
            if (!Mess.Header.Sequensed)
            { }
            else if (Mess.Header.Ordered)
            {
                if (Mess.Header.TypeId = host.TypeCounts[Mess.Header.])
                { }
            }
            return messageArray;
        }
    }
}
/* 




    создать методы для обработки каждого типа сообщения 
    в методах обработки принимать результат функции MessBuilder
    если sequ/ord  то идет сравнение с typeID последнего пакета того же типа
    иначе обработка по типу 
     
     
     */