using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public sealed class ApiLoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class ApiParticipantRegistrationRequest
    {
        [JsonPropertyName("participantName")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("participantSurname")]
        public string Surname { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class ApiOrganiserRegistrationRequest
    {
        [JsonPropertyName("organiserName")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("organiserSurname")]
        public string Surname { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class ApiAccountResponse
    {
        [JsonPropertyName("participantID")]
        public int? ParticipantId { get; set; }

        [JsonPropertyName("organiserID")]
        public int? OrganiserId { get; set; }

        [JsonPropertyName("participantName")]
        public string? ParticipantName { get; set; }

        [JsonPropertyName("organiserName")]
        public string? OrganiserName { get; set; }

        [JsonPropertyName("participantSurname")]
        public string? ParticipantSurname { get; set; }

        [JsonPropertyName("organiserSurname")]
        public string? OrganiserSurname { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        public int? UserId => ParticipantId ?? OrganiserId;

        public string? UserName => ParticipantName ?? OrganiserName;

        public string? UserSurname =>
            ParticipantSurname ?? OrganiserSurname;
    }

    public sealed class RaceEventViewModel
    {
        [JsonPropertyName("eventID")]
        public int EventId { get; set; }

        [Required]
        [StringLength(120)]
        [JsonPropertyName("eventName")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [JsonPropertyName("eventDescription")]
        public string EventDescription { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [JsonPropertyName("eventDate")]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(200)]
        [JsonPropertyName("eventLocation")]
        public string EventLocation { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.1", "1000")]
        [JsonPropertyName("eventDistance")]
        public decimal EventDistance { get; set; }

        [Required]
        [StringLength(100)]
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("organiserID")]
        public int OrganiserId { get; set; }
    }
}
