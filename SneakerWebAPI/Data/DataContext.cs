using Microsoft.EntityFrameworkCore;
using SneakerWebAPI.Models;
using SneakerWebAPI.Models.Card;
using SneakerWebAPI.Models.FunkoPop;
using SneakerWebAPI.Models.Game;
using SneakerWebAPI.Models.Sneaker;

namespace SneakerWebAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Sneaker> Sneakers => Set<Sneaker>();
        public DbSet<SneakerPrice> SneakerPrices => Set<SneakerPrice>();
        public DbSet<User> users => Set<User>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<CardPrice> CardPrices => Set<CardPrice>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<GamePrice> GamePrices => Set<GamePrice>();
        public DbSet<FunkoPop> FunkoPops => Set<FunkoPop>();
        public DbSet<FunkoPopPrice> FunkoPopPrices => Set<FunkoPopPrice>();

    }
}
