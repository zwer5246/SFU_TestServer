using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SFU_MainCluster.Domain
{
    public class ServerDbContext : IdentityDbContext<IdentityUser>
    {
        public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) { }
    }
}
