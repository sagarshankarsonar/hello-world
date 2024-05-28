using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace PCiServer.DEM.Web.Api.Common.Identity
{
    public class CustomAuthenticationFilter : AuthorizeAttribute, IAuthenticationFilter
    {
        public override bool AllowMultiple
        {
            get { return false; }
        }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;

            try
            {
                string authParameter = string.Empty;
                AuthenticationHeaderValue authorization = request.Headers.Authorization;
                if (authorization == null)
                {
                    context.ErrorResult = new AuthenticationFailureResult("Missing Authorization Header", request);
                    return;
                }
                else if (authorization.Scheme != "Bearer")
                {
                    context.ErrorResult = new AuthenticationFailureResult("Invalid Authorization Schema", request);
                    return;
                }
                else if (string.IsNullOrEmpty(authorization.Parameter))
                {
                    context.ErrorResult = new AuthenticationFailureResult("Missing token", request);
                    return;
                }
                else
                {
                    context.Principal = TokenManager.GetPrincipal(authorization.Parameter);
                }

            }
            catch (SecurityTokenExpiredException)
            {
                context.ErrorResult = new AuthenticationFailureResult("Token is invalid or expired", request);
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var challenge = new AuthenticationHeaderValue("Bearer", "realm=\"Access to the site\"");
            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
            return Task.FromResult(0);
        }

        public class AuthenticationFailureResult : IHttpActionResult
        {
            public string ReasonPharse;
            public HttpRequestMessage Request { get; set; }

            public AuthenticationFailureResult(string reasonPharse, HttpRequestMessage request)
            {
                ReasonPharse = reasonPharse;
                Request = request;
            }
            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(Execute());
            }

            private HttpResponseMessage Execute()
            {
                var response = new
                {
                    status = "error",
                    message = ReasonPharse
                };

                return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
            }
        }

        public class AddChallengeOnUnauthorizedResult : IHttpActionResult
        {
            public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
            {
                Challenge = challenge;
                InnerResult = innerResult;
            }

            public AuthenticationHeaderValue Challenge { get; private set; }

            public IHttpActionResult InnerResult { get; private set; }

            /// <summary>
            /// This method is used to add challege on Unauthorized result for example setting the authentication scheme
            /// </summary>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                HttpResponseMessage response = await InnerResult.ExecuteAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Only add one challenge per authentication scheme.
                    if (!response.Headers.WwwAuthenticate.Any((h) => h.Scheme == Challenge.Scheme))
                    {
                        response.Headers.WwwAuthenticate.Add(Challenge);
                    }
                }

                return response;
            }
        }
    }
}
