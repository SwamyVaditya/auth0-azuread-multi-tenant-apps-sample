using System;
using System.ComponentModel.DataAnnotations;

namespace Fabrikam.Timesheets.Data
{
    public class TimesheetEntry : Entity
    {
        public Guid UserId { get; set; }

        public User User { get; set; }

        public DateTime Date { get; set; }

        public string Title { get; set; }
    }
}