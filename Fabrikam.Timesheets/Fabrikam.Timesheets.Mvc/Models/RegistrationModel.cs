using System.ComponentModel;

namespace Fabrikam.Timesheets.Mvc.Models
{
    public class RegistrationModel
    {
        [DisplayName("Organization Name")]
        public string OrganizationName { get; set; }

        [DisplayName("Enable for my organization")]
        public bool EnableForMyOrganization { get; set; }
    }
}