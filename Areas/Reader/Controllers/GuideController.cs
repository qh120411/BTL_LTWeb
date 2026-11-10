using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    [AllowAnonymous]
    public class GuideController : ReaderBaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
