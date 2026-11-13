namespace RaceDay.MVC.Models
{
    public class UserSession
    {
        public required string userID {get; set;}

        public required string userName {get; set;}

        public required string userSurname{get; set;}

        public required string Email{get; set;}

        public required string Role{get; set;}

        public required string Token{get; set;}
    }
}