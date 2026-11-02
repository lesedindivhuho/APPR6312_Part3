using System.ComponentModel.DataAnnotations;

namespace Part1.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone_Number { get; set; }
        public string role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}