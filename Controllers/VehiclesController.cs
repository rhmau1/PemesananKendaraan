using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PemesananKendaraan.Models;
using monitorKendaraan.Models;

namespace PemesananKendaraan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public VehiclesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Vehicles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicle()
        {
            return await _context.Vehicle.ToListAsync();
        }

        // GET: api/Vehicles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicle.FindAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle;
        }

        [HttpGet("vehicle-usage")]
        public async Task<IActionResult> GetVehicleUsage([FromQuery] DateTime start_date, [FromQuery] DateTime end_date)
        {
            // Validasi tanggal
            if (start_date > end_date)
            {
                return BadRequest("Start date cannot be later than end date");
            }

            // Ambil data booking yang sesuai dengan rentang waktu dan minimal memiliki 2 approval yang disetujui
            var vehicleUsage = await _context.Booking
                .Where(b => b.start_booking >= start_date && b.end_booking <= end_date)
                .Select(b => new
                {
                    b.vehicle_id,
                    Approvals = _context.Approval
                        .Where(a => a.booking_id == b.booking_id && a.is_approved == true)
                        .ToList()
                })
                .Where(b => b.Approvals.Count >= 2)
                .GroupBy(b => b.vehicle_id)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    TotalBookings = g.Count(),
                    Bookings = g.ToList()
                })
                .ToListAsync();

            return Ok(vehicleUsage);
        }

        // PUT: api/Vehicles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> PutVehicle(int id, Vehicle vehicle)
        {
            if (id != vehicle.vehicle_id)
            {
                return BadRequest();
            }

            _context.Entry(vehicle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Vehicles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult<Vehicle>> PostVehicle([FromForm] VehicleDTO createVehicleDTO)
        {
            var vehicle = new Vehicle
            {
                type = createVehicleDTO.type,
                plate_number = createVehicleDTO.plate_number,
                rental_price_per_day = createVehicleDTO.rental_price_per_day,
                ownership = createVehicleDTO.ownership
            };
            _context.Vehicle.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehicle", new { id = vehicle.vehicle_id }, vehicle);
        }

        // DELETE: api/Vehicles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            _context.Vehicle.Remove(vehicle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicle.Any(e => e.vehicle_id == id);
        }
    }
}
