using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models{
    public class RegisterViewModel
    {
        [Required]
        public string Name{get; set;}

        [Required]
        public string Surname {get; set;}

        [Required]
        [EmailAddress]
        public string email{get; set;}

        [Required]
        [DataType(DataType.Password)]
        public string Password{get; set;}

        [Required]
        [Phone]
        public string PhoneNumber{get; set;}

        [Required]
        public string Role{get; set;}
    }
}