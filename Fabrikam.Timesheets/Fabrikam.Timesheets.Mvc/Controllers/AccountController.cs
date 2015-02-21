using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Fabrikam.Timesheets.Data;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Fabrikam.Timesheets.Mvc.Controllers
{
    [RoutePrefix("account")]
    public class AccountController : OwinController
    {
        [Route("callback")]
        [AllowAnonymous]
        public async Task<ActionResult> Callback(string returnUrl)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            var externalIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
            if (externalIdentity == null)
            {
                throw new Exception("Could not get the external identity. Please check your Auth0 configuration settings and ensure that " +
                                    "you configured UseCookieAuthentication and UseExternalSignInCookie in the OWIN Startup class. " +
                                    "Also make sure you are not calling setting the callbackOnLocationHash option on the JavaScript login widget.");
            }

            // Create or update the user.
            using (var db = new TimesheetContext())
            {
                var givenNameClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == "given_name");
                var familyNameClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == "family_name");
                var providerClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == "provider");
                var identifierClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == "user_id");
                var tenantClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == "tenantid");
                var upnClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == "upn");

                // Check if the user already exists.
                var user = await db.Users.FirstOrDefaultAsync(u => u.Provider == providerClaim.Value && u.Identifier == identifierClaim.Value);
                if (user == null && tenantClaim != null && upnClaim != null)
                    user = await db.Users.FirstOrDefaultAsync(u => u.AzureTenantId == tenantClaim.Value && u.AzureUpn == upnClaim.Value);

                // New user.
                if (user == null)
                    user = db.Users.Add(new User { Id = Guid.NewGuid(), CreatedOn = DateTime.UtcNow });

                // Update user profile.
                user.Identifier = identifierClaim.Value;
                user.Provider = providerClaim.Value;
                user.Firstname = givenNameClaim != null ? givenNameClaim.Value : null;
                user.Lastname = familyNameClaim != null ? familyNameClaim.Value : null;
                user.AzureTenantId = tenantClaim != null ? tenantClaim.Value : null;
                user.AzureUpn = upnClaim != null ? upnClaim.Value : null;

                await db.SaveChangesAsync();

                // Login.
                var applicationIdentity = CreateIdentity(externalIdentity, user);
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, applicationIdentity);
            }

            // Optional: Redirect new users to page where they can complete their profile.
            return RedirectToLocal(returnUrl ?? Url.Action("Index", "TimesheetEntries"));
        }

        [Route("logoff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff(string returnUrl)
        {
            var appTypes = AuthenticationManager.GetAuthenticationTypes().Select(at => at.AuthenticationType).ToArray();
            AuthenticationManager.SignOut(appTypes);

            var absoluteReturnUrl = string.IsNullOrEmpty(returnUrl) ?
                this.Url.Action("Index", "Home", new { }, this.Request.Url.Scheme) :
                this.Url.IsLocalUrl(returnUrl) ?
                    new Uri(this.Request.Url, returnUrl).AbsoluteUri : returnUrl;

            return Redirect(
                string.Format("https://{0}/logout?returnTo={1}",
                    ConfigurationManager.AppSettings["auth0:Domain"],
                    absoluteReturnUrl));
        }
		
        private static ClaimsIdentity CreateIdentity(ClaimsIdentity externalIdentity, User user)
        {
            var identity = new ClaimsIdentity(externalIdentity.Claims, DefaultAuthenticationTypes.ApplicationCookie);

            // Add our internal ID as name identifier claim.
            identity.RemoveClaim(identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            // Required for anti forgery token.
            identity.AddClaim(
                new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", 
                    "Auth0 Identity", "http://www.w3.org/2001/XMLSchema#string"));
            return identity;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}