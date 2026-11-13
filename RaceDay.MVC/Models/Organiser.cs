namespace RaceDay.MVC.Models
{
    public class Organiser
    {
        public int organiserID { get; set; }
        public string organiserName { get; set; } = string.Empty;
        public string organiserSurname { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
    }
}