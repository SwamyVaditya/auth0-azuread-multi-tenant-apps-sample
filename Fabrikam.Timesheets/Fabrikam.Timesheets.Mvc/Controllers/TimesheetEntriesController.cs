using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Fabrikam.Timesheets.Data;
using Microsoft.AspNet.Identity;

namespace Fabrikam.Timesheets.Mvc.Controllers
{
    [RoutePrefix("entries")]
    public class TimesheetEntriesController : Controller
    {
        private readonly TimesheetContext _db = new TimesheetContext();

        private IQueryable<TimesheetEntry> MyEntries
        {
            get
            {
                var userId = new Guid(User.Identity.GetUserId());
                return _db.TimesheetEntries.Where(e => e.UserId == userId);
            }
        }
            
            
        [Route("")]
        public async Task<ActionResult> Index()
        {
            return View(await MyEntries.ToListAsync());
        }

        [Route("details/{id:guid}")]
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var timesheetEntry = await MyEntries.FirstOrDefaultAsync(e => e.Id == id);
            if (timesheetEntry == null)
            {
                return HttpNotFound();
            }
            return View(timesheetEntry);
        }

        [Route("create")]
        public ActionResult Create()
        {
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Date,Title")] TimesheetEntry entry)
        {
            if (ModelState.IsValid)
            {
                entry.Id = Guid.NewGuid();
                entry.CreatedOn = DateTime.UtcNow;
                entry.UserId = new Guid(User.Identity.GetUserId());

                _db.TimesheetEntries.Add(entry);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(entry);
        }

        [Route("edit/{id:guid}")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var timesheetEntry = await MyEntries.FirstOrDefaultAsync(e => e.Id == id);
            if (timesheetEntry == null)
            {
                return HttpNotFound();
            }

            return View(timesheetEntry);
        }

        [Route("edit/{id:guid}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Date,Title")] TimesheetEntry timesheetEntry)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(timesheetEntry).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(timesheetEntry);
        }

        [Route("delete/{id:guid}")]
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var timesheetEntry = await MyEntries.FirstOrDefaultAsync(e => e.Id == id);
            if (timesheetEntry == null)
            {
                return HttpNotFound();
            }
            return View(timesheetEntry);
        }

        [Route("delete/{id:guid}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            var timesheetEntry = await MyEntries.FirstOrDefaultAsync(e => e.Id == id);
            _db.TimesheetEntries.Remove(timesheetEntry);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
