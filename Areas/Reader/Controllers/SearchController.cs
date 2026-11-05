using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class SearchController : ReaderBaseController
    {
        private readonly IBookCatalogService _catalogService;

        public SearchController(IBookCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        // [Figma Reader 02] Tra cứu học liệu OPAC đa tiêu chí
        public IActionResult Index(
            string? keyword, 
            string? category, 
            string? major, 
            string? language, 
            string? availability)
        {
            var cat = string.IsNullOrWhiteSpace(category) ? "all" : category.Trim();
            var maj = string.IsNullOrWhiteSpace(major) ? "all" : major.Trim();
            var lang = string.IsNullOrWhiteSpace(language) ? "all" : language.Trim();
            var avail = string.IsNullOrWhiteSpace(availability) ? "all" : availability.Trim();
            var kw = keyword?.Trim() ?? string.Empty;

            var items = _catalogService.Search(kw, cat, maj, lang, avail).ToList();

            var viewModel = new BookSearchViewModel
            {
                Keyword = kw,
                Category = cat,
                Major = maj,
                Language = lang,
                Availability = avail,
                Items = items
            };

            return View(viewModel);
        }
    }
}
