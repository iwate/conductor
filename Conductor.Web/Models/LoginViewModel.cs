using System.ComponentModel.DataAnnotations;

namespace Conductor.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Password { get; set; }   
    }
}