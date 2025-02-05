using System.Collections.Generic;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Converters;

namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// Data structure summarizing auction contract data.
    /// </summary>
    public class AuctionState
    {
        /// <summary>
        /// All bids contained within a vector.
        /// </summary>
        [JsonPropertyName("bids")]
        [JsonConverter(typeof(BidKindsListConverter))]
        public List<BidKind> Bids { get; init; }
        
        /// <summary>
        /// Block height.
        /// </summary>
        [JsonPropertyName("block_height")]
        public ulong BlockHeight { get; init; }

        /// <summary>
        /// Era validators.
        /// </summary>
        [JsonPropertyName("era_validators")]
        public List<EraValidators> EraValidators { get; init; }

        /// <summary>
        /// Global state hash.
        /// </summary>
        [JsonPropertyName("state_root_hash")]
        public string StateRootHash { get; init; }
    }
}