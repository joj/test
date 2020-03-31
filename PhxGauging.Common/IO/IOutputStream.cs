namespace PhxGauging.Common.IO
{
    public interface IOutputStream
    {
        void Write(byte[] buffer, int offset, int count);
        void Flush();
    }
}