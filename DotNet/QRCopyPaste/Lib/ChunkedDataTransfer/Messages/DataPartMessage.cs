namespace ChunkedDataTransfer
{
    internal class DataPartMessage
    {
        public string MsgIntegrity { get; set; }
        public int PartID { get; set; }
        public string Data { get; set; }
        public string DataHash { get; set; }
        public string ObjectID { get; set; }
    }
}
