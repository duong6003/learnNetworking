using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

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
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Price { get; set; }
        public string? Category { get; set; }
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
    public class Data
    {
        public static Student student = new Student()
        {
            Id = 69,
            Name = "dasd",
            Avatar = "sdasd"
        };
        public static Product product = new Product()
        {
            Id = 69,
            Name = "dasd",
            Price = "sdasd",
            Category = "dasdad"
        };
    }
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetStudentRemote(string? url)
        {
            HttpClient client = new();
            url = "https://621840fd1a1ba20cba9c4b7c.mockapi.io/api/product/1";
            Repository? repository = new Repository(client);

            //Uri path = repository.CreateProductAsync(Data.product, url).Result;
            //var result = repository.GetProductAsync(url).Result;
            //var result = repository.DeleteProductAsync("27", url).Result;
            Product? product = new()
            {
                Id = 1,
                Name = "dasd",
                Price = "sdasd",
                Category = "dasdad"
            };
            Product? result = repository.UpdateProductAsync(product, url).Result;

            //HttpRequestMessage httpRequest = new(); 
            //HttpMethod post = HttpMethod.Post;
            //HttpMethod get = HttpMethod.Get;
            //(string? student, int? status) = post.SendRequestWithFormDataContent("https://621840fd1a1ba20cba9c4b7c.mockapi.io/api/student");
            //(string? student, int? status) = post.SendRequestWithStringContent("https://621840fd1a1ba20cba9c4b7c.mockapi.io/api/student");
            return NoContent();
         } 
    }
}
