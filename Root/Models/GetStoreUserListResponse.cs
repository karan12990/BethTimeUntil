using System.Xml.Serialization;

namespace Root.Models;

[XmlRoot(ElementName = "StoreUser", Namespace = "urn:microsoft-dynamics-nav/xmlports/StoreUser")]
public class StoreUser
{
    [XmlElement(ElementName = "UserID")]
    public string UserID { get; set; }

    [XmlElement(ElementName = "StoreCode")]
    public string StoreCode { get; set; }

    [XmlElement(ElementName = "StoreName")]
    public string StoreName { get; set; }

    [XmlElement(ElementName = "Default")]
    public string Default { get; set; }
}

[XmlRoot(ElementName = "xmlFile")]
public class XmlFile
{
    [XmlElement(ElementName = "StoreUser", Namespace = "urn:microsoft-dynamics-nav/xmlports/StoreUser")]
    public StoreUser[] StoreUsers { get; set; }
}

[XmlRoot(ElementName = "GetStoreUserList_Result", Namespace = "urn:microsoft-dynamics-schemas/codeunit/PDA")]
public class GetStoreUserListResult
{
    [XmlElement(ElementName = "return_value")]
    public string ReturnValue { get; set; }

    [XmlElement(ElementName = "xmlFile")]
    public XmlFile XmlFile { get; set; }
}

[XmlRoot(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class Body
{
    [XmlElement(ElementName = "GetStoreUserList_Result", Namespace = "urn:microsoft-dynamics-schemas/codeunit/PDA")]
    public GetStoreUserListResult GetStoreUserListResult { get; set; }
}

[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class GetStoreUserListResponse
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public Body Body { get; set; }
}