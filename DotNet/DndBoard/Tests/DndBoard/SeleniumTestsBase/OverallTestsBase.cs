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
            _clientHelper.ConnectToBoard(nameof(UploadIcon_Works));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");

            _clientHelper.SetCurrentCanvasId("IconsModelsDivCanvas");
            _clientHelper.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void AddIcon_Works()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(AddIcon_Works));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);

            _clientHelper.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void MoveIcon_Works()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(MoveIcon_Works));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);

            int moveByX = 250;
            int moveByY = 250;
            _clientHelper.MoveIcon(StartPosMiddleX, StartPosMiddleY, moveByX, moveByY);

            _clientHelper.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper.EnsureItemWasMoved(StartPosMiddleX, StartPosMiddleY, moveByX, moveByY);
        }

        [Fact]
        public void DeleteModel_Works()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(DeleteModel_Works));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.SetCurrentCanvasId("IconsModelsDivCanvas");
            _clientHelper.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);

            _clientHelper.DeleteIconFromCanvas("IconsModelsDivCanvas", StartPosMiddleX, StartPosMiddleY);
            _clientHelper.SetCurrentCanvasId("IconsModelsDivCanvas");
            _clientHelper.EnsureIconDeletedFromMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void DeleteInstance_Works()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(DeleteInstance_Works));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);
            _clientHelper.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);

            _clientHelper.DeleteIconFromCanvas("IconsInstancesCanvasDiv", StartPosMiddleX, StartPosMiddleY);
            _clientHelper.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper.EnsureIconDeletedFromMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void AddingIconInstance_After_DeleteModel_Works()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(AddingIconInstance_After_DeleteModel_Works));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.DeleteIconFromCanvas("IconsModelsDivCanvas", StartPosMiddleX, StartPosMiddleY);
            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");

            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);
            _clientHelper.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);
        }

        //[Fact]
        //public void AddingIconInstance_After_DeleteModel_WithActiveInstances_Works()
        //{
        //    _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
        //    _clientHelper.ConnectToBoard(nameof(AddingIconInstance_After_DeleteModel_WithActiveInstances_Works));

        //    _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
        //    _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);
        //    _clientHelper.DeleteIconFromCanvas("IconsModelsDivCanvas", StartPosMiddleX, StartPosMiddleY);
        //    _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");

        //    _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);
        //    _clientHelper.SetCurrentCanvasId("IconsInstancesCanvasDiv");
        //    _clientHelper.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);
        //}

        [Fact]
        public void Other_Clients_See_UploadedModels()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(Other_Clients_See_UploadedModels));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");


            _clientHelper2.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper2.ConnectToBoard(nameof(Other_Clients_See_UploadedModels));
            Thread.Sleep(1000);

            _clientHelper2.SetCurrentCanvasId("IconsModelsDivCanvas");
            _clientHelper2.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);
        }


        [Fact]
        public void Other_Clients_See_AddedIconsInstances()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(Other_Clients_See_AddedIconsInstances));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);


            _clientHelper2.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper2.ConnectToBoard(nameof(Other_Clients_See_AddedIconsInstances));
            Thread.Sleep(1000);

            _clientHelper2.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper2.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void Other_Clients_See_IconsModelsDeletions()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(Other_Clients_See_IconsModelsDeletions));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");


            _clientHelper2.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper2.ConnectToBoard(nameof(Other_Clients_See_IconsModelsDeletions));
            Thread.Sleep(1000);

            _clientHelper2.SetCurrentCanvasId("IconsModelsDivCanvas");
            _clientHelper2.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);

            _clientHelper.DeleteIconFromCanvas("IconsModelsDivCanvas", StartPosMiddleX, StartPosMiddleY);

            _clientHelper2.SetCurrentCanvasId("IconsModelsDivCanvas");
            _clientHelper2.EnsureIconDeletedFromMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void Other_Clients_See_IconsInstancesDeletions()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(Other_Clients_See_IconsInstancesDeletions));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);


            _clientHelper2.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper2.ConnectToBoard(nameof(Other_Clients_See_IconsInstancesDeletions));
            Thread.Sleep(1000);

            _clientHelper2.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper2.EnsureIconAddedToMap(StartPosMiddleX, StartPosMiddleY);

            _clientHelper.DeleteIconFromCanvas("IconsInstancesCanvasDiv", StartPosMiddleX, StartPosMiddleY);

            _clientHelper2.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper2.EnsureIconDeletedFromMap(StartPosMiddleX, StartPosMiddleY);
        }

        [Fact]
        public void Other_Clients_See_IconsInstancesMovement()
        {
            _clientHelper.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper.ConnectToBoard(nameof(Other_Clients_See_IconsInstancesMovement));

            _clientHelper.UploadIcon($"{_dataDir}/blueSquare.png");
            _clientHelper.AddIconToMap(StartPosMiddleX, StartPosMiddleY);


            _clientHelper2.OpenUrl($"{_siteBaseAddress}/board");
            _clientHelper2.ConnectToBoard(nameof(Other_Clients_See_IconsInstancesMovement));

            int moveByX = 250;
            int moveByY = 250;
            _clientHelper.MoveIcon(StartPosMiddleX, StartPosMiddleY, moveByX, moveByY);

            _clientHelper2.SetCurrentCanvasId("IconsInstancesCanvasDiv");
            _clientHelper2.EnsureItemWasMoved(StartPosMiddleX, StartPosMiddleY, moveByX, moveByY);
        }
    }
}
