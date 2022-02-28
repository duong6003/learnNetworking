using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace test.Controllers
{
    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Avatar { get; set; }
    }
    public class Product
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public string? Story { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        public string? NutriInfo { get; set; }
        public double? PointFrom { get; set; }
        public double? PointTo { get; set; }
    }
    public class ProductImage
    {
        public string? Path { get; set; }
        public Guid? ProductId { get; set; }
        public Product? Product { get; set; }
    }
    public class Repository{
        private HttpClient client;

        public Repository(HttpClient client)
        {
            this.client = client;
        }

        void ShowProduct(Product product)
        {
            
        }

        public async Task<Uri> CreateProductAsync( Product product, string requestUri)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(requestUri, product);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location!;
        }

        public async Task<Product?> GetProductAsync(string path)
        {
            Product? product = new();
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                product = await response.Content.ReadAsAsync<Product>();
            }
            return product;
        }

        public async Task<Product?> UpdateProductAsync( Product? product, string api)
        {
            if (product is null) return null!;
            HttpResponseMessage response = await client.PutAsJsonAsync(api, product);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            product = await response.Content.ReadAsAsync<Product>();
            return product;
        }

        public async Task<HttpStatusCode> DeleteProductAsync( string id, string api)
        {
            if (api.EndsWith("/")) api = api + id;
            else api = api + "/" + id;
            HttpResponseMessage response = await client.DeleteAsync(api);
            return response.StatusCode;
        }
    }
    public class DataReturn
    {

        public Product? Data { get; set; }
        public string? Message { get; set; }

    }
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetStudentRemote(string? url)
        {
            using HttpClient client = new();
            url = "https://localhost:5067/api/Relations";
            //MultipartFormDataContent content = new MultipartFormDataContent();
            //content.Add(new StringContent("Văn"), "Name");
           HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post,url);
            IDictionary<string,object> keys = new Dictionary<string, object>();
            string fileName = Path.GetFullPath(@"C:\Users\PC\Pictures\Jisoo_for_Marie_Claire_Korea_210914(1).jpg");
            byte[] image = System.IO.File.ReadAllBytes(fileName);
            IDictionary<string, object> content = new Dictionary<string, object>();
            content.Add("Title", "Title");
            content.Add("Detail", "Detail");
            content.Add("Story", "Story");
            content.Add("PointFrom", "11");
            content.Add("PointTo", "22");
            content.Add("ProductImages", image);
            content.Add("NutriInfo", image);
            message.CreateMessage(content);
            AuthenticationHeaderValue token = AuthenticationHeaderValue.Parse("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjAwNTc5NzBjLWI3M2UtNGYxYi1hOGYwLWI3YmNjNzgwMmQ2YSIsIlBob25lIjoiMDk4NzY1NDMyMSIsImV4cCI6MTY0ODEwNDk3Mn0.Y_Aa2vRS4VUPr-lWcN350DK3iTp40VomuRMP0R1m5sU");
            client.DefaultRequestHeaders.Add("Authentication", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjAwNTc5NzBjLWI3M2UtNGYxYi1hOGYwLWI3YmNjNzgwMmQ2YSIsIlBob25lIjoiMDk4NzY1NDMyMSIsImV4cCI6MTY0ODEwNDk3Mn0.Y_Aa2vRS4VUPr-lWcN350DK3iTp40VomuRMP0R1m5sU");

            var result = await client.SendAsync(message);

            //(string response, int? statusCode) = HttpMethod.Post.SendRequestWithFormDataContent(url, content);
            return NoContent();
         } 
    }
}
