using Xunit;

namespace ChunkedDataTransfer.Tests
{
    public class ChunkedDataReceiverTests
    {
        private readonly ChunkedDataReceiver chunkedDataReceiver;

        public ChunkedDataReceiverTests()
        {
            this.chunkedDataReceiver = new ChunkedDataReceiver();
        }


        [Theory]
        [InlineData("This is a garbage chunk")]
        public void ProcessChunk_DoesNotThrowOnGarbageChunk(string garbageChunk)
        {
            this.chunkedDataReceiver.StartReceiving();
            this.chunkedDataReceiver.ProcessChunk(garbageChunk);
        }


        [Theory]
        [InlineData("This is a garbage chunk")]
        public void ProcessChunk_DoesNotReceiveGarbageChunkAsTotalResult(string garbageChunk)
        {
            int dataReceivedCalledCount = 0;
            this.chunkedDataReceiver.OnStringDataReceived += data => dataReceivedCalledCount++;
            this.chunkedDataReceiver.StartReceiving();
            this.chunkedDataReceiver.ProcessChunk(garbageChunk);

            Assert.True(dataReceivedCalledCount == 0);
        }
    }
}
