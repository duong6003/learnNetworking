using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace test.Controllers
{
    public class PageInfo
    {
        public long TotalCount { get; set; }
        public long PageSize { get; set; }
        public long CurrentPage { get; set; }
        public long TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }

        public PageInfo()
        { }

        public PageInfo(long totalCount, long pageSize, long currentPage, long totalPages, bool hasNext, bool hasPrevious)
        {
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = totalPages;
            HasNext = hasNext;
            HasPrevious = hasPrevious;
        }
    }

    public class PaginationResponse<T>
    {
        public IEnumerable<T> PagedData { get; set; }
        public PageInfo PageInfo { get; set; }

        public PaginationResponse(IEnumerable<T> items, PageInfo pageInfo)
        {
            PagedData = items;
            PageInfo = pageInfo;
        }
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
        public Guid Id { get; set; }
        public string? Path { get; set; }
        public Guid? ProductId { get; set; }
        public Product? Product { get; set; }
    }
    public class ProductImageTest
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
    }

    public class ShelfUploadRequest
    {
        [IsComplex(ComplexType.IsEntity)]
        public ProductImageTest? ProductImageTest { get; set; }
        //public IList<IFormFile>? Images { get; set; }
    }
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetStudentRemote([FromBody]ShelfUploadRequest request)
        {
            IDictionary<string, object> header = new Dictionary<string,object>();
            header.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjAwNTc5NzBjLWI3M2UtNGYxYi1hOGYwLWI3YmNjNzgwMmQ2YSIsIlBob25lIjoiMDk4NzY1NDMyMSIsImV4cCI6MTY0ODEwNDk3Mn0.Y_Aa2vRS4VUPr-lWcN350DK3iTp40VomuRMP0R1m5sU");

            HttpClient client = _httpClientFactory.CreateClient("client");
            string url = "https://localhost:5067/api/Shelfs";
            using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url);

            (HttpResponseResult<PaginationResponse<ShelfUploadRequest>> result, int? status) = await HttpResultUltility<PaginationResponse<ShelfUploadRequest>>.GetHttpResponseResult(client, message);
            return NoContent();
        }
    }
}