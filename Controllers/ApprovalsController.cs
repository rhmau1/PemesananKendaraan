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

namespace PemesananKendaraan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ApprovalsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Approvals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Approval>>> GetApproval()
        {
            return await _context.Approval.ToListAsync();
        }

        // GET: api/Approvals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Approval>> GetApproval(int id)
        {
            var approval = await _context.Approval.FindAsync(id);

            if (approval == null)
            {
                return NotFound();
            }

            return approval;
        }

        // PUT: api/Approvals/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "approver")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> PutApproval(int id, bool is_approved)
        {
            var approval = await _context.Approval.FindAsync(id);
            if (approval == null)
            {
                return NotFound("Approval data not found");
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

            if(user.user_id != approval.user_id)
            {
                return Unauthorized("Unauthorized access, approver id not match");
            }
            approval.is_approved = is_approved;
            if (is_approved)
            {
                approval.approved_at = DateTime.Now;                
            }
            _context.Entry(approval).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApprovalExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var approvalsForBooking = await _context.Approval.Where(a => a.booking_id == approval.booking_id && a.is_approved == true).ToListAsync();

            // Cek apakah sudah ada minimal 2 approval level yang disetujui
            if (approvalsForBooking.Count >= 2)
            {
                var booking = await _context.Booking.FindAsync(approval.booking_id);
                if (booking == null)
                {
                    return NotFound("Booking not found");
                }

                // Update driver availability
                var driver = await _context.Driver.FindAsync(booking.driver_id);
                if (driver != null)
                {
                    driver.is_available = false;
                    _context.Entry(driver).State = EntityState.Modified;
                }

                // Update vehicle availability
                var vehicle = await _context.Vehicle.FindAsync(booking.vehicle_id);
                if (vehicle != null)
                {
                    vehicle.is_available = false;
                    _context.Entry(vehicle).State = EntityState.Modified;
                }
            }
            await _context.SaveChangesAsync();

            var approvalResponseDTO = new ApprovalResponseDTO
            {
                approval_id = approval.approval_id,
                booking_id = approval.booking_id,
                user_id = approval.user_id,
                approval_level = approval.approval_level,
                is_approved = approval.is_approved,
                approved_at = approval.approved_at
            };

            return Ok(approvalResponseDTO);
        }

        // POST: api/Approvals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Approval>> PostApproval(Approval approval)
        {
            _context.Approval.Add(approval);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetApproval", new { id = approval.approval_id }, approval);
        }

        // DELETE: api/Approvals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApproval(int id)
        {
            var approval = await _context.Approval.FindAsync(id);
            if (approval == null)
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

            if (user.user_id != approval.user_id)
            {
                return Unauthorized("Unauthorized access, approver id not match");
            }
            _context.Approval.Remove(approval);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ApprovalExists(int id)
        {
            return _context.Approval.Any(e => e.approval_id == id);
        }
    }
}
