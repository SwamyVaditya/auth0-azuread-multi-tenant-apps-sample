# Azure AD Multi-Tenant Applications with Auth0

Sample for the "Fabrikam Timesheets SaaS" application which is hosted here: http://fabrikam-timesheets.azurewebsites.net

This sample shows how to build a multi-tenant SaaS application with Azure AD and Auth0.

## Getting started.

This sample is based on the [auth0-aspnet-owin](https://github.com/auth0/auth0-aspnet-owin) sample. First we'll have the setup in the `Startup.cs`file:

```cs
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
```

And then we'll need our Azure AD credentials, Auth0 credentials and the connection string for the database:

```xml
  <appSettings>
    <add key="webpages:Version" value="3.0.0.0" />
    <add key="webpages:Enabled" value="false" />
    <add key="ClientValidationEnabled" value="true" />
    <add key="UnobtrusiveJavaScriptEnabled" value="true" />

    <!-- AzureAD -->
    <add key="AzureAD:DirectoryName" value="fabrikamcorporation.onmicrosoft.com" />
    <add key="AzureAD:ClientId" value="..." />
    <add key="AzureAD:Key" value="..." />

    <!-- Auth0 configuration. -->
    <add key="auth0:ClientId" value="..." />
    <add key="auth0:ClientSecret" value="..." />
    <add key="auth0:Domain" value="fabrikam.auth0.com" />
  </appSettings>
  <connectionStrings>
    <add name="TimesheetDb" providerName="System.Data.SqlClient" connectionString="..." />
  </connectionStrings>
```

## What is Auth0?

Auth0 helps you to:

* Add authentication with [multiple authentication sources](https://docs.auth0.com/identityproviders), either social like **Google, Facebook, Microsoft Account, LinkedIn, GitHub, Twitter, Box, Salesforce, amont others**, or enterprise identity systems like **Windows Azure AD, Google Apps, Active Directory, ADFS or any SAML Identity Provider**.
* Add authentication through more traditional **[username/password databases](https://docs.auth0.com/mysql-connection-tutorial)**.
* Add support for **[linking different user accounts](https://docs.auth0.com/link-accounts)** with the same user.
* Support for generating signed [Json Web Tokens](https://docs.auth0.com/jwt) to call your APIs and **flow the user identity** securely.
* Analytics of how, when and where users are logging in.
* Pull data from other sources and add it to the user profile, through [JavaScript rules](https://docs.auth0.com/rules).

## Create a free Auth0 Account

1. Go to [Auth0](https://auth0.com) and click Sign Up.
2. Use Google, GitHub or Microsoft Account to login.

## Issue Reporting

If you have found a bug or if you have a feature request, please report them at this repository issues section. Please do not report security vulnerabilities on the public GitHub issue tracker. The [Responsible Disclosure Program](https://auth0.com/whitehat) details the procedure for disclosing security issues.

## Author

[Auth0](auth0.com)

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.
