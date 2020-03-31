namespace PhxGauging.Common.IO
{
    public interface IInputStream
    {
        byte ReadByte();
        void Flush();
    }
}