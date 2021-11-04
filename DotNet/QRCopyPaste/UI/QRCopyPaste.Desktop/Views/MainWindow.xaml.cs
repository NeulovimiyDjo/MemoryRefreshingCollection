using ChunkedDataTransfer;
using Microsoft.Win32;
using QRCopyPaste.Desktop.QRLogic;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace QRCopyPaste
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private int _scanCycle;
        public int ScanCycle
        {
            get => this._scanCycle;
            set
            {
                if (this._scanCycle != value)
                {
                    this._scanCycle = value;
                    OnPropertyChanged(nameof(ScanCycle));
                }
            }
        }

        private int _receiverProgress;
        public int ReceiverProgress
        {
            get => this._receiverProgress;
            set
            {
                if (this._receiverProgress != value)
                {
                    this._receiverProgress = value;
                    OnPropertyChanged(nameof(ReceiverProgress));
                }
            }
        }

        private int _senderProgress;
        public int SenderProgress
        {
            get => this._senderProgress;
            set
            {
                if (this._senderProgress != value)
                {
                    this._senderProgress = value;
                    OnPropertyChanged(nameof(SenderProgress));
                }
            }
        }

        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get => this._imageSource;
            set
            {
                if (this._imageSource != value)
                {
                    this._imageSource = value;
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly ChunkedDataSender chunkedDataSender;
        private readonly ChunkedDataReceiver chunkedDataReceiver;
        private readonly TimeoutWatcher timeoutWatcher;
        private string currentlyReceivingObjectID;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            var dataSender = new DesktopQRDataSender();
            dataSender.OnQRImageChanged += imageSource => this.ImageSource = imageSource;
            this.chunkedDataSender = new ChunkedDataSender(dataSender);
            this.chunkedDataSender.OnProgressChanged += progress => this.SenderProgress = progress;
            this.chunkedDataSender.ChunkSize = QRSenderSettings.ChunkSize;

            this.chunkedDataReceiver = new ChunkedDataReceiver();
            this.chunkedDataReceiver.OnProgressChanged += progress => this.ReceiverProgress = progress;
            this.chunkedDataReceiver.OnReceivingStarted += objectID =>
            {
                this.ScanCycle++;
                this.currentlyReceivingObjectID = objectID;
                this.timeoutWatcher.Start();
            };
            this.chunkedDataReceiver.OnReceivingStopped += objectID =>
            {
                this.timeoutWatcher.Stop();
                this.currentlyReceivingObjectID = null;
            };
            this.timeoutWatcher = new TimeoutWatcher(() =>
            {
                this.chunkedDataReceiver.StopReceiving(this.currentlyReceivingObjectID);
            });
            this.chunkedDataReceiver.OnChunkReceived += objectID =>
            {
                this.timeoutWatcher.Restart();
            };
            this.chunkedDataReceiver.OnNotification += msg => this.ShowMessage(msg);
        }

        #endregion

        #region Methods

        private void ShowMessage(string msg)
        {
            Dispatcher.Invoke(() => MsgBoxWindow.Show(this, msg, "", MsgBoxWindow.MessageBoxButtons.Ok));
        }


        private void StartScanningBtn_Click(object sender, RoutedEventArgs e)
        {
            this.chunkedDataReceiver.OnStringDataReceived += HandleReceivedData;
            this.chunkedDataReceiver.OnByteDataReceived += HandleReceivedData;
            this.chunkedDataReceiver.StartReceiving();

            var qrScreenScanner = new QRScreenScanner();
            qrScreenScanner.OnQRTextDataReceived += (data) => this.chunkedDataReceiver.ProcessChunk(data);
            qrScreenScanner.OnError += (errorMsg) => this.ShowMessage($"Error: {errorMsg}");

            if (qrScreenScanner.StartScanning())
                this.ShowMessage("Scanning started.");
            else
                this.ShowMessage("Scanning is already running.");
        }


        private void HandleReceivedData(object receivedData)
        {
            if (receivedData.GetType() == typeof(string))
            {
                Dispatcher.Invoke(() => Clipboard.SetText((string)receivedData));
                this.ShowMessage($"Data copied to clipboard.\n\n{(string)receivedData}");
            }
            else if (receivedData.GetType() == typeof(byte[]))
            {
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, (byte[])receivedData);
                }
            }
            else
            {
                throw new Exception($"Unsupported data type {receivedData.GetType()} in {nameof(HandleReceivedData)}.");
            }
        }


        private async void SendClipboardTextBtn_Click(object sender, RoutedEventArgs e)
        {
            var stringData = Clipboard.GetData(DataFormats.Text);
            if (string.IsNullOrEmpty((string)stringData))
            {
                this.ShowMessage($"There is no text in clipboard right now.");
                return;
            }


            try
            {
                await this.chunkedDataSender.SendAsync((string)stringData);
            }
            catch (Exception ex)
            {
                this.ShowMessage($"Error while sending clipboard text: {ex.Message}");
            }
        }


        private async void SendFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var binaryData = File.ReadAllBytes(openFileDialog.FileName);

                try
                {
                    await this.chunkedDataSender.SendAsync(binaryData);
                }
                catch (Exception ex)
                {
                    this.ShowMessage($"Error while sending file: {ex.Message}");
                }
            }
        }


        private void StopSendingBtn_Click(object sender, RoutedEventArgs e)
        {
            this.chunkedDataSender.StopSending();
        }


        private void ClearCacheBtn_Click(object sender, RoutedEventArgs e)
        {
            this.chunkedDataReceiver.ClearCache();
        }


        private async void ResendLastBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string idsStr = ResendIDsTextBox.Text;
                int[] ids =
                    string.IsNullOrEmpty(idsStr)
                    ? null
                    : idsStr.Split(" ").Select(idStr => int.Parse(idStr)).ToArray();

                await this.chunkedDataSender.ResendLastAsync(ids);
            }
            catch (Exception ex)
            {
                this.ShowMessage($"Error while resending: {ex.Message}");
            }
        }

        #endregion
    }
}
