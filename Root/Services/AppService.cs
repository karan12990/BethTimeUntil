using System.Net;
using Newtonsoft.Json;
using Root.Interfaces;
using Root.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Root.Services
{
    public class AppService : IAppService
    {
        public async Task<MainResponse> AuthenticateUser(LoginModel loginModel)
        {
            var returnResponse = new MainResponse();

            try
            {
                var envelope = new GetStoreUserListRequest.Envelope
                {
                    Body = new GetStoreUserListRequest.Body
                    {
                        GetStoreUserList = new GetStoreUserListRequest.GetStoreUserList
                        {
                            userCode = loginModel.UserName,
                            xmlFile = new GetStoreUserListRequest.XmlFile
                            {
                                StoreUser = new GetStoreUserListRequest.StoreUser
                                {
                                    UserID = "[string]",
                                    StoreCode = "[string]",
                                    StoreName = "[string]",
                                    Default = false
                                }
                            }
                        }
                    }
                };

                // Serialize the Envelope object to XML
                string xmlString;
                XmlSerializer serializer = new XmlSerializer(typeof(GetStoreUserListRequest.Envelope));
                using (StringWriter textWriter = new Utf8StringWriter())
                {
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        OmitXmlDeclaration = true,
                        Indent = true
                    };
                    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                    {
                        serializer.Serialize(xmlWriter, envelope);
                    }
                    xmlString = textWriter.ToString();
                }

                // Make HTTP request
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30); // Set a timeout

                    var request = new HttpRequestMessage(HttpMethod.Post, Setting.BaseUrl);
                    request.Headers.Add("SOAPAction", "urn:microsoft-dynamics-schemas/codeunit/PDA:GetStoreUserList");
                    // Encode username and password in Base64
                    var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{loginModel.UserName}:{loginModel.Password}"));

                    // Add Basic Authentication header
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                    // Set the content type
                    request.Content = new StringContent(xmlString, Encoding.UTF8, "text/xml");


                    // Send the request
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and process the response
                        var responseXml = await response.Content.ReadAsStringAsync();

                        var serializerResponse = new XmlSerializer(typeof(GetStoreUserListResponse));

                        using TextReader reader = new StringReader(responseXml);
                        var getStoreUserListResponse = (GetStoreUserListResponse)serializerResponse.Deserialize(reader);
                        returnResponse.StoreUsers = getStoreUserListResponse.Body.GetStoreUserListResult.XmlFile.StoreUsers.ToList();
                        returnResponse.IsSuccess = true;

                        return returnResponse;
                    }

                    // Handle different response status codes
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            returnResponse.ErrorMessage = "Please check username and password";
                            break;
                        default:
                            returnResponse.ErrorMessage = $"Error: {response.ReasonPhrase}";
                            break;
                    }
                    returnResponse.IsSuccess = false;
                    return returnResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // Better to use logging library here
                returnResponse.ErrorMessage = $"Exception: {ex.Message}";
                returnResponse.IsSuccess = false;
                return returnResponse;
            }
        }
        
        // Custom StringWriter to specify UTF-8 encoding
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
