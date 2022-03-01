using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace test
{
    public class HttpResponseResult<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
    public class HttpResultUltility<T>
    {
        public static async Task<(HttpResponseResult<T> result, int? statusCode)> GetHttpResponseResult(HttpClient client, HttpRequestMessage message)
        {
            try
            {
                using HttpResponseMessage response = await client.SendAsync(message).ConfigureAwait(false);
                int responseStatusCode = (int)response.StatusCode;
                
                if(response.IsSuccessStatusCode is false) return (new HttpResponseResult<T>() { Message = "Request failed, check your uri or your http method"}, responseStatusCode);
                
                if (response.Content != null)
                {
                    HttpResponseResult<T> result = await response.Content.ReadAsAsync<HttpResponseResult<T>>().ConfigureAwait(false);
                    return (result!, responseStatusCode); 
                }
                return (null! , responseStatusCode);
            }
            catch (Exception ex)
            {
                HttpResponseResult<T> error = new();
                error.Message = ex.Message;
                return (error, (int)HttpStatusCode.InternalServerError);
            }
        }
    }
    public enum ContentFlags
    {
        FromFrom,
        FromBody
    }
    public class HttpContentHelper<T>
    {
        public static IDictionary<string, object> GenerateContent(T entity, ContentFlags flag)
        {
            IDictionary<string, object> content = new Dictionary<string, object>();
            if (flag.Equals(ContentFlags.FromFrom))
            {
                PropertyInfo[] propertyInfo = typeof(T).GetProperties();
                foreach (PropertyInfo property in propertyInfo)
                {
                    content.Add(property.Name, property.GetValue(entity)!);
                }
            }
            else
            {
                content.Add("", JsonConvert.SerializeObject(entity));
            }
            return content;
        }
    }
}
