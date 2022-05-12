using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FacebookAuthenticationhandler.Facebook
{
    public class FacebookAuthHanlder : AuthenticationHandler<FacebookOptions>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IFacebookClient _facebookClient;

        public FacebookAuthHanlder(IOptionsMonitor<FacebookOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IFacebookClient facebookAgent, IMemoryCache memoryCache) : base(options, logger, encoder, clock)
        {
            _facebookClient = facebookAgent ?? throw new ArgumentNullException(nameof(facebookAgent));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();

            if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out AuthenticationHeaderValue? headerValue))
                return AuthenticateResult.NoResult();

            if (headerValue == null || !FacebookDefaults.AuthenticationScheme.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.NoResult();

            var cachedClaims = await _memoryCache.GetOrCreateAsync(headerValue.Parameter, async (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await GetClaims(headerValue?.Parameter);
            });

            if (cachedClaims?.Any() ?? false)
            {
                var identity = new ClaimsIdentity(cachedClaims, FacebookDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, FacebookDefaults.AuthenticationScheme);
                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.NoResult();
        }

        private async Task<IEnumerable<Claim>?> GetClaims(string? accessToken)
        {
            if (accessToken != null)
            {
                var appTask = _facebookClient.GetApp(accessToken);
                var userTask = _facebookClient.GetUser(accessToken, "email,name");
                await Task.WhenAll(appTask, userTask);
                var app = appTask.Result;
                var user = userTask.Result;

                //First we check if you're loggedin via our app by comparing the clientid, next we'll check if your useraccount realy exists
                if (app?.Id == Options.AppId && user != null)
                    return new[]{
                        new Claim("socialuserid", user.Id, ClaimValueTypes.String),
                        new Claim("email", user.Email, ClaimValueTypes.Email),
                        new Claim("name", user.Name, ClaimValueTypes.String)
                    };
            }
            return null;
        }
    }
}
