using System.Collections.Generic;

namespace DndBoard.SeleniumTestsBase.Helpers
{
    public static class PixelHelper
    {
        public static bool IsBluePixel(Dictionary<string, int> pixel)
        {
            int r = pixel["0"];
            int g = pixel["1"];
            int b = pixel["2"];
            bool isBlue = true
                && r > 30 && r < 100
                && g > 30 && g < 100
                && b > 170 && b < 230
                ;
            return isBlue;
        }
    }
}
