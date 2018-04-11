namespace TrumpBot.Models
{
    public class AlabamaElectionConfigModel
    {
        // For the Alabama special election 2017
        // They look like numbers but they're represented as strings in the API
        // don't want to get burned if something changes
        public string RoyMooreId { get; set; } = "3821";
        public string DougJonesId { get; set; } = "3610";
        public string WriteInId { get; set; } = "100008";
        public string PoliticoUri { get; set; } =
            "https://www.politico.com/interactives/elections/2017/alabama/special-election/dec-12/results.json";
        public string LastUpdatedUri { get; set; } =
            "https://www.politico.com/interactives/elections/2017/alabama/special-election/dec-12/last-updated.json";
    }
}