using GServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace p1
{
    class Program
    {
        static void DecodeAndShow(Message msg)
        {
            var ds = new DataStorage(msg.Body);
            Console.WriteLine(ds.ReadString());
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Введите порт");
            Host host = new Host(int.Parse(Console.ReadLine()));
            host.StartListen(0);

            Console.WriteLine("Установить соединение?");
            if (Console.ReadLine() == "yes")
            {
                Console.WriteLine("Введите ip второго клиента");
                var ip = IPAddress.Parse(Console.ReadLine());
                Console.WriteLine("Введите порт второго клиента");
                var port = int.Parse(Console.ReadLine());
                host.Connect(new IPEndPoint(ip, port));
                Random rnd = new Random();
                Console.WriteLine("Соединение установленно");
            }
            MessageCounter mc = 0;
            host.AddHandler((short)MessageType.Encoded, (msg, ep) => DecodeAndShow(msg));
            host.AddHandler((short)MessageType.Ack, (msg, ep) => Console.WriteLine("Сообщение доставленно"));
            while (true)
            {
                var ds = new DataStorage();
                ds.Push(Console.ReadLine());
                Console.WriteLine(mc.ToString());
                host.Send(new Message(MessageType.Encoded, Mode.Reliable | Mode.Ordered, null, mc, ds));
                mc++;
            }
        }
    }
}
