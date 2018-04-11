using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BlockchairApi.Models
{
    public class BlocksModel
    {
        public class Block
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("hash")]
            public string Hash { get; set; }
            [JsonProperty("date")]
            public DateTime Date { get; set; }
            [JsonProperty("time")]
            public DateTime Time { get; set; }
            [JsonProperty("median_time")]
            public DateTime MedianTime { get; set; } // No I don't know what "median time" means
            [JsonProperty("size")]
            public long Size { get; set; }
            [JsonProperty("size_limit")]
            public long SizeLimit { get; set; }
            [JsonProperty("version")]
            public long Version { get; set; }
            [JsonProperty("version_hex")]
            public string VersionHex { get; set; }
            [JsonProperty("version_bits")]
            public string VersionBits { get; set; }
            [JsonProperty("merkle_root")]
            public string MerkleRoot { get; set; }
            [JsonProperty("nonce")]
            public long Nonce { get; set; }
            [JsonProperty("bits")]
            public long Bits { get; set; }
            [JsonProperty("difficulty")]
            public double Difficulty { get; set; }
            [JsonProperty("chainwork")]
            public string Chainwork { get; set; }
            [JsonProperty("coinbase_data_hex")]
            public string CoinbaseDataHex { get; set; }
            [JsonProperty("transaction_count")]
            public int TransactionCount { get; set; }
            [JsonProperty("input_count")]
            public int InputCount { get; set; }
            [JsonProperty("output_count")]
            public int OutputCount { get; set; }
            [JsonProperty("input_total")]
            public string InputTotal { get; set; }
            [JsonProperty("input_total_usd")]
            public decimal InputTotalUsd { get; set; }
            [JsonProperty("output_total")]
            public string OutputTotal { get; set; }
            [JsonProperty("output_total_usd")]
            public decimal OutputTotalUsd { get; set; }
            [JsonProperty("fee_total")]
            public string FeeTotal { get; set; }
            [JsonProperty("fee_total_usd")]
            public decimal FeeTotalUsd { get; set; }
            [JsonProperty("fee_per_kb")]
            public decimal FeePerKb { get; set; }
            [JsonProperty("fee_per_kb_usd")]
            public decimal FeePerKbUsd { get; set; }
            [JsonProperty("cdd_total")]
            public decimal CddTotal { get; set; }
            [JsonProperty("generation")]
            public string Generation { get; set; }
            [JsonProperty("generation_usd")]
            public decimal GenerationUsd { get; set; }
            [JsonProperty("reward")]
            public string Reward { get; set; }
            [JsonProperty("reward_usd")]
            public decimal RewardUsd { get; set; }
            [JsonProperty("guessed_miner")]
            public string GuessedMiner { get; set; }
            [JsonProperty("coinbase_data_bin")]
            public string CoinbaseDataBin { get; set; }
        }

        public class Statistics
        {
            [JsonProperty("elapsed")]
            public decimal Elapsed { get; set; }
            [JsonProperty("rows_read")]
            public int RowsRead { get; set; }
            [JsonProperty("bytes_read")]
            public long BytesRead { get; set; }
        }

        public class Data
        {
            [JsonProperty("data")]
            public List<Block> Blocks { get; set; }
            [JsonProperty("rows")]
            public int Rows { get; set; }
            [JsonProperty("statistics")]
            public Statistics Statistics { get; set; }
            [JsonProperty("real_rows")]
            public int RealRows { get; set; }
            [JsonProperty("total")]
            public long Total { get; set; }
            [JsonProperty("limit")]
            public int Limit { get; set; }
            [JsonProperty("time")]
            public decimal Time { get; set; }
            [JsonProperty("cache")]
            public int Cache { get; set; }
            [JsonProperty("source")]
            public string Source { get; set; }
        }
    }
}
