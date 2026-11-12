using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class HomeController : ReaderBaseController
    {
        private readonly IBookCatalogService _catalog;
        public HomeController(IBookCatalogService catalog) => _catalog = catalog;

        // [Figma Reader 01] Trang chủ Thư viện UTC
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View("~/Views/Home/Reader/Index.cshtml", _catalog.GetAll().Where(b => b.Category == "books").Take(4).ToList());
        }
    }
}
