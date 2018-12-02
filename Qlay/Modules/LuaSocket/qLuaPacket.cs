using MoonSharp.Interpreter;
using System.IO;

namespace Qlay.Modules.LuaSocket
{
    public class qLuaPacket
    {
        public Table luaTable;
        public qStream stream;

        public uint id;
        public byte[] data;
        public qConnection sender;

        public qLuaPacket()
        {
            if (stream == null)
                stream = new qStream(new MemoryStream());
        }

        public qLuaPacket(uint id)
        {
            if (id != 0)
                this.id = id;

            if (stream == null)
                stream = new qStream(new MemoryStream());
        }

        #region Read
        public bool ReadBool()
        {
            return stream.ReadBool();
        }

        public short ReadShort()
        {
            return stream.ReadShort();
        }

        public ushort ReadUShort()
        {
            return stream.ReadUshort();
        }

        public int ReadInt()
        {
            return stream.ReadInt();
        }

        public uint ReadUInt()
        {
            return stream.ReadUint();
        }

        public long ReadLong()
        {
            return stream.ReadLong();
        }

        public ulong ReadULong()
        {
            return stream.ReadUlong();
        }

        public float ReadFloat()
        {
            return stream.ReadFloat();
        }

        public double ReadDouble()
        {
            return stream.ReadDouble();
        }

        public string ReadString(short len = 0)
        {
            return stream.ReadString();
        }
        #endregion
        #region Write
        public void WriteBool(bool value)
        {
            stream.Write(value);
        }

        public void WriteShort(short num)
        {
            stream.Write(num);
        }

        public void WriteUShort(ushort num)
        {
            stream.Write(num);
        }

        public void WriteInt(int num)
        {
            stream.Write(num);
        }

        public void WriteUInt(uint num)
        {
            stream.Write(num);
        }

        public void WriteLong(long num)
        {
            stream.Write(num);
        }

        public void WriteULong(ulong num)
        {
            stream.Write(num);
        }

        public void WriteFloat(float num)
        {
            stream.Write(num);
        }

        public void WriteDouble(double num)
        {
            stream.Write(num);
        }

        public void WriteString(string str)
        {
            stream.Write(str);
        }
        #endregion
    }
}
