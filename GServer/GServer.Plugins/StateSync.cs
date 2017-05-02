using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GServer.Containers;

namespace GServer.Plugins
{
    public class StateObject : ISerializable, IDeserializable, IDeepDeserializable, IDeepSerializable
    {
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _speed;
        private Vector3 _angularSpeed;
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                Changed = true;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                Changed = true;
            }
        }
        public Vector3 Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
                Changed = true;
            }
        }
        public Vector3 AngularSpeed
        {
            get
            {
                return _angularSpeed;
            }
            set
            {
                _angularSpeed = value;
                Changed = true;
            }
        }
        public long TickIndex { get; set; }
        public bool Changed { get; set; }
        public void FillDeserialize(byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            ReadFromDs(ds);
        }
        public void PushToDs(DataStorage ds)
        {
            ds.Push(Position);
            ds.Push(Rotation);
            ds.Push(Speed);
            ds.Push(AngularSpeed);
            ds.Push(TickIndex);
        }
        public void ReadFromDs(DataStorage ds)
        {
            Position = new Vector3();
            Position.ReadFromDs(ds);
            Rotation = new Quaternion();
            Rotation.ReadFromDs(ds);
            Speed = new Vector3();
            Speed.ReadFromDs(ds);
            AngularSpeed = new Vector3();
            AngularSpeed.ReadFromDs(ds);
            TickIndex = ds.ReadInt64();
        }
        public byte[] Serialize()
        {
            var ds = DataStorage.CreateForWrite();
            PushToDs(ds);
            return ds.Serialize();
        }
    }
    public class StateSync : IPlugin
    {
        private IDictionary<int, StateObject> _objects;
        private Host _host;
        public void AddObject(int stateId, StateObject state)
        {
            _objects[stateId] = state;
        }
        public List<Connection> Connections { get; private set; }
        public StateSync()
        {
            _objects = new Dictionary<int, StateObject>();
            Connections = new List<Connection>();
        }
        public void GenerateStateUpdate()
        {
            foreach (var obj in _objects)
            {
                if (obj.Value.Changed)
                {
                    var ds = DataStorage.CreateForWrite();
                    ds.Push(obj.Key);
                    obj.Value.TickIndex = DateTime.UtcNow.Ticks;
                    ds.Push(obj.Value);
                    var msg = new Message(4400, Mode.Reliable | Mode.Sequenced, ds.Serialize());
                    foreach (var con in Connections)
                    {
                        _host.Send(msg, con);
                    }
                }
            }
        }

        private void OnStateReceived(Message msg, Connection con)
        {
            var data = DataStorage.CreateForRead(msg.Body);
            while (!data.Empty)
            {
                int id = data.ReadInt32();
                StateObject so = new StateObject();
                so.ReadFromDs(data);
                OnStateChanged.Invoke(id, so);
            }
        }

        public void Bind(Host host)
        {
            _host = host;
            _host.AddHandler(4400, OnStateReceived);
        }

        public event Action<int, StateObject> OnStateChanged;
    }
}
