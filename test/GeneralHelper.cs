﻿
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace test
{
    public static class GeneralHelper
    {
        private static readonly HttpClient HttpClient = new ();

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext">
        /// Getting the current url of the request depends on whether you use Https or not?
        /// </param>
        /// <param name="useHttps">
        /// Option to use Https (The parameter is taken from the UseHttps property in appsetting.json):
        ///     There used: true
        ///     Do not use: false
        /// </param>
        /// <returns></returns>
        public static string GetBaseUrl(this HttpContext httpContext, bool useHttps)
        {
            if (useHttps)
            {
                return httpContext.Request.Scheme + "s://" + httpContext.Request.Host.Value;
            }
            else
            {
                return httpContext.Request.Scheme + "://" + httpContext.Request.Host.Value;
            }
        }

        public static T MergeData<T>(this object newData, T originData)
        {
            foreach (PropertyInfo propertyInfo in newData.GetType().GetProperties())
            {
                if (propertyInfo.GetValue(newData, null) != null && originData!.GetType().GetProperties().Any(p => p.Name.Equals(propertyInfo.Name)))
                {
                    originData.GetType().GetProperty(propertyInfo.Name)!.SetValue(originData, propertyInfo.GetValue(newData, null));
                }
            }
            return originData;
        }

        public static async Task<string> UploadFile(this IFormFile requestFile, string folderPath)
        {
            if(requestFile != null)
            {
                string fileFullName = new Random().Next() + "_" + Regex.Replace(requestFile.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "");
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                Directory.CreateDirectory(Path.Combine(webRootPath, folderPath)); // Tự động tạo dường dẫn thư mục nếu chưa có
                using (var stream = new FileStream(Path.Combine(webRootPath, folderPath, fileFullName), FileMode.Create))
                {
                    await requestFile.CopyToAsync(stream);
                    stream.Close();
                }
                return fileFullName;
            }

            return "";
        }

        public static  string CreatePath(string pathName)
        {
            string FileDic = pathName;
            string FilePath = Path.Combine("", FileDic);
            return FilePath;
        }

        //public static async Task<bool> SendEmail(string from, string to, string subject, string body, SmtpClient client)
        //{
        //    MailMessage mail = new MailMessage(
        //           from: from,
        //           to: to,
        //           subject: subject,
        //           body: body
        //           );

        //    mail.BodyEncoding = System.Text.Encoding.UTF8;
        //    mail.SubjectEncoding = System.Text.Encoding.UTF8;
        //    mail.IsBodyHtml = true;
        //    mail.ReplyToList.Add(new MailAddress(from));
        //    mail.Sender = new MailAddress(from);
        //    try
        //    {
        //        await client.SendMailAsync(mail);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        public static (bool, string) DeleteFile(this string FileToDelete)
        {
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            try
            {
                string fullPathToDelete = Path.Combine(webRootPath, FileToDelete);
                if (File.Exists(fullPathToDelete))
                {
                    File.Delete(fullPathToDelete);
                    return (true, "File deleted successfully!");
                }
                else
                {
                    return (false, "File no longer exists on the server!");
                }
            }
            catch (IOException ioExp)
            {
                return (false, ioExp.Message);
            }
        }

        public static bool PingIPDevice(this string targetHost, string data = "PingForTest")
        {
            Ping pingSender = new ();
            PingOptions options = new()
            {
                DontFragment = true
            };
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            PingReply reply = pingSender.Send(targetHost, 120, buffer, options);
            if (reply.Status == IPStatus.Success)
                return true;
            return false;
        }

        public static bool ValidateIPv4(this string DeviceIP)
        {
            if (string.IsNullOrWhiteSpace(DeviceIP))
                return false;
            string[] splitValues = DeviceIP.Split('.');
            if (splitValues.Length != 4)
                return false;
            return splitValues.All(r => byte.TryParse(r, out byte tempForParsing));
        }

        public static string GetMimeType(this string fileName)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string? contentType);
            return contentType ?? "application/octet-stream";
        }

        public static (bool flagCheckToken, JwtSecurityToken dataToken) ValidateToken(this string token)
        {
            JwtSecurityToken _ = new();
            try
            {
                _ = new JwtSecurityTokenHandler().ReadJwtToken(token);
            }
            catch (Exception)
            {
                return (false, null!);
            }
            return (true, _);
        }
    }
}