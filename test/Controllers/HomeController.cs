using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace test.Controllers
{
    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Avatar { get; set; }
    }
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetStudentRemote()
        {
            var some = new Student()
            {
                Id = 69,
                Name = "dasd",
                Avatar = "sdasd"
            };
            var studentstring = JsonConvert.SerializeObject(some);
            HttpMethod post = HttpMethod.Post;
            (string? student, int? status) = post.SendRequestWithStringContent("https://621840fd1a1ba20cba9c4b7c.mockapi.io/api/student");
            HttpMethod check = HttpMethod.Get;
            //(string? student, int? status) = post.SendRequestWithStringContent("https://621840fd1a1ba20cba9c4b7c.mockapi.io/api/student", studentstring);
            return new OkObjectResult(student);
         } 
    }
}
