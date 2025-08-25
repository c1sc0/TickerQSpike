using Microsoft.EntityFrameworkCore;

namespace TickerQSpike
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
    }
}
