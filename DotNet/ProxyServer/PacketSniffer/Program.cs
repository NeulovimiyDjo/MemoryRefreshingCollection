using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PacketSniffer
{
    class Program
    {

        //using addr_t = uint32_t;
        //using port_t = uint16_t;

        //struct ether_header_t
        //{
        //    uint8_t dst_addr[6];
        //    uint8_t src_addr[6];
        //    uint16_t llc_len;
        //};

        //struct ip_header_t
        //{
        //    uint8_t ver_ihl;  // 4 bits version and 4 bits internet header length
        //    uint8_t tos;
        //    uint16_t total_length;
        //    uint16_t id;
        //    uint16_t flags_fo; // 3 bits flags and 13 bits fragment-offset
        //    uint8_t ttl;
        //    uint8_t protocol;
        //    uint16_t checksum;
        //    addr_t src_addr;
        //    addr_t dst_addr;

        //    uint8_t ihl() const;
        //    size_t size() const;
        //};

        //class udp_header_t
        //{
        //    public:
        //port_t src_port;
        //    port_t dst_port;
        //    uint16_t length;
        //    uint16_t checksum;
        //};


        public class IPHeader
        {
            //IP Header fields 
            private byte byVersionAndHeaderLength; // Eight bits for version and header 
                                                   // length 
            private byte byDifferentiatedServices; // Eight bits for differentiated 
                                                   // services
            private ushort usTotalLength;          // Sixteen bits for total length 
            private ushort usIdentification;       // Sixteen bits for identification
            private ushort usFlagsAndOffset;       // Eight bits for flags and frag. 
                                                   // offset 
            private byte byTTL;                    // Eight bits for TTL (Time To Live) 
            private byte byProtocol;               // Eight bits for the underlying 
                                                   // protocol 
            private short sChecksum;               // Sixteen bits for checksum of the 
                                                   //  header 
            private uint uiSourceIPAddress;        // Thirty two bit source IP Address 
            private uint uiDestinationIPAddress;   // Thirty two bit destination IP Address 

            //End IP Header fields   
            private byte byHeaderLength;             //Header length 
            public byte[] byIPData = new byte[65536]; //Data carried by the datagram
            public int dataLen;
            public IPAddress srcIP;
            public IPAddress dstIP;
            public string protocol;
            public IPHeader(byte[] byBuffer, int nReceived)
            {
                try
                {
                    //Create MemoryStream out of the received bytes
                    MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);

                    //Next we create a BinaryReader out of the MemoryStream
                    BinaryReader binaryReader = new BinaryReader(memoryStream);

                    //The first eight bits of the IP header contain the version and
                    //header length so we read them
                    byVersionAndHeaderLength = binaryReader.ReadByte();

                    //The next eight bits contain the Differentiated services
                    byDifferentiatedServices = binaryReader.ReadByte();

                    //Next eight bits hold the total length of the datagram
                    usTotalLength =
                             (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    //Next sixteen have the identification bytes
                    usIdentification =
                              (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    //Next sixteen bits contain the flags and fragmentation offset
                    usFlagsAndOffset =
                              (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    //Next eight bits have the TTL value
                    byTTL = binaryReader.ReadByte();

                    //Next eight represent the protocol encapsulated in the datagram
                    byProtocol = binaryReader.ReadByte();
                    switch (byProtocol)
                    {
                        case 6:
                            protocol = "TCP";
                            break;
                        default:
                            protocol = byProtocol.ToString();
                            break;
                    }

                    //Next sixteen bits contain the checksum of the header
                    sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    //Next thirty two bits have the source IP address
                    uiSourceIPAddress = (uint)(binaryReader.ReadInt32());
                    srcIP = new IPAddress(uiSourceIPAddress);

                    //Next thirty two hold the destination IP address
                    uiDestinationIPAddress = (uint)(binaryReader.ReadInt32());
                    dstIP = new IPAddress(uiDestinationIPAddress);

                    //Now we calculate the header length
                    byHeaderLength = byVersionAndHeaderLength;

                    //The last four bits of the version and header length field contain the
                    //header length, we perform some simple binary arithmetic operations to
                    //extract them
                    byHeaderLength <<= 4;
                    byHeaderLength >>= 4;

                    //Multiply by four to get the exact header length
                    byHeaderLength *= 4;

                    //Copy the data carried by the datagram into another array so that
                    //according to the protocol being carried in the IP datagram
                    dataLen = nReceived - byHeaderLength;
                    Array.Copy(byBuffer,
                               byHeaderLength, //start copying from the end of the header
                               byIPData, 0, dataLen);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public class TCPHeader
        {
            public UInt16 src_port;
            public UInt16 dst_port;
            public UInt32 seq;
            public UInt32 ack;
            public byte data_offset;  // 4 bits
            public byte flags;
            public UInt16 window_size;
            public UInt16 checksum;
            public UInt16 urgent_p;


            public byte[] data = new byte[65536];
            public int dataSize;

            public TCPHeader(byte[] buffer, int size)
            {
                MemoryStream memoryStream = new MemoryStream(buffer, 0, size);
                BinaryReader binaryReader = new BinaryReader(memoryStream);


                src_port = binaryReader.ReadUInt16();
                dst_port = binaryReader.ReadUInt16();
                seq = binaryReader.ReadUInt32();
                ack = binaryReader.ReadUInt32();
                data_offset = (byte)(binaryReader.ReadByte() >> 4);
                flags = binaryReader.ReadByte();
                window_size = binaryReader.ReadUInt16();
                checksum = binaryReader.ReadUInt16();
                urgent_p = binaryReader.ReadUInt16();

                int dataOffsetInBytes = 4 * data_offset;
                dataSize = size - dataOffsetInBytes;
                Array.Copy(buffer, dataOffsetInBytes, data, 0, dataSize);
            }
        }


        static async Task Main(string[] args)
        {
            var buffer = new byte[65536];
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            sock.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.7"), 0));
            //sock.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
            byte[] trueBytes = new byte[] { 1, 0, 0, 0 };
            byte[] outBytes = new byte[] { 1, 0, 0, 0 };
            sock.IOControl(IOControlCode.ReceiveAll, trueBytes, outBytes);


            while (true)
            {
                int size = await sock.ReceiveAsync(buffer, SocketFlags.None);

                Console.WriteLine($"{size} bytes recieved");
                File.AppendAllText("log.txt", $"{DateTime.Now.ToLongTimeString()}: {size} bytes recieved");

                var iPHeader = new IPHeader(buffer, size);
                File.AppendAllText("log.txt", "\n");
                File.AppendAllText("log.txt", $"\nsrcIP: {iPHeader.srcIP}, dstIP: {iPHeader.dstIP}, protocol: {iPHeader.protocol}");
                if (iPHeader.protocol == "TCP")
                {
                    var tcpHeader = new TCPHeader(iPHeader.byIPData, iPHeader.dataLen);
                    File.AppendAllText("log.txt", $"\nsrcPort: {tcpHeader.src_port}, dstPort: {tcpHeader.dst_port}");
                    File.AppendAllText("log.txt", $"\nData: {Encoding.ASCII.GetString(tcpHeader.data, 0, tcpHeader.dataSize)}");
                }
                else
                {
                    File.AppendAllText("log.txt", $"\nData: {Encoding.ASCII.GetString(iPHeader.byIPData, 0, iPHeader.dataLen)}");
                }
                File.AppendAllText("log.txt", "\n\n");


                //using (var fs = File.Open("log.txt", FileMode.Append, FileAccess.Write, FileShare.Read))
                //{
                //    await fs.WriteAsync(tcpHeader.data, 0, tcpHeader.dataSize);
                //}
            }
        }
    }
}
