using Microsoft.AspNetCore.Identity;

namespace News_Portal_1._1.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
