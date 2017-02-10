using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    class ModeMessageBuilder
    {
        byte b = (byte)2;           
        ModeType Mode { get; set; }
        Message BuildedMessage;
        public Message MessageBuilder(Message Mess, Host host)
        {                        
            switch (Mess.Header.Mode)
            {
                case ModeType.Unreliable:

                    return null;                    
                case ModeType.UnreliableSequenced:
                    return Mess;
                case ModeType.ReliableUnsequenced:                    
                    //BuildedMessage = new Message(host, MessageType.Ack, ModeType.Unreliable, );
                    //присвоить BM body значение Id Mess
                    return Mess;                    
                case ModeType.ReiableSequenced:
                    return Mess;                    
                case ModeType.ReliableOrdered:
                    return Mess;
             }
            throw new NotImplementedException();
        }
    }
}
/* 




    создать методы для обработки каждого типа сообщения 
    в методах обработки принимать результат функции MessBuilder
    если sequ/ord  то идет сравнение с typeID последнего пакета того же типа
    иначе обработка по типу 
     
     
     */