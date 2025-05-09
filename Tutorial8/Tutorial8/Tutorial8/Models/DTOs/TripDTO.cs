using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class TripDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public string Description { get; set; }
    public List<CountryDTO> Countries { get; set; }
}

public class CountryDTO
{
    public string Name { get; set; }
}

public class ClientDTO
{
    [Required] //pole nie może być puste
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)] //max 50 znaków
    public string LastName { get; set; }

    [Required]
    [EmailAddress] //sprawdza czy email ma dobry format
    public string Email { get; set; }

    [Required]
    [Phone] //sprawdza czy email ma dobry format
    public string Telephone { get; set; }

    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "Pesel musi miec 11 cyfr")] //sprawdza czy PESEL to dokładnie 11 cyfr
    public string Pesel { get; set; }
}

public class Client_TripDTO
{
    public int ClientId { get; set; }
    public int TripId { get; set; }
    public DateTime RegisteredAt { get; set; }
    public int PaymentDate { get; set; }
}

