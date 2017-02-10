using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    class ModeMessageBuilder
    {        
        ModeType Mode { get; set; }
        public Message MessageBuilder(Message Mess)
        {                        
            switch (Mess.Header.Mode)
            {
                case ModeType.Unreliable:
                    return null;                    
                case ModeType.UnreliableSequenced:
                    return Mes
                case ModeType.ReliableUnsequenced:
                    break;
                case ModeType.ReiableSequenced:
                    break;
                case ModeType.ReliableOrdered:
                    break;
            } 
        }
    }
}
/*
 в каждом кейсе написать обработчик сообщений соотв типа 
     
     */