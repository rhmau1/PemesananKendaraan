using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PemesananKendaraan.Models;
using monitorKendaraan.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OfficeOpenXml;

namespace PemesananKendaraan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public BookingsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBooking()
        {
            return await _context.Booking.ToListAsync();
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(Guid id)
        {
            var booking = await _context.Booking.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        // PUT: api/Bookings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Consumes("application/x-www-form-urlencoded")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PutBooking(Guid id, DateTime start_booking, DateTime end_booking)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound("booking not found");
            }
            string? username = User.FindFirstValue(ClaimTypes.Name);
            if (username == null)
            {
                return Unauthorized("Unauthorized access, username not found in token");
            }
            var user = await _context.User.SingleOrDefaultAsync(u => u.username == username);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.user_id != booking.user_id)
            {
                return Unauthorized("Unauthorized access, admin id not match");
            }
            if (start_booking > end_booking)
            {
                return BadRequest("Start date must be earlier than end date");
            }
            booking.start_booking = start_booking;
            booking.end_booking = end_booking;

            var vehicle = await _context.Vehicle.FindAsync(booking.vehicle_id);
            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }
            TimeSpan bookingDuration = end_booking - start_booking;
            int totalDays = (int)bookingDuration.TotalDays + 1;
            int totalPrice = totalDays * vehicle.rental_price_per_day;

            booking.total_price = totalPrice;
            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            var bookingResponseDTO = new BookingResponseDTO
            {
                booking_id = booking.booking_id,
                user_id = booking.user_id,
                vehicle_id = booking.vehicle_id,
                driver_id = booking.driver_id,
                start_booking = booking.start_booking,
                end_booking = booking.end_booking,
                created_at = booking.created_at,
                total_price = booking.total_price,
                status = booking.status
            };
            return Ok(bookingResponseDTO);
        }

        // POST: api/Bookings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Booking>> PostBooking([FromBody]BookingDTO createBookingDTO)
        {
            bool checkDriver = await _context.Driver.AnyAsync(d => d.driver_id == createBookingDTO.driver_id && d.is_available == true);
            if (!checkDriver)
            {
                return BadRequest("Driver is either not found or not available");
            }

            var vehicle = await _context.Vehicle.SingleOrDefaultAsync(v => v.vehicle_id == createBookingDTO.vehicle_id && v.is_available == true);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }
            string? username = User.FindFirstValue(ClaimTypes.Name);
            if (username == null)
            {
                return Unauthorized("Unauthorized access, username not found in token");
            }
            var user = await _context.User.SingleOrDefaultAsync(u => u.username == username);
            if (user == null)
            {
                return NotFound("User not found");
            }
            if (createBookingDTO.start_booking > createBookingDTO.end_booking)
            {
                return BadRequest("Start date must be earlier than end date");
            }
            TimeSpan bookingDuration = createBookingDTO.end_booking - createBookingDTO.start_booking;
            int totalDays = (int)bookingDuration.TotalDays + 1;
            int totalPrice = totalDays * vehicle.rental_price_per_day;
            var booking = new Booking
            {
                driver_id = createBookingDTO.driver_id,
                user_id = user.user_id,
                vehicle_id = createBookingDTO.vehicle_id,
                start_booking = createBookingDTO.start_booking,
                end_booking = createBookingDTO.end_booking,
                total_price = totalPrice
            };
            _context.Booking.Add(booking);
            await _context.SaveChangesAsync();

            foreach (var approver in createBookingDTO.Approvers)
            {
                var approval = new Approval
                {
                    booking_id = booking.booking_id,
                    user_id = approver.user_id,
                    approval_level = approver.approval_level
                };
                _context.Approval.Add(approval);
            }
            await _context.SaveChangesAsync();

            var bookingResponseDTO = new BookingResponseDTO
            {
                booking_id = booking.booking_id,
                user_id = booking.user_id,
                vehicle_id = booking.vehicle_id,
                driver_id = booking.driver_id,
                start_booking = booking.start_booking,
                end_booking = booking.end_booking,
                created_at = booking.created_at,
                total_price = booking.total_price,
                status = booking.status
            };
            return CreatedAtAction("GetBooking", new { id = booking.booking_id }, bookingResponseDTO);
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            string? username = User.FindFirstValue(ClaimTypes.Name);
            if (username == null)
            {
                return Unauthorized("Unauthorized access, username not found in token");
            }
            var user = await _context.User.SingleOrDefaultAsync(u => u.username == username);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.user_id != booking.user_id)
            {
                return Unauthorized("Unauthorized access, admin id not match");
            }
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookingExists(Guid id)
        {
            return _context.Booking.Any(e => e.booking_id == id);
        }

        [HttpGet("export-booking-report")]
        public async Task<IActionResult> ExportBookingReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var bookingData = await (from b in _context.Booking
                                     join u in _context.User on b.user_id equals u.user_id
                                     join v in _context.Vehicle on b.vehicle_id equals v.vehicle_id
                                     join d in _context.Driver on b.driver_id equals d.driver_id
                                     join a in _context.Approval on b.booking_id equals a.booking_id
                                     select new
                                     {
                                         booking_id = b.booking_id,
                                         total_price = b.total_price,
                                         admin_name = u.name,
                                         vehicle_type = v.type,
                                         rental_price_per_day = v.rental_price_per_day,
                                         plate_number = v.plate_number,
                                         start_booking = b.start_booking,
                                         end_booking = b.end_booking,
                                         approval_name = a.user.name, 
                                         approval_level = a.approval_level,
                                         is_approved = a.is_approved,
                                         approved_at = a.approved_at,
                                         driver_name = d.name
                                     })
                                     .Where(b => b.start_booking >= startDate && b.end_booking <= endDate)
                                     .ToListAsync();


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Booking Report");

                worksheet.Cells[1, 1, 1, 1].Merge = true; 
                worksheet.Cells[1, 1].Value = "Admin";
                worksheet.Cells[2, 1].Value = "Admin Name";

                worksheet.Cells[1, 2, 1, 4].Merge = true;
                worksheet.Cells[1, 2].Value = "Booking";
                worksheet.Cells[2, 2].Value = "Booking ID";
                worksheet.Cells[2, 3].Value = "Start Booking";
                worksheet.Cells[2, 4].Value = "End Booking";

                worksheet.Cells[1, 5, 1, 7].Merge = true; 
                worksheet.Cells[1, 5].Value = "Vehicle";
                worksheet.Cells[2, 5].Value = "Type";
                worksheet.Cells[2, 6].Value = "Plate Number";
                worksheet.Cells[2, 7].Value = "Rental Price Per Day";

                worksheet.Cells[1, 8, 1, 11].Merge = true; 
                worksheet.Cells[1, 8].Value = "Approval";
                worksheet.Cells[2, 8].Value = "Approval Name";
                worksheet.Cells[2, 9].Value = "Approval Level";
                worksheet.Cells[2, 10].Value = "Is Approved";
                worksheet.Cells[2, 11].Value = "Approved At";

                worksheet.Cells[1, 12, 1, 12].Merge = true; 
                worksheet.Cells[1, 12].Value = "Driver";
                worksheet.Cells[2, 12].Value = "Driver Name";

                worksheet.Cells[1, 13, 1, 13].Merge = true; 
                worksheet.Cells[1, 13].Value = "Total Price";
                worksheet.Cells[2, 13].Value = "Total Price";

                for (int i = 0; i < bookingData.Count; i++)
                {
                    var booking = bookingData[i];
                    worksheet.Cells[i + 3, 1].Value = booking.admin_name; 
                    worksheet.Cells[i + 3, 2].Value = booking.booking_id;
                    worksheet.Cells[i + 3, 3].Value = booking.start_booking.ToString("yyyy-MM-dd"); 
                    worksheet.Cells[i + 3, 4].Value = booking.end_booking.ToString("yyyy-MM-dd"); 
                    worksheet.Cells[i + 3, 5].Value = booking.vehicle_type; 
                    worksheet.Cells[i + 3, 6].Value = booking.plate_number; 
                    worksheet.Cells[i + 3, 7].Value = booking.rental_price_per_day; 
                    worksheet.Cells[i + 3, 8].Value = booking.approval_name; 
                    worksheet.Cells[i + 3, 9].Value = booking.approval_level; 
                    worksheet.Cells[i + 3, 10].Value = booking.is_approved ? "Yes" : "No"; 
                    worksheet.Cells[i + 3, 11].Value = booking.approved_at.ToString("yyyy-MM-dd"); 
                    worksheet.Cells[i + 3, 12].Value = booking.driver_name; 
                    worksheet.Cells[i + 3, 13].Value = booking.total_price; 
                }
                worksheet.Cells.AutoFitColumns();
                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"BookingReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx");
                }
            }
        }
    }
}
