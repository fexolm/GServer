# GServer

## About GServer
 * [What is GServer](#wtf)
 * [What is reliable udp](#r_udp)
 * [Why reliable udp is better than tcp](#udp_better)

## Usage

### Classes
 * [Host](#host)
 * [Message](#message)
 * [DataStorage](#datastorage)
 * [Plugins](#plugins)

### Examples
 * [Starting server](#start_server)
 * [Handling messages](#handling_messages)
 * [Adding plugins](#adding_plugins)

<a name="wtf"></a>
# What is GServer
**GServer** is reliable UDP networking library, designed for games. 
Its main advantages are:
 * [Reliable UDP](#r_udp) protocol
 * [Plugin architecture](#plugins) which provides high flexibility
 * It's extremely easy to use

<a name="r_udp"></a>
# What is reliable UDP
**Reliable udp** (RUDP) is a transport layer protocol designed for situations where TCP adds too much overhead, but nonetheless we need guaranteed-order packet delivery.

<a name="r_mods"></a>
## RUDP mods
RUDP supports 3 mods and their combinations.
GServer implements some of the most useful of them:
 * No mode - works like standard UDP
 * Reliable - provides delivery checking and guarantees that all packages will be delivered
 * Reliable sequenced - guarantees that all packages that are delivered will be processed in correct order, but no implication of delivering all packages is made
 * Reliable ordered - guarantees that all packages will be delivered and will be processed in correct order

<a name="udp_better"></a>
# Why reliable udp is better than tcp
 * Reliable udp provides more flexibility than udp because of it's modes
 * Reliable ordered mode simulates tcp behavior but works faster since it doesn't need to wait for lost packages. 

# Classes

<a name="host"></a>
## Host

Host is the main class in **GServer**. It provides methods to receive, send and process messages.
Messages are accept automatically in listen thread.

Messages are send using the "Send" method and processed with handlers, which is registered by user using the "AddHandler" method. See [examples](#examples).

<a name="message"></a>
## Message
Messages are used to send data from one host to another.
It has 3 parameters:
 * type : short - message type which is used to add handler to specific messages.
 * mode : Mode - reliable mode (see [RUDP mods](#r_mods))
 * body : byte[] - serialized data in an array of bytes

<a name="datastorage"></a>
## DataStorage
DataStorage is used to serialize data into byte array. 
It has 2 mods:
 * Calling the constructor without params enables *write only* mode.
 * Calling the constructor with byte[] or MemoryStream param enables *read only* mode.

In *read only* mode you could use just Read methods which are reading data from buffer.
In *write only* mode you could use just Write methods which are writing data into buffer.

<a name="plugins"></a>
## Plugins
In process ...

<a name="examples"></a>
# Examples

<a name="start_server"></a>
## Start server
*server*

```cs
Host host= new Host(portNumber); //instantiate host on portNumber port
host.StartListen(numberOfThreads); //StartListen on numberOfThreads threads

Timer timer = new Timer(o=>host.Tick());
timer.Change(10,10); // enables timer tick every 10 milliseconds
```

*client*

```cs
Host host= new Host(portNumber); //instantiate host on portNumber port
host.StartListen(numberOfThreads); //StartListen on numberOfThreads threads

Timer timer = new Timer(o=>host.Tick());
timer.Change(10,10); // enables timer tick every 10 milliseconds

host.OnConnect = () => Console.WriteLine("Connected"); // connect handler 

host.BeginConnect(serverEndpoint); // connecting to server endpoint

```

<a name="handling_messages"></a>
## Handling messages

```cs
/* host inicialization here */

//add handler to message with id == messageId
//when message with that id will arrive callback will be invoked
//connection - connection associated with sender
host.AddHanlder(messageId, (message, connection) => 
{
    /* deserialize message buffer */
    /* process message buffer */
    /* send response if needed */
});
```

<a name="datastorage"></a>
## DataStorage
```cs
class SimpleClass : ISerializable, IDeserializable 
{
     public int IntField {get;set;}
     public byte[] ByteArray {get;set} // arraySize = IntField
     public byte[] Serialize() 
     {
          var ds = DataStorage.CreateForWrite();
          ds.Push(IntField);
          ds.Push(ByteArray);
          return ds.Serialize();
     }
     public void FillDeserialize(byte[] buffer)
     {
          var ds = DataStorage.CreateForRead(buffer);
          IntField = ds.ReadInt32();
          ByteArray = ds.ReadBytes(IntField);
     }
}
```


