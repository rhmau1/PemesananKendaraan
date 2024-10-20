using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PemesananKendaraan.Models;
using monitorKendaraan.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PemesananKendaraan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly MyDbContext _context;

        public DriversController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Drivers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Driver>>> GetDriver()
        {
            return await _context.Driver.ToListAsync();
        }

        // GET: api/Drivers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Driver>> GetDriver(int id)
        {
            var driver = await _context.Driver.FindAsync(id);

            if (driver == null)
            {
                return NotFound();
            }

            return driver;
        }        

        // POST: api/Drivers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "admin")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult<Driver>> PostDriver([FromForm]DriverDTO createDriverDTO)
        {
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
            var driver = new Driver
            {
                name = createDriverDTO.name,
                phone_number = createDriverDTO.phone_number
            };
            _context.Driver.Add(driver);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDriver", new { id = driver.driver_id }, driver);
        }
        
        private bool DriverExists(int id)
        {
            return _context.Driver.Any(e => e.driver_id == id);
        }
    }
}
