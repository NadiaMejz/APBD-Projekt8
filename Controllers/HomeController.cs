using Microsoft.AspNetCore.Mvc;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using Cwiczenia7.DTO;
using System.Reflection.Metadata.Ecma335;
namespace Cwiczenia7.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }
       //Celem endpointu jest pobranie listy wszytskich wycieczek jakie mamy zapisane w bazie danych 
        [HttpGet("trips")]
        public IActionResult getTrips() {
            List<object> data = new List<object>();
            using SqlConnection con = GetConnection();
            //Ponizsza kwerenda pobiera dabe z tabeli trips
            using SqlCommand cmd = new SqlCommand(@"SELECT t.IdTrip,t.Name,t.Description, t.DateFrom,t.DateTo, t.MaxPeople,c.Name as CountryName from Trip t join Country_Trip ct on t.IdTrip = ct.IdTrip join Country c on ct.IdCountry = c.IdCountry", con);
            con.Open();
            using SqlDataReader reader= cmd.ExecuteReader();
            while (reader.Read())
            {
                data.Add(new
                {
                    IdTrip = reader["IdTrip"],
                    Name= reader["Name"],
                    Description = reader["Description"],
                    DateFrom= reader["DateFrom"],
                    DateTo = reader["DateTo"],
                    MaxPeople= reader["MaxPeople"],
                    CountryName= reader["CountryName"],

                });

            }
            return Ok(data);
        }
        //Celem endpointu jest pobranie wszystkich wycieczek na ktore zapisany jest klient o podanym numerze IP
        [HttpGet("clients/{id}/trips")]

        public IActionResult getClientTrips(int id) {
            List<object> data = new List<object>();
            using SqlConnection con = GetConnection();
            //ponizsza kwerenda pobiera dane z tabeli trips ale tylko dla podanego klienta (id klienta)
            using SqlCommand cmd = new SqlCommand(@"SELECT t.IdTrip,t.Name,t.Description, t.DateFrom,t.DateTo, t.MaxPeople, ct.RegisteredAt ,c.Name as CountryName
from Trip t 
join Client_Trip ct on t.IdTrip = ct.IdTrip
join Country_Trip ctryt on t.IdTrip= ctryt.IdTrip
join Country c on ctryt.IdCountry = c.IdCountry
where ct.IdClient = @IdClient", con);
          cmd.Parameters.AddWithValue("@IdClient", id);
            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                return NotFound("brak wycieczek lub klient nie istnieje");
            }
            while (reader.Read())
            {
                data.Add(new
                {
                    IdTrip = reader["IdTrip"],
                    Name = reader["Name"],
                    Description = reader["Description"],
                    DateFrom = reader["DateFrom"],
                    DateTo = reader["DateTo"],
                    MaxPeople = reader["MaxPeople"],
                    CountryName = reader["CountryName"],
                    RegisteredAt = reader["RegisteredAt"]
                });

            }
            return Ok(data);
        }
        //Celem endpointu jest dodanie nowego klienta do bazy danych
        [HttpPost("clients")]
        public IActionResult NewClient([FromBody] Client client) {
            if(client==null || string.IsNullOrWhiteSpace(client.first_Name) || string.IsNullOrWhiteSpace(client.last_Name) || string.IsNullOrWhiteSpace(client.email) || string.IsNullOrWhiteSpace(client.pesel)){
                return BadRequest("nie podano wszystkich danych ");
            }
            using SqlConnection sqlConnection = GetConnection();

            //using SqlCommand IdCmd = new SqlCommand(@"select IdClient from Client", sqlConnection);
            //List<int> ids = new List<int>();
            //sqlConnection.Open();
            //using SqlDataReader reader = IdCmd.ExecuteReader();
            //while (reader.Read())
            //{
            //    ids.Add((int)reader["IdClient"]);
            //}
            //int id = ids.Max() + 1;
            //reader.Close();


            //zadaniem ponizszej kwerendy jest dodanie nowego klienta dp tabeli clients 
            using SqlCommand cmd = new SqlCommand(@"insert into Client(FirstName, LastName,Email,Telephone,Pesel) output inserted.IdClient values(@FirstName, @LastName,@Email, @Telephone, @Pesel)", sqlConnection);
           //md.Parameters.AddWithValue("@IdClient", id);
            cmd.Parameters.AddWithValue("@FirstName", client.first_Name);

            cmd.Parameters.AddWithValue("@LastName", client.last_Name);
            cmd.Parameters.AddWithValue("@Email", client.email);
            cmd.Parameters.AddWithValue("@Telephone", client.phone_number);
            
            cmd.Parameters.AddWithValue("@Pesel", client.pesel);
            sqlConnection.Open();
            int Client_Id=(int) cmd.ExecuteScalar();
            return Ok(Client_Id);



        }
        //Celem endpointu jest zarejestrowanie wycieczki do danego uzytkowanika 
        [HttpPut("clients/{cid}/trips/{tid}")]
        public IActionResult RegisterClientTrip(int cid,int tid)
        {
            using SqlConnection sqlConnection= GetConnection();
            sqlConnection.Open();
            //zadaniem kwerendy jest sprawdzenie czy klient o podanym numerze id istnieje w tabeli client
            using SqlCommand checkClient = new SqlCommand(@"select count(1) from Client where IdClient = @Id", sqlConnection);
            checkClient.Parameters.AddWithValue("@Id", cid);
            int clientCount=(int) checkClient.ExecuteScalar();
            if(clientCount<1) {
            return NotFound("Podany klient nie istnieje");  
            }
            //zadaniem kwerendy jest sprawdzenie czy wycieczka o podanym numerze id istnieje w tabeli trip
            using SqlCommand checkTrip = new SqlCommand(@"select count(1) from Trip where IdTrip = @Id", sqlConnection);
            checkTrip.Parameters.AddWithValue("@Id", tid);
            int TripCount = (int)checkTrip.ExecuteScalar();
            if (TripCount < 1)
            {
                return NotFound("Podana wyciieczka nie istnieje");
            }
            //zadaniem kwerendy jest pobranie maksymalnej liczby uczestnikow dla podanej wycieczki 
            using SqlCommand CheckTripLimit = new SqlCommand(@"SELECT MaxPeople from Trip Where IdTrip = @Id", sqlConnection);
            CheckTripLimit.Parameters.AddWithValue("@Id", tid);
            int tripLimit=(int) CheckTripLimit.ExecuteScalar();

            //zadaniem kwerendy jest pobranie liczby zapisanych do wycieczki uczesnikow 
            using SqlCommand CheckTripOccupation = new SqlCommand(@"select count(*) from Client_Trip where IdTrip=@Id", sqlConnection);
            CheckTripOccupation.Parameters.AddWithValue("@Id", tid);
            int tripOccupation =(int )CheckTripOccupation.ExecuteScalar();

            if (tripOccupation >= tripLimit)
            {
                return BadRequest("Brak wolnych miejsc na wycieczce");

            }
            //zadaniem kwerendy jest dodanie wpisu dp tabeli client_Trip. Wpis ten laczy klienta i wycieczke 
            using SqlCommand addTrip = new SqlCommand(@"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) values (@idc, @idt, @Date)", sqlConnection);
            addTrip.Parameters.AddWithValue("@idc", cid);
            addTrip.Parameters.AddWithValue("@idt", tid);
            addTrip.Parameters.AddWithValue("@Date", int.Parse(DateTime.Now.ToString("yyyyMMdd")));
            addTrip.ExecuteNonQuery();
            return Ok("dodano klienta do wycieczki");
        }
        //celem endpointu jest wyrejestrowanie wycieczki przypisanej do klienta 
        [HttpDelete("clients/{cid}/trips/{tid}")]
        public  IActionResult RemoveClientTrip(int cid,int tid)
        {
            using SqlConnection sqlConnection = GetConnection();
            //zadaniem kwerendy jest usuniecie wpisu laczacego klienta i wycieczke 
            using SqlCommand cmd = new SqlCommand(@"delete from Client_Trip where IdClient = @cid and IdTrip = @tid", sqlConnection);
            cmd.Parameters.AddWithValue("@cid", cid);
            cmd.Parameters.AddWithValue("@tid", tid);
            sqlConnection.Open();
            int deletedCounts = cmd.ExecuteNonQuery();
            if (deletedCounts == 0) {
                return NotFound("Nie znaleziono wycieczki dla klienta");

            }
            else
            {
                return Ok("Wycieczka zostala usunieta");
            }
        }



    }
    
}
