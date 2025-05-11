using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")] //ustawia jak ma wyglądać adres do kontrolera
    [ApiController] //mówi że ten kontroler ma działać jak API
    public class
        TripsController : ControllerBase //ControllerBase - daje metody jak Ok(), NotFound() itp. do zwracania odpowiedzi
    {
        private readonly ITripsService
            _tripsService; //ta zmienna służy tylko do wywołania metody przypisanej do enpointa np. GET - getTrips(), PUT - registerTrip() itp ta zmienna _tripsService to jak pilot do telewizora
        // sama w sobie nic nie robi tylko daje ci dostęp do metod które faktycznie coś ogarniają
        // kontroler używa tego pilota żeby odpalić konkretną akcję - np. GetTrips(), RegisterTrip() 

        public TripsController(ITripsService tripsService)
        {
            _tripsService =
                tripsService; //przypisujemy do zmiennej obiektu obiekt ktory jest serwisem który ogarnia operacje na wycieczkach czyli pp przypsiujemy poprzedniej zmiennej obiekt(serwis) z ktorego metod ma korzystac 
        }

        [HttpGet("/api/trips")]//mówi że ta konkretna metoda w kontrolerze odpowiada na żądanie GET
        public async Task<IActionResult> GetTrips() //Metoda, która zwraca wszystkie wycieczki
        {
            /*
             * Task- pozwala aplikacji nie blokować się w trakcie czekania na dane (np. z bazy albo API)
               w c# Task to jakbyś zamówił pizzę przez telefon
               dzwonisz i zamawiasz (wywołujesz metodę)
               gość z pizzerii mówi: "dobra, będzie za 30 minut"
               w tym czasie nie czekasz jak głupek tylko robisz coś innego (np. oglądasz serial)
               kiedy pizza przyjedzie, dostajesz to co zamówiłeś (czyli wynik)
               Task w c# to właśnie taka obietnica wyniku w przyszłości
               Task<IActionResult> - obiecuje że kiedyś zwróci odpowiedź HTTP (np. Ok(), NotFound())
               w praktyce to znaczy że metoda na razie zwraca "pusty talerz" ale potem przyniesie pełną pizzę (czyli wynik)
             */

            var trips = await _tripsService.GetTrips();
            //async - oznacza że metoda jest asynchroniczna (czyli może czekać na coś)
            //await - mówi "poczekaj na wynik tej operacji" (ale nie blokuj całego programu)
            return Ok(trips); //Ok() - standardowa odpowiedź HTTP 200 (sukces)
            //oznacza że oprócz kodu 200 zwracasz jeszcze dane w formacie JSON
        }


        [HttpGet("/api/clients/{id}/trips")] // Oznacza, że metoda odpowiada na żądanie GET z podanym id.
        public async Task<IActionResult> GetTrip(int id) //Metoda, która zwraca jedną wycieczkę na podstawie ID.
        {
            var trips = await _tripsService.GetTrips(id);
            if (trips == null || trips.Count() == 0)
            {
                return NotFound("Klient nie istnieje lub nie ma podpietych wycieczek ");
            }
            return Ok();
        }

        [HttpPost("/api/clients")]
        public async Task<IActionResult> AddClient([FromBody] ClientDTO client)
        {
            // Sprawdza czy dane są poprawne
            if (!ModelState
                    .IsValid) //ModelState to taki strażnik w kontrolerze który pilnuje czy dane które dostajesz są zgodne z tym czego oczekujesz
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Wywołuje metodę serwisu żeby dodać klienta
                int newClientId = await _tripsService.AddClient(client);

                // Jak wszystko poszło git, zwróć ID nowego klienta
                return CreatedAtAction(nameof(AddClient), new { id = newClientId }, newClientId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Coś się zepsuło: {ex.Message}");
            }
        }

        [HttpPut("api/clients/{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            bool success = await _tripsService.RegisterClientForTrip(id, tripId);

            if (!success)
                return BadRequest("Nie udało się zarejestrować klienta na wycieczkę. Sprawdź dane.");

            return Ok("Klient zarejestrowany na wycieczkę!");
        }


        [HttpDelete("clients/{id}/trips/{tid}")]
        public async Task<IActionResult> RemoveClientTrip(int id, int tid)
        {
            bool isDeleted = await _tripsService.DeleteClientTrip(id, tid);

            if (!isDeleted)
                return NotFound($"Rejestracja klienta {id} na wycieczkę {tid} nie istnieje.");

            return Ok($"Rejestracja klienta {id} na wycieczkę {tid} została usunięta.");
        }

           
    }
}

/*
TripsController to taki ziomek który mówi:

   "ej jak ktoś zapuka do drzwi (endpoint), to ja wiem co z tym zrobić"
   sam nie robi całej roboty, tylko woła odpowiednią metodę z serwisu

   ModelState sprawdza:
   czy wszystkie wymagane pola są podane
   czy dane mają właściwy format (np. email wygląda jak email)

   */