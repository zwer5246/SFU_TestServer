using System.ComponentModel.DataAnnotations;

namespace SFU_MainCluster.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Login must be declared")]
        public required string Login { get; set; }

        [Required(ErrorMessage = "Password must be declared")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Remember login data")]
        public bool RememberMe { get; set; }

        [Display(Name = "Enter as guest")] 
        public bool IsGuest { get; set; } = false;
        
        public string? ReturnUrl { get; set; }
    }
}