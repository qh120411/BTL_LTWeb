using BTL_LTWEB.Areas.Admin.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public abstract class AdminBaseController : Controller
    {
    }
}
