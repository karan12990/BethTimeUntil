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
                    var request = new HttpRequestMessage(HttpMethod.Post, Setting.BaseUrl);
                    request.Headers.Add("SOAPAction", "urn:microsoft-dynamics-schemas/codeunit/PDA:GetStoreUserList");

                    // Encode username and password in Base64
                    var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{loginModel.UserName}:{loginModel.Password}"));

                    // Add Basic Authentication header
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64String);

                    // Set the content type
                    request.Content = new StringContent(xmlString, null, "application/xml");

                    // Send the request
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and print the response
                        string responseXml = await response.Content.ReadAsStringAsync();

                        var serializerResponse = new XmlSerializer(typeof(GetStoreUserListResponse));

                        using (TextReader reader = new StringReader(responseXml))
                        {
                            var getStoreUserListResponse = (GetStoreUserListResponse)serializerResponse.Deserialize(reader);
                            returnResponse.StoreUsers = getStoreUserListResponse.Body.GetStoreUserListResult.XmlFile.StoreUsers.ToList();
                            returnResponse.IsSuccess = true;
                        }

                        Console.WriteLine(responseXml);

                        return returnResponse;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        returnResponse.IsSuccess = false;
                        returnResponse.ErrorMessage = "Please check username and password";
                    }
                    else
                    {
                        returnResponse.ErrorMessage = "Something is wrong!";
                        returnResponse.IsSuccess = false;
                    }
                    return returnResponse;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                returnResponse.ErrorMessage = "Please connect to the VPN";
                return returnResponse;
            }

        }

        public async Task<List<StudentModel>> GetAllStudents()
        {
            var returnResponse = new List<StudentModel>();
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.GetAllStudents}";

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Setting.UserBasicDetail?.AccessToken}");
                var response = await client.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    bool isTokenRefreshed = await RefreshToken();
                    if (isTokenRefreshed) return await GetAllStudents();
                }
                else
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string contentStr = await response.Content.ReadAsStringAsync();
                        var mainResponse = JsonConvert.DeserializeObject<MainResponse>(contentStr);
                        if (mainResponse.IsSuccess)
                        {
                            returnResponse = JsonConvert.DeserializeObject<List<StudentModel>>(mainResponse.Content.ToString());
                        }
                    }
                }

            }
            return returnResponse;
        }

        public async Task<bool> RefreshToken()
        {
            bool isTokenRefreshed = false;
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.RefreshToken}";

                var serializedStr = JsonConvert.SerializeObject(new AuthenticateRequestAndResponse
                {
                    RefreshToken = Setting.UserBasicDetail.RefreshToken,
                    AccessToken = Setting.UserBasicDetail.AccessToken
                });

                try
                {
                    var response = await client.PostAsync(url, new StringContent(serializedStr, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        string contentStr = await response.Content.ReadAsStringAsync();
                        var mainResponse = JsonConvert.DeserializeObject<MainResponse>(contentStr);
                        if (mainResponse.IsSuccess)
                        {
                            var tokenDetails = JsonConvert.DeserializeObject<AuthenticateRequestAndResponse>(mainResponse.Content.ToString());
                            Setting.UserBasicDetail.AccessToken = tokenDetails.AccessToken;
                            Setting.UserBasicDetail.RefreshToken = tokenDetails.RefreshToken;

                            string userDetailsStr = JsonConvert.SerializeObject(Setting.UserBasicDetail);
                            //await SecureStorage.SetAsync(nameof(Setting.UserBasicDetail), userDetailsStr);
                            isTokenRefreshed = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }


            }
            return isTokenRefreshed;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> RegisterUser(RegistrationModel registerUser)
        {
            string errorMessage = string.Empty;
            bool isSuccess = false;
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.RegisterUser}";

                var serializedStr = JsonConvert.SerializeObject(registerUser);
                var response = await client.PostAsync(url, new StringContent(serializedStr, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            return (isSuccess, errorMessage);
        }

        public static string GetStoreUserListXml(string userCode)
        {
            var envelope = new GetStoreUserListRequest.Envelope
            {
                Body = new GetStoreUserListRequest.Body
                {
                    GetStoreUserList = new GetStoreUserListRequest.GetStoreUserList
                    {
                        userCode = userCode,
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

            return xmlString;
        }

        // Custom StringWriter to specify UTF-8 encoding
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
