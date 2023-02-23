using System.ComponentModel.DataAnnotations;

namespace ChummerHub.Services.JwT
{
    public class AuthenticateRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
