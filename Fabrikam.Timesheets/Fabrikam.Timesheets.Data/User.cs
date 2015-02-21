using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fabrikam.Timesheets.Data
{
    public class User : Entity
    {
        public string Identifier { get; set; }

        public string Provider { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string AzureUpn { get; set; }

        public string AzureTenantId { get; set; }
    }
}