using System.Xml.Serialization;

namespace Root.Models;

public class GetStoreUserListRequest
{
    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope
    {
        [XmlElement("Body")]
        public Body Body { get; set; }
    }

    public class Body
    {
        [XmlElement("GetStoreUserList", Namespace = "urn:microsoft-dynamics-schemas/codeunit/PDA")]
        public GetStoreUserList GetStoreUserList { get; set; }
    }

    public class GetStoreUserList
    {
        public string userCode { get; set; }
        public XmlFile xmlFile { get; set; }
    }

    public class XmlFile
    {
        public StoreUser StoreUser { get; set; }
    }

    public class StoreUser
    {
        public string UserID { get; set; }
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public bool Default { get; set; }
    }

}
