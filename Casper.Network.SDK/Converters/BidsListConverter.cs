using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.Converters
{
    public class BidKindsListConverter : JsonConverter<List<BidKind>>
    {
        const ulong DefaultMinimumDelegationAmount = 500l * 1_000_000_000l;
        const ulong DefaultMaximumDelegationAmount = 1_000_000_000 * 1_000_000_000l;
        
        public override List<BidKind> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var bids = new List<BidKind>();

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("StartArray token expected to deserialize a list of Bids");

            reader.Read(); // Start array

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                reader.Read();

                string publicKey = null;
                BidKind bid = null;

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var property = reader.GetString();
                    reader.Read();
                    switch (property)
                    {
                        case "public_key":
                            publicKey = reader.GetString();
                            reader.Read();
                            break;
                        case "bid":
                            try
                            {
                                using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                                {
                                    if (document.RootElement.TryGetProperty("bonding_purse", out var bondingPurse))
                                    {
                                        var unifiedBid =
                                            JsonSerializer.Deserialize<Bid>(document.RootElement.GetRawText());
                                        bid = new BidKind()
                                        {
                                            Unified = unifiedBid,
                                        };
                                    }
                                    else
                                    {
                                        bid = JsonSerializer.Deserialize<BidKind>(document.RootElement.GetRawText());
                                    }

                                    reader.Read(); // read end of object
                                }
                            }
                            catch (Exception e)
                            {
                                throw new JsonException(e.Message);
                            }

                            break;
                        default:
                            throw new JsonException($"Unexpected property '{property}' deserializing Bid");
                    }
                }

                if (bid?.Unified != null)
                {
                    // Convert Unified bids to 1 Validator BidKind and 1 Delegator BidKind per delegator included
                    bids.Add(new BidKind()
                    {
                        Validator = new ValidatorBid()
                        {
                            PublicKey = bid.Unified.PublicKey ?? PublicKey.FromHexString(publicKey),
                            BondingPurse = bid.Unified.BondingPurse,
                            DelegationRate = bid.Unified.DelegationRate,
                            StakedAmount = bid.Unified.StakedAmount,
                            MinimumDelegationAmount = DefaultMinimumDelegationAmount,
                            MaximumDelegationAmount = DefaultMaximumDelegationAmount,
                            ReservedSlots = 0,
                            Inactive = bid.Unified.Inactive,
                        }
                    });

                    foreach (var delegator in bid.Unified.Delegators)
                    {
                        bids.Add(new BidKind()
                        {
                            Delegator = new DelegatorBid()
                            {
                                VestingSchedule = delegator.VestingSchedule,
                                ValidatorPublicKey = delegator.ValidatorPublicKey,
                                StakedAmount = delegator.StakedAmount,
                                BondingPurse = delegator.BondingPurse,
                                DelegatorKind = new DelegatorKind() { PublicKey = delegator.DelegatorPublicKey },
                            }
                        });
                    }
                }
                else 
                    bids.Add(bid);

                if (reader.TokenType != JsonTokenType.EndObject)
                    throw new JsonException("End object token expected while deserializing a list of Bids");
                reader.Read(); // end object
            }

            return bids;
        }

        public override void Write(
            Utf8JsonWriter writer,
            List<BidKind> value,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException("Write method for Bid not yet implemented");
        }
    }
}