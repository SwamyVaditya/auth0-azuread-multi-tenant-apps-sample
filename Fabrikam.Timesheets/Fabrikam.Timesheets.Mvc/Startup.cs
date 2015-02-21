using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Fabrikam.Timesheets.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;

using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace Fabrikam.Timesheets.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        { 
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);


            // Use Auth0
            var provider = new Auth0.Owin.Auth0AuthenticationProvider
            {
                OnAuthenticated = context =>
                {
                    // Add custom claims we get from Azure AD to the user's identity.
                    if (context.User["tenantid"] != null)
                        context.Identity.AddClaim(new Claim("tenantid", context.User.Value<string>("tenantid")));
                    if (context.User["upn"] != null)
                        context.Identity.AddClaim(new Claim("upn", context.User.Value<string>("upn")));
                    return Task.FromResult(0);
                }
            };
            
            app.UseAuth0Authentication(
                clientId: ConfigurationManager.AppSettings["auth0:ClientId"],
                clientSecret: ConfigurationManager.AppSettings["auth0:ClientSecret"],
                domain: ConfigurationManager.AppSettings["auth0:Domain"],
                redirectPath: "/account/callback",
                provider: provider);
        }
    }
}