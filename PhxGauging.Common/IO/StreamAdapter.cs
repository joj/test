using System;
using System.IO;
using System.Threading;

#if COMPACT_FRAMEWORK
using System.Net.Sockets;
#endif

namespace PhxGauging.Common.IO
{
    public class StreamAdapter : IInputStream, IOutputStream
    {
        private readonly Stream stream;

        public StreamAdapter(Stream stream)
        {
            this.stream = stream;

        }

        public byte ReadByte()
        {
            return (byte)stream.ReadByte();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        public void Flush()
        {
            //StreamReader reader = new StreamReader(stream);
            //if(!reader.EndOfStream)
            //    reader.ReadToEnd();

            stream.Flush();
        }
    }

#if COMPACT_FRAMEWORK
    public class NetworkStreamAdapter : IInputStream, IOutputStream
    {
         private readonly NetworkStream stream;

         public NetworkStreamAdapter(NetworkStream stream)
        {
            this.stream = stream;
        }

        public byte ReadByte()
        {
            int count = 0;

            while (count < 50 && !stream.DataAvailable)
            {
                Thread.Sleep(10);
                count++;
            }

            if (!stream.DataAvailable)
            {
                throw new Exception("No data available to read");
            }
            
            return (byte)stream.ReadByte();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        public void Flush()
        {
            //StreamReader reader = new StreamReader(stream);
            //if(!reader.EndOfStream)
            //    reader.ReadToEnd();

            stream.Flush();
        }
    }
#endif
}