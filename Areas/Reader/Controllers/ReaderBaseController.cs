using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BTL_LTWEB.Areas.Reader.Controllers;
[Area("Reader"), Authorize]
public abstract class ReaderBaseController : Controller { }
