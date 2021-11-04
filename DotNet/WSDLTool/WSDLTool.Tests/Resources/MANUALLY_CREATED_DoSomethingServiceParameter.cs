using System.Runtime.Serialization;

namespace SomeProject.SomeMethod
{
    [DataContract(Name = "DoSomethingServiceRequest", Namespace = "http://testsrv")]
    public class DoSomethingServiceRequest
    {
        [DataMember(Name = "DATA_NODE", IsRequired = true, Order = 0)]
        public DATA_NODE DATA_NODE;
    }

    [DataContract(Name = "DATA_NODE", Namespace = "http://testsrv")]
    public class DATA_NODE
    {
        [DataMember(Name = "FIELD1", IsRequired = true, Order = 0)]
        public string FIELD1;

        [DataMember(Name = "TEST", IsRequired = true, Order = 1)]
        public string TEST;

        [DataMember(Name = "SomeList", IsRequired = true, Order = 2)]
        public SomeListInfo SomeList;
    }

    [DataContract(Name = "SomeList", Namespace = "http://testsrv")]
    public class SomeListInfo
    {
        [DataMember(Name = "FLD_1", IsRequired = true, Order = 0)]
        public string FLD_1;

        [DataMember(Name = "DATA22", IsRequired = true, Order = 1)]
        public string DATA22;

        [DataMember(Name = "POSITIONS", IsRequired = true, Order = 2)]
        public POSITION[] POSITIONS;
    }

    [DataContract(Name = "POSITION", Namespace = "http://testsrv")]
    public class POSITION
    {
        [DataMember(Name = "FIELDO33", IsRequired = true, Order = 0)]
        public string FIELDO33;

        [DataMember(Name = "FIELDO44", IsRequired = true, Order = 1)]
        public string FIELDO44;

        [DataMember(Name = "XXEUU", IsRequired = true, Order = 2)]
        public string XXEUU;

        [DataMember(Name = "POSITIONS_X", IsRequired = false, Order = 3)]
        public POSITION_X[] POSITIONS_X;

        [DataMember(Name = "POSITIONS_Y", IsRequired = true, Order = 4)]
        public POSITION_Y[] POSITIONS_Y;
    }

    [DataContract(Name = "POSITION_X", Namespace = "http://testsrv")]
    public class POSITION_X
    {
        [DataMember(Name = "PVAL1", IsRequired = true, Order = 0)]
        public string PVAL1;

        [DataMember(Name = "PVAL2", IsRequired = true, Order = 1)]
        public string PVAL2;

        [DataMember(Name = "PVAL3", IsRequired = true, Order = 2)]
        public string PVAL3;

        [DataMember(Name = "PVAL4", IsRequired = true, Order = 3)]
        public string PVAL4;
    }

    [DataContract(Name = "POSITION_Y", Namespace = "http://testsrv")]
    public class POSITION_Y
    {
        [DataMember(Name = "ZZNAME", IsRequired = true, Order = 0)]
        public string ZZNAME;

        [DataMember(Name = "FileList", IsRequired = true, Order = 1)]
        public File[] FileList;
    }

    [DataContract(Name = "File", Namespace = "http://testsrv")]
    public class File
    {
        [DataMember(Name = "FILE_ID", IsRequired = true, Order = 0)]
        public string FILE_ID;

        [DataMember(Name = "FILE_NAME", IsRequired = true, Order = 1)]
        public string FILE_NAME;

        [DataMember(Name = "FILE_EXT", IsRequired = true, Order = 2)]
        public string FILE_EXT;
    }
}
