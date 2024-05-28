using System.Net;
using System.Net.Http;
using System.Web.Http.Results;
using System.Web.Http.ExceptionHandling;
using System.Threading.Tasks;
using System.Threading;

namespace PCiServer.DEM.Web.Api.Common
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Logger.LogError(context.Exception, "");

            var response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
                new
                {
                    status = "error",
                    message = "An unexpected error occured. Please try again later."
                });

            context.Result = new ResponseMessageResult(response);

            return Task.CompletedTask;
        }
    }
}
