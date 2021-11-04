namespace ChunkedDataTransfer
{
    public delegate void StringDataReceivedEventHandler(string data);
    public delegate void ByteDataReceivedEventHandler(byte[] data);
    public delegate void NotificationEventHandler(string msg);
    public delegate void ReceivingStartedEventHandler(string objectID);
    public delegate void ReceivingStoppedEventHandler(string objectID);
    public delegate void ChunkReceivedEventHandler(string objectID);
    public delegate void ProgressChangedEventHandler(int progress);
}
