using HtmlAgilityPack;

namespace SneakerWebAPI.Services.CardService
{
    public class CardService : ICardService
    {
        private readonly HttpClient _httpclient;

        public CardService(HttpClient httpclient)
        {
            _httpclient = httpclient;
        }

        public float GetPrice(string a)
        {

            string source = a;
            // card.ResellURL;
            var response = _httpclient.GetAsync(source).Result;
            string card_site = response.Content.ReadAsStringAsync().Result;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(card_site);

            string pricefinder = "//span[@id='sale-price']";
            HtmlNode scriptNode = document.DocumentNode.SelectSingleNode(pricefinder);

            if (scriptNode != null)
            {

                var price = scriptNode.InnerText;
                float cost = float.Parse(price);
                Console.WriteLine(price);
                // Parse the JSON to a JObject
                return cost;
            }
            else
            {


                return 0;

            }
        }
    }
}
