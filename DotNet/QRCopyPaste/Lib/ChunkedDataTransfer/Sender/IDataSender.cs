using System.Threading.Tasks;

namespace ChunkedDataTransfer
{
    public interface IDataSender
    {
        Task SendAsync(string data);
        void Stop();
    }
}