using System.Windows.Media;

namespace QRCopyPaste
{
    public delegate void QRTextDataReceivedEventHandler(string data);
    public delegate void ErrorEventHandler(string msg);
    public delegate void QRImageChangedEventHandler(ImageSource imageSource);
}
