using System;
using System.ComponentModel.DataAnnotations;

namespace Fabrikam.Timesheets.Data
{
    public class Entity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
