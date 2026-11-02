using System.ComponentModel.DataAnnotations;

namespace Part1.Models
{
    public class Donation
    {
        public int Id { get; set; }

        [Range(1, 1000000)]
        public decimal DonationAmount { get; set; }
        public string Currency { get; set; } // ZAR, USD, EUR
        public bool IsRecurring { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.Now;
        public string UserId { get; set; } // Null if anonymous guest
        public string DonorName { get; set; } // For anonymous donation
        public string Email { get; set; } // To send tax certificate
        public string? CertificateNumber { get; set; } // Issued by the GenerateTaxCertificate function

    }

    public class Clothes
    {
        public int Id { get; set; }
        public string Size { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
    }

    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }
    }
}
