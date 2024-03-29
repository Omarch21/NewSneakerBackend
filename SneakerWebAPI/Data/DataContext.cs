using Microsoft.EntityFrameworkCore;

namespace SneakerWebAPI.Data
{
    public class DataContext :  DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options) { }
        public DbSet<Sneaker> Sneakers => Set<Sneaker>();
        public DbSet<SnkrPriceHistory> SnkrPrices => Set<SnkrPriceHistory>();
        public DbSet<User> users => Set<User>();

        public DbSet<Card> Cards => Set<Card>();


    }
}
