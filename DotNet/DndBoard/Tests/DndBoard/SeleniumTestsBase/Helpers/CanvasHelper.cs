using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace DndBoard.SeleniumTestsBase.Helpers
{
    public class CanvasHelper
    {
        private string _divCanvasId;
        public string DivCanvasId {
            get
            {
                if (_divCanvasId is null)
                    throw new Exception($"{nameof(DivCanvasId)} is null in {nameof(CanvasHelper)}");
                return _divCanvasId;
            }
            set => _divCanvasId = value;
        }
        private readonly ChromeDriver _driver;

        public CanvasHelper(ChromeDriver driver)
        {
            _driver = driver;
        }


        public Actions MoveToElemLeftTopCorner(IWebElement mapCanvas)
        {
            _driver.ExecuteScript("arguments[0].scrollIntoView();", mapCanvas);
            Actions actions = new Actions(_driver);
            actions.MoveToElement(mapCanvas, 1, 1).Perform();
            return actions;
        }

        public Dictionary<string, int> GetPixel(int x, int y)
        {
            string script = @$"
var mapCanvas = document.getElementById('{DivCanvasId}').getElementsByTagName('canvas')[0];
var ctx = mapCanvas.getContext('2d');
var pixelData = ctx.getImageData(arguments[0], arguments[1], 1, 1).data;
return JSON.stringify(pixelData);
";

            string pixelData = (string)_driver.ExecuteScript(script, x, y);
            Dictionary<string, int> pixel = JsonSerializer.Deserialize<Dictionary<string, int>>(pixelData);
            return pixel;
        }
    }
}
