using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class LogInDto
    {
        [Required]
        public string userName {get; set;}

        [Required]
        public string password {get; set;}
    }
}