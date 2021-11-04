# QRCopyPaste
QRCopyPaste is an app to transfer clipboard text or files through a sequence of QR codes on the screen.
It's useful for example for copying data between devices by displaying QR codes on one device and scanning on the other.


# How it works
Once send-button is pressed data(clipboard text or file) is getting gzipped in memory with max compression level.
Then gzipped data is transformed to base64 string and then it is split to chunks of QRSenderSettings.ChunkSize symbols.

Each chunk has an id, hashcode of data-payload and some additional meta information.
First special chunk contains no data, but rather the total amount of chunks,
total data hashcode for later verification on receiving end and some other additional meta information.
Then all of these chunks get displayed on the screen one after another with QRSenderSettings.SendDelayMilliseconds delay.

Receiving-machine that is scanning for this first specail chunk every QRSenderSettings.ScanPeriodForSettingsMessageMilliseconds
then detects the first chunk (sometimes not, need to restart the sending process in this case),
gets number of chunks for the whole data and keeps scanning for other chunks and saving successfully decoded chunks in-memory.

Once all chunks are received or QR sender stopped displaying QR codes(or receiver failed to decody any)
for longer than QRSenderSettings.MaxMillisecondsToContinueSinceLastSuccessfulQRRead
or time-outed on QRSenderSettings.SendDelayMilliseconds * numberOfChunks * 3 / 2
saving stops and scan for first-specail chunk begins again.

If all chunks were successfully transferred data is either saved in clipboard (if was sent from clipboard) or offers file save dialog if a file was sent.
If unsuccessful the sending process can be repeated with the same data (with full-new transfer or just unsuccessfully received chunks by their ID)
until all chunks get successfully received (successfully transferred chunks stay in memory unless reset button is used).
Unsuccessfully transferred chunk-ids are displayed in error message.


# Transfer speed
Is pretty crap. Haven't seen more than 200kb per minute on binary files, (may be significantly more on text data due to more efficient gzipping.)
There's room for tweaking settings of send delay and chunk size per one QR code, but it leads to more errors and more time on repeating transfers.
Bottleneck is the max frequency of scans which is now about 10 per second.
Therei's also room for rewriting code for displaying and scanning more than one QR code on screen simultaneously.


# How to use
1) Run application on both machines.
2) Use StartScanning button on a machine that needs to receive data.
This receiving machine has to "see" the screen of the other machine that is going to send data.
3) Use SendClipboardText or SendFile buttons on a machine that has the data.
Application will start to display QR codes containing chunks of that data.
Receiving machine will detect and decode them and assemble full data from chunks once all of them are transferred.

You can use StopSending button on sender-machine to stop displaying QR codes if needed.
You can use ClearCache button on receiver-machine to delete all cached data from all successfully received QR codes if needed.
You can use ResendLast button on sender-machine to resend last item (clipboard text or file) if needed.
In texbox below you can enter data part id's delimited by spaces that weren't received due to transfer errors,
if there is at least one ID then only these parts will be sent instead of full-new transfer attempt.

Upper-green progress bar is the percentage of data-parts that sender-machine has sent(displayed).
Lower-red progress bar is the percentage that receiver-machine has successfully detected and decoded.


# How to build/publish
Publish bat-scripts examples for desktop are in the root of repository.
Otherwise it's a typical simple Visual studio solution.


# Author
Johnny Borov JohnnyBorov@gmail.com


# Licence
MIT License. See [LICENSE](LICENSE) file.