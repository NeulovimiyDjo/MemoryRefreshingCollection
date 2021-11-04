using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ChunkedDataTransfer.Tests
{
    public class ChunkedDataTransferTests
    {
        private readonly Mock<IDataSender> dataSenderMock;
        private readonly ChunkedDataSender chunkedDataSender;
        private readonly ChunkedDataReceiver chunkedDataReceiver;

        public ChunkedDataTransferTests()
        {
            this.dataSenderMock = new Mock<IDataSender>();
            this.chunkedDataSender = new ChunkedDataSender(this.dataSenderMock.Object);
            this.chunkedDataReceiver = new ChunkedDataReceiver();
        }


        [Theory]
        [InlineData("This is a test data string", 99)]
        [InlineData("This is a test data string", 5)]
        public async Task SendReceive_TransfersAllStringDataIntact(string testData, int chunkSize)
        {
            string received = null;
            this.chunkedDataReceiver.OnStringDataReceived += data => received = data;
            this.chunkedDataReceiver.StartReceiving();

            this.dataSenderMock
                .Setup(x => x.SendAsync(It.IsAny<string>()))
                .Callback<string>(chunk => this.chunkedDataReceiver.ProcessChunk(chunk));

            this.chunkedDataSender.ChunkSize = chunkSize;
            await this.chunkedDataSender.SendAsync(testData);

            Assert.Equal(testData, received);
        }


        [Theory]
        [InlineData(new byte[] { 0, 7, 6, 173, 97, 240, 255, 233, 0 }, 99)]
        [InlineData(new byte[] { 0, 7, 6, 173, 97, 240, 255, 233, 0 }, 3)]
        public async Task SendReceive_TransfersAllByteDataIntact(byte[] testData, int chunkSize)
        {
            byte[] received = null;
            this.chunkedDataReceiver.OnByteDataReceived += data => received = data;
            this.chunkedDataReceiver.StartReceiving();

            this.dataSenderMock
                .Setup(x => x.SendAsync(It.IsAny<string>()))
                .Callback<string>(chunk => this.chunkedDataReceiver.ProcessChunk(chunk));

            this.chunkedDataSender.ChunkSize = chunkSize;
            await this.chunkedDataSender.SendAsync(testData);

            Assert.True(testData.SequenceEqual(received));
        }
    }
}
