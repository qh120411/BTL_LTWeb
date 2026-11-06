using BTL_LTWEB.Areas.Reader.Services;

using Microsoft.AspNetCore.Mvc;



namespace BTL_LTWEB.Areas.Reader.Controllers

{

    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [Area("Reader")]

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

            var bookId = id ?? 1;

            var book = _catalogService.GetById(bookId);

            if (book == null) return NotFound();
            return View(book);

        }

    }

}
