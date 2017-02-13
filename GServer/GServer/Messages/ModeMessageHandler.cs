using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GServer
{
    public class ModeMessageHandler
    {           
        public IDictionary<MessageType, Int16> TypesCountsWeighting { get; private set; }        
        public IDictionary<MessageType, List<Int16>> DictOrdered { get; private set; }
        public Message HeaderWorker(Message Mess)
        {
            if(!TypesCountsWeighting.ContainsKey(Mess.Header.Type))
            {
                TypesCountsWeighting.Add(Mess.Header.Type, 1);
            }  
            if (!Mess.Header.Sequensed )
            { }
            else if (Mess.Header.Ordered)
            {
                if (Mess.Header.TypeId >= TypesCountsWeighting[Mess.Header.Type])
                {
                    if (DictOrdered.ContainsKey(Mess.Header.Type))
                    {
                        DictOrdered[Mess.Header.Type].Add(Mess.Header.TypeId);
                        while(DictOrdered[Mess.Header.Type].Contains(TypesCountsWeighting[Mess.Header.Type]))
                        {                            
                            Int16 previousCount = TypesCountsWeighting[Mess.Header.Type]++;
                            DictOrdered[Mess.Header.Type].Remove(previousCount);
                        }
                    }
                    else DictOrdered.Add(Mess.Header.Type, new List<short>());                   
                }
            }
            else
                if (Mess.Header.TypeId >= TypesCountsWeighting[Mess.Header.Type])
            {
                TypesCountsWeighting[Mess.Header.Type] = (Int16)(Mess.Header.TypeId + 1);
            }
            if (Mess.Header.Reliable)
            {
                AckBody ack = new AckBody(Mess.Header.MessageId);
                Message message = new Message(MessageType.Ack, Mode.Unreliable, Mess.Header.ConnectionToken, ack);
                return message;
            }               
            else
                return null;
            
        } 
        public ModeMessageHandler()
        {

        }       
    }
    public class AckBody : ISerializable
    {
        public int messageId { get; set; }
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(messageId);
                }
                return m.ToArray();
            }
        }
        public AckBody(int _id)
        {
            messageId = _id;
        }
    }   
}


/* 




    создать методы для обработки каждого типа сообщения 
    в методах обработки принимать результат функции MessBuilder
    если sequ/ord  то идет сравнение с typeID последнего пакета того же типа
    иначе обработка по типу 
     
     
     */