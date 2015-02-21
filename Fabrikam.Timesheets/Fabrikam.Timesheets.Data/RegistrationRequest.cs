using System;

namespace Fabrikam.Timesheets.Data
{
    public class RegistrationRequest : Entity
    {
        public string SignupToken { get; set; }

        public bool Completed { get; set; }

        /// <summary>
        /// Did we try to register the whole organization?
        /// </summary>
        public bool AdminConsented { get; set; }

        /// <summary>
        /// User or AzureDirectory the request was linked to.
        /// </summary>
        public Guid Identifier { get; set; }

        /// <summary>
        /// Organization name.
        /// </summary>
        public string OrganizationName { get; set; }
    }
}