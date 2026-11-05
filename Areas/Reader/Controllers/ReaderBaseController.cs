using BTL_LTWEB.Areas.Reader.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    [ReaderAuthorize]
    public abstract class ReaderBaseController : Controller
    {
    }
}
