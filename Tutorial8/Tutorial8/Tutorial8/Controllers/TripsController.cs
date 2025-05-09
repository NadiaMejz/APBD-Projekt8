using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")] 
    [ApiController] 
    public class
        TripsController : ControllerBase
    {
        private readonly ITripsService
            _tripsService; 

        public TripsController(ITripsService tripsService)
        {
            _tripsService =
                tripsService;         }

        [HttpGet("/api/trips")]
        public async Task<IActionResult> GetTrips() 
        {

            var trips = await _tripsService.GetTrips();
            
            return Ok(trips);
           
        }


        [HttpGet("/api/clients/{id}/trips")] 
        public async Task<IActionResult> GetTrip(int id) 
        {
            var trips = await _tripsService.GetTrips(id);
            if (trips == null || trips.Count() == 0)
            {
                return NotFound("Klient nie istnieje lub nie ma podpietych wycieczek ");
            }
            return Ok(trips);
        }

        [HttpPost("/api/clients")]
        public async Task<IActionResult> AddClient([FromBody] ClientDTO client)
        {
            if (!ModelState
                    .IsValid) 
            {
                return BadRequest(ModelState);
            }

            try
            {
                int newClientId = await _tripsService.AddClient(client);

                return CreatedAtAction(nameof(AddClient), new { id = newClientId }, newClientId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Coś się zepsuło: {ex.Message}");
            }
        }

        [HttpPut("/api/clients/{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            bool success = await _tripsService.RegisterClientForTrip(id, tripId);

            if (!success)
                return BadRequest("Nie udało się zarejestrować klienta na wycieczkę. Sprawdź dane.");

            return Ok("Klient zarejestrowany na wycieczkę!");
        }
        
        [HttpDelete("/api/clients/{id}/trips/{tid}")]
        public async Task<IActionResult> RemoveClientTrip(int id, int tid)
        {
            bool isDeleted = await _tripsService.DeleteClientTrip(id, tid);
            
            if (!isDeleted)
                return NotFound($"Rejestracja klienta {id} na wycieczkę {tid} nie istnieje.");

            return Ok($"Rejestracja klienta {id} na wycieczkę {tid} została usunięta.");
        }

           
    }
}
