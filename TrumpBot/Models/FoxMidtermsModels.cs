using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class FoxMidtermsModels
    {
        public class SenatePrediction
        {
            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; } // You shouldn't use this, it is always set to "This is description..."
            [JsonProperty("n_current_republicans")]
            public int CurrentRepublicansTotal { get; set; }
            [JsonProperty("n_current_democrats")]
            public int CurrentDemocratsTotal { get; set; }
            [JsonProperty("republicans_not_running")]
            public int RepublicansNotRunningTotal { get; set; }
            [JsonProperty("democrats_not_running")]
            public int DemocratsNotRunningTotal { get; set; }
            [JsonProperty("total_number")]
            public int? TotalSeats { get; set; }
            [JsonProperty("helper_text")]
            public string HelperText { get; set; }
            [JsonProperty("special_election")]
            public Dictionary<string, SenateSpecialElection> SpecialElections { get; set; }
            [JsonProperty("states")]
            public Dictionary<string, SenateStatePrediction> States { get; set; }
            
        }

        public class SenateStatePrediction
        {
            [JsonProperty("state_name")]
            public string StateName { get; set; }
            [JsonProperty("state_short_code")]
            public string StateShortCode { get; set; }
            [JsonProperty("fips")]
            public string FipsStateCode { get; set; }
            [JsonProperty("prediction")]
            public string Prediction { get; set; }
            [JsonProperty("title_1")]
            public string Title1 { get; set; }
            [JsonProperty("description_1")]
            public string Description1 { get; set; }
            [JsonProperty("title_2")]
            public string Title2 { get; set; }
            [JsonProperty("description_2")]
            public string Description2 { get; set; }
            [JsonProperty("title_3")]
            public string Title3 { get; set; }
            [JsonProperty("description_3")]
            public string Description3 { get; set; }
            [JsonProperty("title_4")]
            public string Title4 { get; set; }
            [JsonProperty("description_4")]
            public string Description4 { get; set; }
            [JsonProperty("title_5")]
            public string Title5 { get; set; }
            [JsonProperty("description_5")]
            public string Description5 { get; set; }
            [JsonProperty("title_6")]
            public string Title6 { get; set; }
            [JsonProperty("description_6")]
            public string Description6 { get; set; }
            [JsonProperty("title_7")]
            public string Title7 { get; set; }
            [JsonProperty("description_7")]
            public string Description7 { get; set; }
            [JsonProperty("title_8")]
            public string Title8 { get; set; }
            [JsonProperty("description_8")]
            public string Description8 { get; set; }
            
        }

        public class SenateSpecialElection
        {
            [JsonProperty("state_name")]
            public string StateName { get; set; }
            [JsonProperty("fips")]
            public string FipsStateCode { get; set; }
            [JsonProperty("state_short_code")]
            public string StateShortCode { get; set; }
            [JsonProperty("prediction")]
            public string Prediction { get; set; }
        }

        public class HousePrediction
        {
            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; } // Same issue as Senate description property
            [JsonProperty("n_current_democrats")]
            public int CurrentDemocratsTotal { get; set; }
            [JsonProperty("n_current_republicans")]
            public int CurrentRepublicansTotal { get; set; }
            [JsonProperty("n_vacant_seats")]
            public int VacantSeatsTotal { get; set; }
            [JsonProperty("n_needed_for_control")]
            public int SeatsNeededForControl { get; set; }
            [JsonProperty("predictions")]
            /*
             * Values for this one:
             * Likely Republican
             * Leaning Republican
             * Toss Up
             * Leaning Democrat
             * Likely Democrat
             */
            public Dictionary<string, string> Predictions { get; set; }
            
        }

        // They're insanely similar other than some fields for independents
        public class GubernatorialPrediction : SenatePrediction
        {
            [JsonProperty("n_current_independents")]
            public int CurrentIndependentsTotal { get; set; }

            [JsonProperty("independents_not_running")]
            public int? IndependentsNotRunningTotal { get; set; }
        }

        // No idea what any of the numbers mean
        public class ControlOfHousePredictions
        {
            [JsonProperty("democratic_control")]
            public string DemocraticControl { get; set; }
            [JsonProperty("republican_control")]
            public string RepublicanControl { get; set; }
            [JsonProperty("average_text")]
            public string AverageText { get; set; }
            [JsonProperty("number_of_vacancies")]
            public string TotalVacancies { get; set; }
            [JsonProperty("number_of_democrats")]
            public string TotalDemocrats { get; set; }
            [JsonProperty("number_of_republicans")]
            public string TotalRepublicans { get; set; }
            [JsonProperty("date_stamp")]
            public string DateStamp { get; set; }
        }

        public class State
        {
            [JsonProperty("fips")]
            public string FipsStateCode { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("short_code")]
            public string ShortCode { get; set; }
            // "on"
            [JsonProperty("poll_start_enabled")]
            public string PollStartEnabled { get; set; }
            [JsonProperty("poll_start_time")]
            public long PollStartTime { get; set; }
            [JsonProperty("poll_start_time_raw")]
            public string PollStartTimeRaw { get; set; }
            [JsonProperty("poll_closing_enabled")]
            public string PollClosingEnabled { get; set; }
            [JsonProperty("poll_closing_time")]
            public long PollClosingTime { get; set; }
            [JsonProperty("poll_closing_time_raw")]
            public string PollClosingTimeRaw { get; set; }
        }

        public class SenateRace
        {
            [JsonProperty("fips")]
            public string FipsStateCode { get; set; }
            [JsonProperty("state_name")]
            public string StateName { get; set; }
            [JsonProperty("state_short_code")]
            public string StateShortCode { get; set; }
            [JsonProperty("hot_race")]
            public string HotRace { get; set; }
            [JsonProperty("previously_held_by")]
            public string PreviouslyHeldBy { get; set; }
            [JsonProperty("dial_id")]
            public string DialId { get; set; }
            [JsonProperty("candidates")]
            public List<string> Candidates { get; set; }
        }

        public class HouseRace
        {
            [JsonProperty("fips")]
            public string FipsStateCode { get; set; }
            [JsonProperty("state_name")]
            public string StateName { get; set; }
            [JsonProperty("state_short_code")]
            public string StateShortCode { get; set; }
            [JsonProperty("district")]
            public string District { get; set; }
            [JsonProperty("hot_race")]
            public string HotRace { get; set; }
            [JsonProperty("previously_held_by")]
            public string PreviouslyHeldBy { get; set; }
            [JsonProperty("dial_id")]
            public string DialId { get; set; }
            [JsonProperty("candidates")]
            public List<string> Candidates { get; set; }
        }

        public class GubernatorialRace : SenateRace
        {
            
        }

        public class Races
        {
            [JsonProperty("senate")]
            public Dictionary<string, SenateRace> SenateRaces { get; set; }
            [JsonProperty("house")]
            public Dictionary<string, Dictionary<string, HouseRace>> HouseRaces { get; set; }
            [JsonProperty("gubernatorial")]
            public Dictionary<string, GubernatorialRace> GubernatorialRaces { get; set; }
        }

        public class SenateBalanceOfPower
        {
            [JsonProperty("play")]
            public string Play { get; set; }
            [JsonProperty("control")]
            public string Control { get; set; }
            [JsonProperty("rep_tossup")]
            public string RepublicanTossup { get; set; }
            [JsonProperty("dem_tossup")]
            public string DemocratTossup { get; set; }
            [JsonProperty("ind_tossup")]
            public string IndependentTossup { get; set; }
            [JsonProperty("nd_tossup")]
            // Not sure what this one is
            public string NdTossup { get; set; }
        }

        public class BalanceOfPower
        {
            [JsonProperty("senate")]
            public SenateBalanceOfPower Senate { get; set; }
        }
        
        public class FoxData
        {
            [JsonProperty("senate_predictions")]
            public Dictionary<string, SenatePrediction> SenatePredictions { get; set; } 
            [JsonProperty("house_predictions")]
            public Dictionary<string, HousePrediction> HousePredictions { get; set; }
            [JsonProperty("control_of_house_predictions")]
            public ControlOfHousePredictions ControlOfHousePredictions { get; set; }
            [JsonProperty("states")]
            public Dictionary<string, State> States { get; set; }
            [JsonProperty("races")]
            public Races Races { get; set; }
            [JsonProperty("mode")]
            public string Mode { get; set; }
            [JsonProperty("page_reload")]
            public string PageReload { get; set; }
            [JsonProperty("balance_of_power")]
            public BalanceOfPower BalanceOfPower { get; set; }
            [JsonProperty("gubernatorial_predictions")]
            public Dictionary<string, GubernatorialPrediction> GubernatorialPredictions { get; set; }
        }
    }
}