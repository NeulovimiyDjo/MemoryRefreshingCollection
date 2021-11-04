using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ChunkedDataTransfer.Tests
{
    public class ChunkedDataSenderTests
    {
        private readonly Mock<IDataSender> dataSenderMock;
        private readonly ChunkedDataSender chunkedDataSender;

        public ChunkedDataSenderTests()
        {
            this.dataSenderMock = new Mock<IDataSender>();
            this.chunkedDataSender = new ChunkedDataSender(this.dataSenderMock.Object);
        }


        [Theory]
        [InlineData(null)]
        public async Task SendAsync_ThrowsOnNull(object testData)
        {
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await this.chunkedDataSender.SendAsync(testData);
            });
        }


        [Theory]
        [InlineData(new byte[0])]
        [InlineData("")]
        public async Task SendAsync_ThrowsOnEmptyData(object testData)
        {
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await this.chunkedDataSender.SendAsync(testData);
            });
        }


        [Theory]
        [InlineData("This is a test data")]
        [InlineData(new byte[] { 0, 7, 6, 173, 97, 240, 255, 233, 0 })]
        public async Task SendAsync_CallsDataSender_AtLeastOnce(object testData)
        {
            await this.chunkedDataSender.SendAsync(testData);
            this.dataSenderMock.Verify(x => x.SendAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }


        [Theory]
        [InlineData("This is a test data", 99, 2)] // +1 part for PackageInfo message.
        [InlineData("This is a test data", 3, 5)]
        [InlineData(new byte[] { 0, 7, 6, 173, 97, 240, 255, 233, 0 }, 3, 4)]
        public async Task SendAsync_SplitsData(object testData, int chunkSize, int minPartsToSend)
        {
            this.chunkedDataSender.ChunkSize = chunkSize;
            await this.chunkedDataSender.SendAsync(testData);

            this.dataSenderMock.Verify(
                x => x.SendAsync(It.IsAny<string>()),
                Times.AtLeast(minPartsToSend)
            );
        }
    }
}
