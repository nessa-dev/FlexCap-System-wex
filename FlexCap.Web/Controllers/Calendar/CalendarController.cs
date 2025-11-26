using FlexCap.Web.Data;
using FlexCap.Web.Models.Calendar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FlexCap.Web.Controllers
{
    public class CalendarController : Controller
    {
        private readonly AppDbContext _context;

        public CalendarController(AppDbContext context)
        {
            _context = context;
        }

        // Action RH (Visualização + Edição)
        public async Task<IActionResult> Rh()
        {
            var events = await _context.CalendarEvents.ToListAsync();
            return View("~/Views/Calendar/Rh.cshtml", events);
        }

        // GET: Retorna todos os eventos para o FullCalendar
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.CalendarEvents.ToListAsync();
            return Json(events);
        }

        // POST: Cria um novo evento
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CalendarModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            model.Date = model.Date.Date; 
            _context.CalendarEvents.Add(model);
            await _context.SaveChangesAsync();

            return Json(new
            {
                id = model.Id,
                title = model.Title,
                date = model.Date.ToString("yyyy-MM-dd"),
                type = model.Type,
                color = model.Color
            });
        }

        // Exclui um evento 
        [HttpDelete("Calendar/DeleteEvent/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.CalendarEvents.FindAsync(id);
            if (ev == null)
                return NotFound();

            _context.CalendarEvents.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }

        // Action Manager (Somente Visualização)
        public async Task<IActionResult> Manager()
        {
            var events = await _context.CalendarEvents.ToListAsync();
            return View("~/Views/Calendar/Manager.cshtml", events);
        }

        // Action Colaborador (Somente Visualização)
        public async Task<IActionResult> Colaborador()
        {
            var events = await _context.CalendarEvents.ToListAsync();
            return View("~/Views/Calendar/Colaborador.cshtml", events);
        }

        [HttpGet]
        public async Task<IActionResult> GetThisWeekEvents()
        {
            var now = DateTime.Now.Date;

            DayOfWeek firstDay = DayOfWeek.Sunday;

            int diff = (7 + (now.DayOfWeek - firstDay)) % 7;
            var startDate = now.AddDays(-diff);
            var endDate = startDate.AddDays(7).AddSeconds(-1);

            var events = await _context.CalendarEvents
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderBy(e => e.Date)
                .ToListAsync();

            var formattedEvents = events
                .Select(e => new WeeklyEventViewModel
                {
                    Title = e.Title,
                    DayOfWeekShort = $"({e.Date.ToString("ddd", System.Globalization.CultureInfo.InvariantCulture)})",
                    IconClass = GetIconClassByType(e.Type)
                })
                .Take(4)
                .ToList();

            return Json(formattedEvents);
        }

        private string GetIconClassByType(string type)
        {
            switch (type)
            {
                case "Holiday (Brazil)":
                case "Holiday (EUA)":
                case "Holiday (USA)":
                    return "bi-calendar-check-fill";
                case "Corporate Training":
                    return "bi-lightbulb-fill";
                case "Online Event":
                    return "bi-laptop-fill";
                default:
                    return "bi-calendar-event";
            }
        }
    }

    public class WeeklyEventViewModel
    {
        public string Title { get; set; }
        public string DayOfWeekShort { get; set; }
        public string IconClass { get; set; }
    }
}