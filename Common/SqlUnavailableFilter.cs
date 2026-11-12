using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTL_LTWEB.Common;

public sealed class SqlUnavailableFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception.GetBaseException() is not DbException) return;
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = 503,
            Title = "Không truy cập được dữ liệu SQL Server.",
            Detail = "Vui lòng kiểm tra kết nối và phiên bản cơ sở dữ liệu, sau đó thử lại."
        })
        { StatusCode = 503 };
        context.ExceptionHandled = true;
    }
}
