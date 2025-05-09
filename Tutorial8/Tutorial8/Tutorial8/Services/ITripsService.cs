using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<int> AddClient(ClientDTO client);

    
    Task<bool> RegisterClientForTrip(int clientId, int tripId);

    Task<bool> DeleteClientTrip(int clientId, int tripId);
    Task<List<TripDTO>> GetTrips(int id);

}