using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Controllers
{
    public class ApiDocController : Controller
    {
        private readonly IAntiforgery _antiforgery;

        public ApiDocController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        [HttpGet("/doc")]
        [HttpGet("/docs")]
        [HttpGet("/api-doc")]
        [HttpGet("/api-docs")]
        public IActionResult Index()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            ViewBag.RequestToken = tokens.RequestToken;
            return View();
        }
    }
}
