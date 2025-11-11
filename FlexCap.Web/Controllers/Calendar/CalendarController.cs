using FlexCap.Web.Data;
using FlexCap.Web.Models.Calendar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace FlexCap.Web.Controllers
{
    public class CalendarController : Controller
    {
        private readonly AppDbContext _context;

        public CalendarController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Rh()
        {
            var events = await _context.CalendarEvents.ToListAsync();
            return View("~/Views/Calendar/Rh.cshtml", events);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.CalendarEvents.ToListAsync();
            return Json(events);
        }

        // CreateEvent no controller
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CalendarModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.CalendarEvents.Add(model);
            await _context.SaveChangesAsync();
            return Json(model); // importante: retorna JSON com id, date, title, color
        }

        [HttpDelete]
public async Task<IActionResult> DeleteEvent(int id)
{
    var ev = await _context.CalendarEvents.FindAsync(id);
    if (ev == null)
        return NotFound();

    _context.CalendarEvents.Remove(ev);
    await _context.SaveChangesAsync();
    return Ok();
}

    }
}
