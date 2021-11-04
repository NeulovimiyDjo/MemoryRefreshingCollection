using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace QRCopyPaste.Mobile
{
    public class BarcodePage : ContentPage
    {
        private ZXingBarcodeImageView barcodeView;

        public BarcodePage()
        {
            barcodeView = new ZXingBarcodeImageView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BarcodeFormat = ZXing.BarcodeFormat.QR_CODE,
            };

            barcodeView.BarcodeOptions.Width = 300;
            barcodeView.BarcodeOptions.Height = 300;
            barcodeView.BarcodeOptions.Margin = 10;

            barcodeView.BarcodeValue = null;

            var stackLayout = new StackLayout();
            stackLayout.Children.Add(barcodeView);
            this.Content = stackLayout;
        }

        public void SetBarcodeValue(string text)
        {
            barcodeView.BarcodeValue = text;
        }

        public void SetBarcodeViewVisibility(bool isVisible)
        {
            barcodeView.IsVisible = isVisible;
        }
    }
}
