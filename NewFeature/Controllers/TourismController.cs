using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewFeature.Models;
using NewFeature.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TourismController : ControllerBase
    {
        private readonly ITourismService _tourismService;

        public TourismController(ITourismService tourismService)
        {
            _tourismService = tourismService;
        }

        #region Hotel Bookings
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<TourismHotelBookingDto>>> GetHotelBookings()
        {
            var bookings = await _tourismService.GetAllHotelBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet("bookings/{id}")]
        public async Task<ActionResult<TourismHotelBookingDto>> GetHotelBooking(int id)
        {
            var booking = await _tourismService.GetHotelBookingByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpPost("bookings")]
        public async Task<ActionResult<TourismHotelBookingDto>> CreateHotelBooking([FromBody] TourismHotelBookingDto dto)
        {
            var created = await _tourismService.CreateHotelBookingAsync(dto);
            return CreatedAtAction(nameof(GetHotelBooking), new { id = created.Id }, created);
        }

        [HttpPut("bookings/{id}")]
        public async Task<IActionResult> UpdateHotelBooking(int id, [FromBody] TourismHotelBookingDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var result = await _tourismService.UpdateHotelBookingAsync(dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("bookings/{id}")]
        public async Task<IActionResult> DeleteHotelBooking(int id)
        {
            var result = await _tourismService.DeleteHotelBookingAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        #endregion

        #region Sightseeing Tours
        [HttpGet("tours")]
        public async Task<ActionResult<IEnumerable<TourismTourDto>>> GetTours()
        {
            var tours = await _tourismService.GetAllToursAsync();
            return Ok(tours);
        }

        [HttpGet("tours/{id}")]
        public async Task<ActionResult<TourismTourDto>> GetTour(int id)
        {
            var tour = await _tourismService.GetTourByIdAsync(id);
            if (tour == null) return NotFound();
            return Ok(tour);
        }

        [HttpPost("tours")]
        public async Task<ActionResult<TourismTourDto>> CreateTour([FromBody] TourismTourDto dto)
        {
            var created = await _tourismService.CreateTourAsync(dto);
            return CreatedAtAction(nameof(GetTour), new { id = created.Id }, created);
        }

        [HttpPut("tours/{id}")]
        public async Task<IActionResult> UpdateTour(int id, [FromBody] TourismTourDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var result = await _tourismService.UpdateTourAsync(dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("tours/{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var result = await _tourismService.DeleteTourAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        #endregion

        #region KPIs
        [HttpGet("kpis")]
        public async Task<ActionResult<TourismKpisDto>> GetTourismKpis()
        {
            var kpis = await _tourismService.GetTourismKpisAsync();
            return Ok(kpis);
        }
        #endregion
    }
}
