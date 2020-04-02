using System;
using System.IO;
using System.Text;

namespace HuiDesktop.Package
{
    public class BinaryFileHelper : IDisposable
    {
        const int BUFFER_SIZE = 102400;
        Stream stream;
        byte[] buffer = new byte[BUFFER_SIZE];

        public BinaryFileHelper(Stream stream)
        {
            this.stream = stream;
        }

        public byte ReadByte()
        {
            int read = stream.ReadByte();
            if (read == -1) throw new EndOfStreamException();
            return (byte)read;
        }

        public void WriteByte(byte v) => stream.WriteByte(v);

        public int ReadUnsignedInt()
        {
            int retval = 0;
            for (int i = 0; i < 4; ++i)
            {
                byte read = ReadByte();
                retval |= (read & 0x7f) << (i * 7);
                if ((read >> 7) == 0) return retval;
            }
            throw new InvalidDataException();
        }

        public void WriteUnsignedInt(int val)
        {
            do
            {
                if (val > 0x7f) stream.WriteByte((byte)((val & 0x7f) | 0x80));
                else stream.WriteByte((byte)(val & 0x7f));
                val >>= 7;
            } while (val != 0);
        }

        public byte[] ReadFully(int count)
        {
            byte[] vs = new byte[count];
            int pos = 0, remain = count;
            while (remain > 0)
            {
                int readed = stream.Read(vs, pos, remain);
                if (readed == 0) throw new EndOfStreamException();
                pos += readed;
                remain -= readed;
            }
            return vs;
        }

        public void WriteFully(byte[] vs) => stream.Write(vs, 0, vs.Length);

        public string ReadString()
        {
            int len = ReadUnsignedInt();
            if (len == 0) return string.Empty;
            return Encoding.UTF8.GetString(ReadFully(len));
        }

        public void WriteString(string str)
        {
            var vs = Encoding.UTF8.GetBytes(str);
            WriteUnsignedInt(vs.Length);
            if (vs.Length == 0) return;
            WriteFully(vs);
        }

        public void CopyTo(Stream dest, int length)
        {
            while(length > 0)
            {
                int read = stream.Read(buffer, 0, Math.Min(BUFFER_SIZE, length));
                if (read == 0) throw new EndOfStreamException();
                dest.Write(buffer, 0, read);
                length -= read;
            }
        }

        internal void CopyFrom(Stream src)
        {
            src.CopyTo(stream);
        }

        public void Dispose() => stream.Dispose();
    }
}
