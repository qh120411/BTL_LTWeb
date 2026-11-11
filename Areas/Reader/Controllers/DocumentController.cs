using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    [AllowAnonymous]
    public class DocumentController : ReaderBaseController
    {
        private readonly IBookCatalogService _catalogService;

        public DocumentController(IBookCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        // [Figma Reader 03] Chi tiết học liệu & Giới thiệu sách
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Search");
        }

        public IActionResult Details(int? id)
        {
            if (!id.HasValue) return RedirectToAction("Index", "Search");
            var book = _catalogService.GetById(id.Value);
            if (book == null) return NotFound();

            return View(book);
        }
    }
}
