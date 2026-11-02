using System.ComponentModel.DataAnnotations;

namespace Part1.Models
{
    public class ProjectUpdate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime DatePosted { get; set; } = DateTime.Now;
        public string PostedBy { get; set; }
    }
}