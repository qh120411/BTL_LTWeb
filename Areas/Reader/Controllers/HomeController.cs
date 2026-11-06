using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BTL_LTWEB.Areas.Reader.Controllers;
[Area("Reader"), AllowAnonymous]
public class HomeController(IBookCatalogService catalog) : ReaderBaseController {
    public IActionResult Index() => View(catalog.GetAll().Take(4).ToList());
}
