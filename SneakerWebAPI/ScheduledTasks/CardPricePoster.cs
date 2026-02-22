using Quartz;
using SneakerWebAPI.Services.CardService;
using SneakerWebAPI.Services.SneakerService;

namespace SneakerWebAPI.ScheduledTasks
{
    public class CardPricePoster : IJob
    {
        private readonly ICardService _cardService;
        public CardPricePoster(ICardService cardService)
        {
            _cardService = cardService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                //PerformInitialPlaywrightInstall();
                await _cardService.PostCardPrices();
                Console.WriteLine("Prices for cards have been posted");
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
