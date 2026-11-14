namespace RaceDay.MVC.Models
{
    public class Participant
    {
        public int participantionID { get; set; }
        public string participationName { get; set; } = string.Empty;
        public string participationSurname { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string passwordHash {get; set; } = string.Empty;
    }
}