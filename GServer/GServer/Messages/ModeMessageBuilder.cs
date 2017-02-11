using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    class ModeMessageBuilder
    {   
        public Message BuildedMessage = new Message();
        public IDictionary<MessageType, Int16> TypesCounts { get; private set; }
        public int AllMesssageCount { get; private set; }
        public IDictionary<MessageType, List<Int16>> DictOrdered { get; private set; }
        public List<Message> MessageBuilder(Message Mess)
        {

            BitArray mode = new BitArray(8);
            mode.Set(7, Mess.Header.Reliable);
            mode.Set(6, Mess.Header.Sequensed);
            mode.Set(5, Mess.Header.Ordered);
            byte[] modeByte = new byte[1];
            mode.CopyTo(modeByte, 0);
            List<Message> messageArray = new List<Message>();            
            if (Mess.Header.Reliable)
                messageArray.Add(new Message(MessageType.Ack, (Mode)modeByte[0], null));
            else
                messageArray.Add(null);
            if (!Mess.Header.Sequensed)
            { }
            else if (Mess.Header.Ordered)
            {
                if (Mess.Header.TypeId >= TypesCounts[Mess.Header.Type])
                {
                    if (DictOrdered.ContainsKey(Mess.Header.Type))
                    {
                        DictOrdered[Mess.Header.Type].Add(Mess.Header.TypeId);
                        while(DictOrdered[Mess.Header.Type].Contains(TypesCounts[Mess.Header.Type]))
                        {                            
                            Int16 previousCount = TypesCounts[Mess.Header.Type]++;
                            DictOrdered[Mess.Header.Type].Remove(previousCount);
                        }
                    }
                    else DictOrdered.Add(Mess.Header.Type, new List<short>());                   
                }
            }
            else
                if (Mess.Header.TypeId >= TypesCounts[Mess.Header.Type])
            {
                TypesCounts[Mess.Header.Type] = (Int16)(Mess.Header.TypeId + 1);
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