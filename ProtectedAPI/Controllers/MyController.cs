using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProtectedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MyController : Controller
    {
        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
