using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using Xunit;
using static DndBoard.SeleniumTestsBase.Helpers.PixelHelper;

namespace DndBoard.SeleniumTestsBase.Helpers
{
    public class ClientHelper
    {
        private readonly ChromeDriver _driver;
        private readonly CanvasHelper _canvasHelper;
        private int _iconSize = -1;
        public int IconSize
        {
            get
            {
                if (_iconSize <= 0)
                    throw new Exception($"{nameof(ClientHelper)}.{nameof(IconSize)} is not set or invalid.");
                return _iconSize;
            }
            set => _iconSize = value;
        }

        public ClientHelper(ChromeDriver driver)
        {
            _driver = driver;
            _canvasHelper = new CanvasHelper(_driver);
        }


        public void SetCurrentCanvasId(string divCanvasId)
        {
            _canvasHelper.DivCanvasId = divCanvasId;
        }

        public void OpenUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        public void ConnectToBoard(string boardId)
        {
            IWebElement input = _driver.FindElementById("boardId");
            input.SendKeys(boardId);
            IWebElement button = _driver.FindElementByXPath("//*[contains(text(), 'Connect')]");
            button.Click();
        }

        public void UploadIcon(string filePath)
        {
            IWebElement files = _driver.FindElementByXPath("//*[@type='file']");
            files.SendKeys(filePath);
            Thread.Sleep(500);
        }

        public void AddIconToMap(int clickToX, int clickToY)
        {
            IWebElement iconsModelsCanvas = _driver.FindElementByCssSelector("#IconsModelsDivCanvas > canvas");
            Actions actions = _canvasHelper.MoveToElemLeftTopCorner(iconsModelsCanvas);
            actions.MoveByOffset(clickToX, clickToY).Perform();
            actions.Click().Perform();
            Thread.Sleep(500);
        }

        public void DeleteIconFromCanvas(string divCanvasId, int clickToX, int clickToY)
        {
            IWebElement iconsModelsCanvas = _driver.FindElementByCssSelector($"#{divCanvasId} > canvas");
            Actions actions = _canvasHelper.MoveToElemLeftTopCorner(iconsModelsCanvas);
            actions.MoveByOffset(clickToX, clickToY).Perform();
            actions.ContextClick().Perform();
            Thread.Sleep(500);
        }

        public void MoveIcon(int nowX, int nowY, int moveX, int moveY)
        {
            IWebElement iconsInstancesCanvas = _driver.FindElementByCssSelector("#IconsInstancesCanvasDiv > canvas");

            Actions actions = _canvasHelper.MoveToElemLeftTopCorner(iconsInstancesCanvas);

            actions.MoveByOffset(nowX, nowY).Perform();
            actions.ClickAndHold().Perform();
            Thread.Sleep(100);

            // Moving top left corner => clicking icon middle already moves it by size/2.
            int realMoveX = moveX - IconSize / 2;
            int realMoveY = moveY - IconSize / 2;
            actions.MoveByOffset(realMoveX, realMoveY).Perform();
            Thread.Sleep(100);

            actions.Release().Perform();
            Thread.Sleep(500);
        }

        public void EnsureIconAddedToMap(int nowX, int nowY)
        {
            EnsureItemUnderMouse(nowX, nowY);
            EnsureItemNotUnderMouse(nowX + IconSize, nowY + IconSize);
        }

        public void EnsureItemWasMoved(int wasX, int wasY, int movedByX, int movedByY)
        {
            EnsureItemNotUnderMouse(wasX, wasY);
            EnsureItemUnderMouse(wasX + movedByX, wasY + movedByY);
        }

        public void EnsureIconDeletedFromMap(int wasX, int wasY)
        {
            EnsureItemNotUnderMouse(wasX, wasY);
        }

        private void EnsureItemUnderMouse(int x, int y)
        {
            Dictionary<string, int> pixel = _canvasHelper.GetPixel(x, y);
            Assert.True(IsBluePixel(pixel));
        }

        private void EnsureItemNotUnderMouse(int x, int y)
        {
            Dictionary<string, int> pixel = _canvasHelper.GetPixel(x, y);
            Assert.False(IsBluePixel(pixel));
        }
    }
}
