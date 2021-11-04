using System.Threading;
using DndBoard.SeleniumTestsBase.Fixtures;
using DndBoard.SeleniumTestsBase.Helpers;
using Xunit;

namespace DndBoard.SeleniumTestsBase
{
    public abstract class OverallTestsBase
    {
        private const int AddPosLeftTopX = 10;
        private const int AddPosLeftTopY = 10;
        private const int IconSize = 100;

        private const int StartPosMiddleX = AddPosLeftTopX + IconSize / 2;
        private const int StartPosMiddleY = AddPosLeftTopY + IconSize / 2;

        private readonly string _dataDir;
        private readonly ClientHelper _clientHelper;
        private readonly ClientHelper _clientHelper2;
        private readonly string _siteBaseAddress;


        public OverallTestsBase(SetupClientsFixtureBase setup)
        {
            _dataDir = setup.DataDir;
            _clientHelper = setup.ClientHelper;
            _clientHelper2 = setup.ClientHelper2;
            _siteBaseAddress = setup.SiteBaseAddress;

            _clientHelper.IconSize = IconSize;
            _clientHelper2.IconSize = IconSize;
        }


        [Fact]
        public void Connection_To_BoardPage_Works()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
        }

        [Fact]
        public void UploadIcon_Works()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard("1");

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);
            _clientHelper.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void MoveIcon_Works()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard("2");

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);

            int moveByX = 250;
            int moveByY = 250;
            _clientHelper.MoveIcon(StartPosMiddleX, StartPosMiddleY, moveByX, moveByY);
            _clientHelper.EnsureItemWasMoved(StartPosMiddleX, StartPosMiddleY, moveByX, moveByY);
        }

        [Fact]
        public void Other_Clients_See_AddedMapIcons()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard("3");

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);


            _clientHelper2.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper2.ConnectToBoard("3");
            Thread.Sleep(1000);
            _clientHelper2.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void Other_Clients_See_MapIconsMovement()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard("4");

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);


            _clientHelper2.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper2.ConnectToBoard("4");

            int moveByX = 250;
            int moveByY = 250;
            _clientHelper.MoveIcon(StartPosMiddleX, StartPosMiddleY, moveByX, moveByY);

            _clientHelper2.EnsureItemWasMoved(StartPosMiddleX, StartPosMiddleY, moveByX, moveByY);
        }
    }
}
