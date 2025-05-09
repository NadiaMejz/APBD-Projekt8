using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Trips;Integrated Security=True;";


    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        const string sql = @"
            SELECT t.IdTrip,
                   t.Name,
                   t.Description,
                   t.DateFrom,
                   t.DateTo,
                   t.MaxPeople,
                   STRING_AGG(c.Name, ', ') AS Countries
            FROM Trip t
            JOIN Country_Trip ct ON t.IdTrip  = ct.IdTrip
            JOIN Country      c  ON ct.IdCountry = c.IdCountry
            GROUP BY t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople;";

        using var conn = new SqlConnection(_connectionString);
        using var cmd  = new SqlCommand(sql, conn);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            trips.Add(MapTrip(reader));

        return trips;
    }

    public async Task<List<TripDTO>> GetTrips(int clientId)
    {
        var trips = new List<TripDTO>();

        const string sql = @"
            SELECT t.IdTrip,
                   t.Name,
                   t.Description,
                   t.DateFrom,
                   t.DateTo,
                   t.MaxPeople,
                   STRING_AGG(c.Name, ', ') AS Countries
            FROM Trip t
            JOIN Client_Trip clt ON clt.IdTrip = t.IdTrip
            JOIN Country_Trip ct ON t.IdTrip   = ct.IdTrip
            JOIN Country      c  ON ct.IdCountry = c.IdCountry
            WHERE clt.IdClient = @cid
            GROUP BY t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople;";

        using var conn = new SqlConnection(_connectionString);
        using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cid", clientId);

        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            trips.Add(MapTrip(reader));

        return trips;
    }

    private static TripDTO MapTrip(SqlDataReader reader)
    {
        return new TripDTO
        {
            Id          = reader.GetInt32(reader.GetOrdinal("IdTrip")),
            Name        = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.GetString(reader.GetOrdinal("Description")),

            DateFrom    = reader["DateFrom"] is DateTime df
                            ? df
                            : DateTime.Parse(reader["DateFrom"].ToString()!),
            DateTo      = reader["DateTo"] is DateTime dt
                            ? dt
                            : DateTime.Parse(reader["DateTo"].ToString()!),

            MaxPeople   = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
            Countries   = reader.GetString(reader.GetOrdinal("Countries"))
                              .Split(", ")
                              .Select(n => new CountryDTO { Name = n })
                              .ToList()
        };
    }
    public async Task<int> AddClient(ClientDTO client)
    {
        int newClientId;

        string command = @"
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
        OUTPUT INSERTED.IdClient
        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
    ";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);

            await conn.OpenAsync();
            newClientId = (int)await cmd.ExecuteScalarAsync();
           
        }

        return newClientId;
    }
    public async Task<bool> RegisterClientForTrip(int clientId, int tripId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var checkClient = new SqlCommand(
            "SELECT 1 FROM Client WHERE IdClient = @cid", conn);
        checkClient.Parameters.AddWithValue("@cid", clientId);
        if (await checkClient.ExecuteScalarAsync() is null)
            return false;

        var checkTrip = new SqlCommand(
            "SELECT MaxPeople FROM Trip WHERE IdTrip = @tid", conn);
        checkTrip.Parameters.AddWithValue("@tid", tripId);
        var maxPeopleObj = await checkTrip.ExecuteScalarAsync();
        if (maxPeopleObj is null)
            return false;
        int maxPeople = (int)maxPeopleObj;

        var dup = new SqlCommand(
            "SELECT 1 FROM Client_Trip WHERE IdClient=@cid AND IdTrip=@tid", conn);
        dup.Parameters.AddWithValue("@cid", clientId);
        dup.Parameters.AddWithValue("@tid", tripId);
        if (await dup.ExecuteScalarAsync() is not null)
            return false;

        var cnt = new SqlCommand(
            "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip=@tid", conn);
        cnt.Parameters.AddWithValue("@tid", tripId);
        int current = (int)await cnt.ExecuteScalarAsync();
        if (current >= maxPeople)
            return false;

        var insert = new SqlCommand(
            "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) " +
            "VALUES (@cid, @tid, CONVERT(int, GETDATE()))", conn);
        insert.Parameters.AddWithValue("@cid", clientId);
        insert.Parameters.AddWithValue("@tid", tripId);

        return await insert.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteClientTrip(int clientId, int tripId)
    {
        string command = @"
        DELETE FROM Client_Trip 
        WHERE IdClient = @cid AND IdTrip = @tid;
        ";

        using SqlConnection sqlConnection = GetConnection();
        using SqlCommand cmd = new SqlCommand(command, sqlConnection);

        cmd.Parameters.AddWithValue("@cid", clientId);
        cmd.Parameters.AddWithValue("@tid", tripId);

        await sqlConnection.OpenAsync();
        int deletedCounts = await cmd.ExecuteNonQueryAsync();

        return deletedCounts > 0;
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }



    
}

         