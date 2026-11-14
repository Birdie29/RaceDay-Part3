using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.MVC.Models
{
    public sealed class CategoryViewModel
    {
        [Required]
        [StringLength(50)]
        [JsonPropertyName("categoryID")]
        public string CategoryId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.1", "1000")]
        [JsonPropertyName("categoryDistance")]
        public decimal CategoryDistance { get; set; }

        [Range(typeof(decimal), "0", "100000")]
        [JsonPropertyName("entryFee")]
        public decimal EntryFee { get; set; }

        [Range(1, int.MaxValue)]
        [JsonPropertyName("eventID")]
        public int EventId { get; set; }
    }

    public sealed class RegistrationViewModel
    {
        [JsonPropertyName("registrationID")]
        public int RegistrationId { get; set; }

        [JsonPropertyName("registrationDate")]
        public DateTime RegistrationDate { get; set; }

        [JsonPropertyName("registrationChannel")]
        public string RegistrationChannel { get; set; } = string.Empty;

        [JsonPropertyName("participantID")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("eventID")]
        public int EventId { get; set; }

        [JsonPropertyName("categoryID")]
        public int CategoryId { get; set; }
    }

    public sealed class ResultViewModel
    {
        [JsonPropertyName("resultID")]
        public int ResultId { get; set; }

        [Range(1, int.MaxValue)]
        [JsonPropertyName("position")]
        public int Position { get; set; }

        [Required]
        [StringLength(100)]
        [JsonPropertyName("participantStatus")]
        public string ParticipantStatus { get; set; } = string.Empty;

        [JsonPropertyName("finishTime")]
        public TimeOnly FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        [JsonPropertyName("registrationID")]
        public int RegistrationId { get; set; }
    }

    public sealed class OrganiserProfileViewModel
    {
        [JsonPropertyName("organiserID")]
        public int OrganiserId { get; set; }

        [Required]
        [StringLength(100)]
        [JsonPropertyName("organiserName")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [JsonPropertyName("organiserSurname")]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [MinLength(8)]
        [JsonPropertyName("password")]
        public string? NewPassword { get; set; }
    }
}
