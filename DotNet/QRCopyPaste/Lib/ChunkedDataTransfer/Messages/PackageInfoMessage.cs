namespace ChunkedDataTransfer
{
    internal class PackageInfoMessage
    {
        public string MsgIntegrity { get; set; }
        public int NumberOfParts { get; set; }
        public string DataType { get; set; }
        public string DataHash { get; set; }
        public string ObjectID { get; set; }
    }
}
