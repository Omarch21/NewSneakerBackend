namespace SneakerWebAPI.Models
{
    public abstract class ItemPrice
    {
        public int Id { get; set; }
        public DateTime Date { set; get; }
        public float Price { set; get; }
    }
}
