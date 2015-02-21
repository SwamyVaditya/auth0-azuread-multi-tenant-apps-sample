using System;
using System.Configuration;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;

using Fabrikam.Timesheets.Data;
using Fabrikam.Timesheets.Mvc.Models;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Fabrikam.Timesheets.Mvc.Controllers
{
    [RoutePrefix("registration")]
    public class RegistrationController : OwinController
    {
        private const string OnboardingUrl =
            "https://login.windows.net/common/oauth2/authorize?response_type=code&client_id={0}&resource={1}&redirect_uri={2}&state={3}";

        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            return PartialView(new RegistrationModel());
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult> Start(RegistrationModel model)
        {
            using (var db = new TimesheetContext())
            {
                // Create a new temporary tenant.
                var registration = db.RegistrationRequests.Add(new RegistrationRequest
                {
                    Id = Guid.NewGuid(),
                    CreatedOn = DateTime.UtcNow,
                    SignupToken = Guid.NewGuid().ToString(),
                    AdminConsented = model.EnableForMyOrganization,
                    OrganizationName = model.OrganizationName
                });
                await db.SaveChangesAsync();

                // Build the redirect to the consent page.
                var authorizationRequest = String.Format(OnboardingUrl,
                    Uri.EscapeDataString(ConfigurationManager.AppSettings["AzureAD:ClientID"]),
                    Uri.EscapeDataString("https://graph.windows.net"),
                    Uri.EscapeDataString(Request.Url.GetLeftPart(UriPartial.Authority) + "/registration/complete"),
                    Uri.EscapeDataString(registration.SignupToken));
                if (model.EnableForMyOrganization)
                    authorizationRequest += String.Format("&prompt={0}", Uri.EscapeDataString("admin_consent"));
                return Redirect(authorizationRequest);
            }
        }

        [HttpGet]
        [Route("complete")]
        public async Task<ActionResult> Complete(string code, string error, string error_description, string resource, string state)
        {
            using (var db = new TimesheetContext())
            {
                if (!String.IsNullOrEmpty(error) || !String.IsNullOrEmpty(error_description))
                {
                    return View("RegistrationError", new RegistrationErrorModel() { Error = error, ErrorDescription = error_description });
                }

                var registrationRequest = await db.RegistrationRequests.FirstOrDefaultAsync(r => r.SignupToken == state);
                if (registrationRequest == null)
                {
                    return View("RegistrationRequestUnknown");
                }
                
                // Get the user's profile from Azure AD.
                var credential = new ClientCredential(ConfigurationManager.AppSettings["AzureAD:ClientID"], 
                    ConfigurationManager.AppSettings["AzureAD:Key"]);
                var authContext = new AuthenticationContext("https://login.windows.net/common/");
                var result = authContext.AcquireTokenByAuthorizationCode(code, new Uri(Request.Url.GetLeftPart(UriPartial.Path)), credential);

                // Clean up the registration request.
                db.RegistrationRequests.Remove(registrationRequest);

                // Prevent duplicate users.
                var userExists = await db.Users.AnyAsync(u => u.AzureTenantId == result.TenantId && u.AzureUpn == result.UserInfo.DisplayableId);
                if (!userExists)
                {
                    // Create the user.
                    var user = new User();
                    user.CreatedOn = DateTime.UtcNow;
                    user.Id = Guid.NewGuid();
                    user.Firstname = result.UserInfo.GivenName;
                    user.Lastname = result.UserInfo.FamilyName;
                    user.AzureTenantId = result.TenantId;
                    user.AzureUpn = result.UserInfo.DisplayableId;
                    db.Users.Add(user);
                }

                // Consent happend by administrator for the whole organization..
                if (registrationRequest.AdminConsented)
                {
                    // Prevent duplicate tenants.
                    var directoryExists = await db.AzureDirectories.AnyAsync(u => u.TenantId == result.TenantId);
                    if (!directoryExists)
                    {
                        // Create the tenant.
                        var tenant = new AzureDirectory();
                        tenant.CreatedOn = DateTime.UtcNow;
                        tenant.Id = Guid.NewGuid();
                        tenant.Issuer = String.Format("https://sts.windows.net/{0}/", result.TenantId);
                        tenant.TenantId = result.TenantId;
                        tenant.Name = registrationRequest.OrganizationName;
                        db.AzureDirectories.Add(tenant);
                    }

                    // Save.
                    await db.SaveChangesAsync();

                    // Show confirmation page.
                    return View("TenantRegistrationSuccess");
                }
                
                // Save.
                await db.SaveChangesAsync();

                // Show user confirmation page.
                return View("UserRegistrationSuccess");
            }
        }
    }
}