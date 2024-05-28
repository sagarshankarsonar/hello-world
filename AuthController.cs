using PCiServer.DEM.Web.Api.Modules.Admin.Models;
using PCiServer.DEM.Web.Api.Modules.Admin.Services;
using PCiServer.DEM.Web.Api.Common.Identity;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web;

namespace PCiServer.DEM.Web.Api.Modules.Admin.Controllers
{
    [RoutePrefix("api/admin")]
    public class AuthController : ApiController
    {
        private const string MODULE = "Admin";

        private readonly ILoginService logService;

        public AuthController(ILoginService logService)
        {
            this.logService = logService;
        }

        [HttpPost]
        [Route("auth")]
        [ResponseType(typeof(LoginResponseModel))]
        [AllowAnonymous]
        public IHttpActionResult Login([FromBody] LoginData loginData)
        {
            return Ok(logService.AuthenticateUser(loginData));
        }

        [HttpGet]
        [Route("refreshtoken")]
        [ResponseType(typeof(LoginResponseModel))]
        [CustomAuthenticationFilter]
        public IHttpActionResult RefreshToken()
        {
            return Ok(TokenManager.GenerateRefreshToken(HttpContext.Current, MODULE));
        }
    }
}
