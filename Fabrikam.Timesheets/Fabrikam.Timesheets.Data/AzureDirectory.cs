namespace Fabrikam.Timesheets.Data
{
    public class AzureDirectory : Entity
    {
        public string TenantId { get; set; }

        public string Name { get; set; }

        public string Issuer { get; set; }
    }
}