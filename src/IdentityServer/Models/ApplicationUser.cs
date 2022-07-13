using Microsoft.AspNetCore.Identity;

namespace Sample.IdentityServer.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Oid { get; set; }
    }
}
