using System.Data.Entity;

namespace Fabrikam.Timesheets.Data
{
    public class TimesheetContext : DbContext
    {
        public IDbSet<AzureDirectory> AzureDirectories { get; set; }

        public IDbSet<RegistrationRequest> RegistrationRequests { get; set; }

        public IDbSet<User> Users { get; set; }

        public IDbSet<TimesheetEntry> TimesheetEntries { get; set; } 

        public TimesheetContext()
            : base("name=TimesheetDb")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(GetType().Assembly);
        }
    }
}
