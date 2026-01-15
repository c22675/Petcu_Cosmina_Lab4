using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Petcu_Cosmina_Lab4.Models.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
         : base(options)
        {
        }
        public DbSet<PredictionHistory> PredictionHistories { get; set; }
    }
}
