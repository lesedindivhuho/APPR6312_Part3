using System.ComponentModel.DataAnnotations;

namespace Part1.Models
{
    public class Volunteer
    {
            public int Id { get; set; }
            public string Fullname { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Skills { get; set; } = string.Empty;
            public string Availability { get; set; } = string.Empty;
            public string Status { get; set; } = "Pending";
            public DateTime ApplicationDate { get; set; } = DateTime.Now;
        }
    }