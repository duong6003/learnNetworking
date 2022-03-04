using Microsoft.AspNetCore.StaticFiles;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace test
{
    public static class GeneralHelper
    {
        private static readonly HttpClient HttpClient = new();

        public static async Task<(string responseData, int? responseStatusCode)> SendRequestAsync(this HttpRequestMessage httpRequestMessage, string endpointURL, IDictionary<string, object> headers)
        {
            if (headers != null)
            {
                foreach (KeyValuePair<string, object> header in headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value.ToString());
                }
            }
            try
            {
                using HttpResponseMessage httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                int responseStatusCode = (int)httpResponseMessage.StatusCode;
                if (httpResponseMessage.Content != null)
                {
                    return (await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false), responseStatusCode);
                }
                else if (httpResponseMessage.Content is null && httpResponseMessage.IsSuccessStatusCode)
                {
                    return (null!, responseStatusCode!);
                }
                else
                {
                    return ($"Request to {endpointURL} error! StatusCode = {httpResponseMessage.StatusCode}", responseStatusCode);
                }
            }
            catch (Exception ex)
            {
                return (ex.Message, (int)HttpStatusCode.InternalServerError);
            }
        }

        public static (string responseData, int? responseStatusCode) SendRequestWithStringContent(this HttpMethod method, string endpointURL, string encodingData = null!, IDictionary<string, object> headers = null!, string mediaType = "application/json")
        {
            HttpRequestMessage httpRequestMessage = new(method, endpointURL);
            if (encodingData is not null)
            {
                httpRequestMessage.Content = new StringContent(encodingData, Encoding.UTF8, mediaType);
            }
            return httpRequestMessage.SendRequestAsync(endpointURL, headers).Result;
        }

        public static (string responseData, int? responseStatusCode) SendRequestWithFormDataContent(this HttpMethod method, string endpointURL, MultipartFormDataContent multipartFormData = null!, IDictionary<string, object> headers = null!)
        {
            HttpRequestMessage httpRequestMessage = new(method, endpointURL);
            if (multipartFormData is not null)
            {
                httpRequestMessage.Content = multipartFormData;
            }
            return httpRequestMessage.SendRequestAsync(endpointURL, headers).Result;
        }
        // -------------------------------------Ở dưới này là code của e----------------------------------------------

        public static async Task<(string responseData, int? responseStatusCode)> SendRequestWithObjectToMultipartFormDate(this HttpMethod method, string endpointURL, object sender, IDictionary<string, object> headers = null!, string mediaType = "application/json")
        {
            HttpRequestMessage httpRequestMessage = new(method, endpointURL);
            MultipartFormDataContent multipartFormData = new();
            IDictionary<string, object> contents = HttpContentHelper<object>.GenerateContent(sender);
            foreach (var keyPair in contents!)
            {
                if (keyPair.Value is byte[])
                {
                    multipartFormData.Add(new ByteArrayContent((byte[])keyPair.Value), keyPair.Key, "file");
                }
                else if (keyPair.Value is string || keyPair.Value is DateTime)
                {
                    multipartFormData.Add(new StringContent(keyPair.Value.ToString()!, Encoding.UTF8, mediaType), keyPair.Key);
                }
                else if (keyPair.Value is IFormFile || keyPair.Value is IEnumerable<IFormFile>)
                {
                    if (keyPair.Value is IFormFile)
                    {
                        IFormFile file = (IFormFile)keyPair.Value;
                        StreamContent streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                        multipartFormData.Add(streamContent, file.Name, file.FileName);
                    }
                    IList<IFormFile> files = (keyPair.Value as IList<IFormFile>)!;
                    foreach (IFormFile file in files)
                    {
                        StreamContent streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                        multipartFormData.Add(streamContent, file.Name, file.FileName);
                    }
                }
                else
                {
                    try
                    {

                        IDictionary<string, object> properties = new Dictionary<string, object>();
                        await keyPair.Value.GetComplexProperty(properties);
                        MultipartContent content = new MultipartContent();
                        int n = properties.Keys.Count;
                        foreach (KeyValuePair<string, object> keyValuePair in properties)
                        {
                            content.Add(new StringContent((string)keyValuePair.Value));
                        }
                        multipartFormData.Add(content);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                httpRequestMessage.Content = multipartFormData;
            }
            return httpRequestMessage.SendRequestAsync(endpointURL, headers).Result;
        }

        public static async Task GetComplexProperty(this object property, IDictionary<string, object> propertyHash, string currentName = "")
        {
            PropertyInfo[] infos = property.GetType().GetProperties();
            foreach (PropertyInfo info in infos)
            {
                currentName = string.IsNullOrEmpty(currentName) ? info.Name : currentName + "." + info.Name;

                if (info.GetValue(property)!.CheckComplexProperty().Result == ComplexType.IsEntity)
                {
                    await info.GetValue(property)!.GetComplexProperty(propertyHash, currentName);
                }
                if (info.GetValue(property)!.CheckComplexProperty().Result == ComplexType.IsCollection)
                {
                    var entities = info.GetValue(property) as ICollection<object>;
                    foreach (object entity in entities!)
                    {
                        propertyHash.Add(info.Name, info.GetValue(entity)!);
                    }
                }
                propertyHash.Add(currentName, info.GetValue(property)!);
                currentName = await currentName.PopLastSequence(".");
            }
            return;
        }

        public static Task<string> PopLastSequence(this string sequence, string sign)
        {
            if (string.IsNullOrEmpty(sequence)) return Task.FromResult("");
            int index = sequence.LastIndexOf(sign);
            if (index == -1)
            {
                return Task.FromResult("");
            }
            sequence = sequence.Substring(0, index);
            return Task.FromResult(sequence);
        }

        public static Task<ComplexType> CheckComplexProperty(this object property)
        {
            IEnumerable<Attribute> complexAttributeMembers = property.GetType().GetCustomAttributes(typeof(IsComplexAttribute))!;
            foreach (Attribute attribute in complexAttributeMembers)
            {
                if (attribute is IsComplexAttribute)
                {
                    IsComplexAttribute check = (IsComplexAttribute)attribute;
                    return Task.FromResult(check.ComplexType);
                }
            }
            return Task.FromResult(ComplexType.None);
        }
    }
}