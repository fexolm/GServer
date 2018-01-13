﻿using System;
using System.IO;

// ReSharper disable UseNullPropagation
// ReSharper disable ArrangeAccessorOwnerBody

namespace GServer.Containers
{
    public class DataStorage : ISerializable, IDisposable
    {
        private readonly MemoryStream Stream;
        private readonly BinaryReader Reader;
        private readonly BinaryWriter Writer;

        private DataStorage(byte[] buffer) {
            Stream = new MemoryStream(buffer);
            Reader = new BinaryReader(Stream);
        }

        private DataStorage() {
            Stream = new MemoryStream();
            Writer = new BinaryWriter(Stream);
        }

        public static DataStorage CreateForRead(byte[] buffer) {
            return new DataStorage(buffer);
        }

        public static DataStorage CreateForWrite() {
            return new DataStorage();
        }

        public byte[] Serialize() {
            return Stream.ToArray();
        }

        public DataStorage Push(byte val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(short val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(int val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(long val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(bool val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(char val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(double val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(decimal val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(float val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(string val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        public DataStorage Push(Guid val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val.ToByteArray());
            return this;
        }

        public DataStorage Push(byte[] val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            Writer.Write(val);
            return this;
        }

        [Obsolete]
        public DataStorage Push(IDeepSerializable val) {
            if (Writer == null)
                throw new Exception("DataStorage in read only mode");
            val.PushToDs(this);
            return this;
        }

        public byte ReadByte() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadByte();
        }

        public byte[] ReadBytes(int count) {
            if (Reader == null) {
                throw new Exception("DataStorage in write only mode");
            }
            return Reader.ReadBytes(count);
        }

        public short ReadInt16() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadInt16();
        }

        public int ReadInt32() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadInt32();
        }

        public long ReadInt64() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadInt64();
        }

        public char ReadChar() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadChar();
        }

        public bool ReadBoolean() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadBoolean();
        }

        public string ReadString() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadString();
        }

        public double ReadDouble() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadDouble();
        }

        public decimal ReadDecimal() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadDecimal();
        }

        public float ReadFloat() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadSingle();
        }

        public Guid ReadGuid() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return new Guid(Reader.ReadBytes(16));
        }

        public byte[] ReadToEnd() {
            if (Reader == null)
                throw new Exception("DataStorage in write only mode");
            return Reader.ReadBytes((int) (Stream.Length - Stream.Position));
        }

        public bool Empty {
            get { return Stream.Position == Stream.Length; }
        }

        public void Dispose() {
            if (Reader != null) Reader.Close();
            if (Writer != null) Writer.Close();
            if (Stream != null) Stream.Close();
        }

        ~DataStorage() {
            Dispose();
        }
    }
}