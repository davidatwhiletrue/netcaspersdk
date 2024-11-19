using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casper.Network.SDK.ByteSerializers;
using Casper.Network.SDK.Converters;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    [JsonConverter(typeof(PayloadFieldsConverter))]
    public class PayloadFields : BaseByteSerializer
    {
        private readonly Dictionary<ushort, byte[]> _fields = new Dictionary<ushort, byte[]>();

        public void AddField(ushort field, byte[] value)
        {
            _fields.Add(field, value);
        }

        public class PayloadFieldsConverter : JsonConverter<PayloadFields>
        {
            public override PayloadFields Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var payload = new PayloadFields();
                try
                {
                    reader.Read(); // skip start object
                    while (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var fieldKey = reader.GetString();
                        if (ushort.TryParse(fieldKey, out var field))
                        {
                            reader.Read();
                            var value = reader.GetString();
                            payload._fields.Add(field, Hex.Decode(value));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }

                return payload;
            }

            const ushort ARGS_MAP_KEY = 0;
            const ushort TARGET_MAP_KEY = 1;
            const ushort ENTRY_POINT_MAP_KEY = 2;
            const ushort SCHEDULING_MAP_KEY = 3;
            
            public override void Write(
                Utf8JsonWriter writer,
                PayloadFields payload,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                if(payload._fields.TryGetValue(ARGS_MAP_KEY, out var argsMapField))
                    writer.WriteString(ARGS_MAP_KEY.ToString(), Hex.ToHexString(argsMapField));
                if(payload._fields.TryGetValue(TARGET_MAP_KEY, out var targetMapField))
                    writer.WriteString(TARGET_MAP_KEY.ToString(), Hex.ToHexString(targetMapField));
                if(payload._fields.TryGetValue(ENTRY_POINT_MAP_KEY, out var entryPointMapField))
                    writer.WriteString(ENTRY_POINT_MAP_KEY.ToString(), Hex.ToHexString(entryPointMapField));
                if(payload._fields.TryGetValue(SCHEDULING_MAP_KEY, out var schedulingMapField))
                    writer.WriteString(SCHEDULING_MAP_KEY.ToString(), Hex.ToHexString(schedulingMapField));
                writer.WriteEndObject();
            }
        }
        public byte[] ToBytes()
        {
            var fields_bytes = new MemoryStream();
            WriteInteger(fields_bytes, _fields.Count);
            foreach (var field in _fields)
            {
                WriteUShort(fields_bytes, field.Key);
                var bytes = field.Value;
                WriteBytes(fields_bytes, bytes);
            }
            return fields_bytes.ToArray();
        }
    }

    internal class TransactionV1PayloadJson
    {
        [JsonPropertyName("initiator_addr")]
        public InitiatorAddr InitiatorAddr { get; set; }
        
        [JsonPropertyName("timestamp")]
        [JsonConverter(typeof(DateTime2EpochConverter))]
        public ulong Timestamp { get; set; }
        
        
        /// <summary>
        /// Duration of the Deploy in milliseconds (from timestamp).
        /// </summary>
        [JsonPropertyName("ttl")]
        [JsonConverter(typeof(HumanizeTTLConverter))]
        public ulong Ttl { get; set; }

        /// <summary>
        /// Pricing mode of a Transaction.
        /// </summary>
        [JsonPropertyName("pricing_mode")]
        [JsonConverter(typeof(PricingMode.PricingModeConverter))]
        public IPricingMode PricingMode { get; set; }

        /// <summary>
        /// Name of the chain where the deploy is executed.
        /// </summary>
        [JsonPropertyName("chain_name")]
        public string ChainName { get; set; }
        
        [JsonPropertyName("fields")]
        [JsonConverter(typeof(PayloadFields.PayloadFieldsConverter))]
        public PayloadFields Fields { get; set; }

        public TransactionV1PayloadJson()
        {
            Fields = new PayloadFields();
        }
    }
    
    /// <summary>
    /// The payload portion of a TransactionV1.
    /// </summary>
    [JsonConverter(typeof(TransactionV1PayloadConverter))]
    public class TransactionV1Payload
    {
        /// <summary>
        /// The address of the initiator of a transaction.
        /// </summary>
        [JsonPropertyName("initiator_addr")]
        public InitiatorAddr InitiatorAddr { get; set; }

        /// <summary>
        /// Timestamp formatted as per RFC 3339 
        /// </summary>
        [JsonPropertyName("timestamp")]
        [JsonConverter(typeof(DateTime2EpochConverter))]
        public ulong Timestamp { get; set; }

        /// <summary>
        /// Duration of the Deploy in milliseconds (from timestamp).
        /// </summary>
        [JsonPropertyName("ttl")]
        [JsonConverter(typeof(HumanizeTTLConverter))]
        public ulong Ttl { get; set; }

        /// <summary>
        /// Pricing mode of a Transaction.
        /// </summary>
        [JsonPropertyName("pricing_mode")]
        [JsonConverter(typeof(PricingMode.PricingModeConverter))]
        public IPricingMode PricingMode { get; set; }

        /// <summary>
        /// Name of the chain where the deploy is executed.
        /// </summary>
        [JsonPropertyName("chain_name")]
        public string ChainName { get; set; }
        
        // [JsonPropertyName("fields")]
        // public PayloadFields Fields { get; set; }
        
        /// <summary>
        /// List of runtime arguments.
        /// </summary>
        [JsonIgnore]
        public List<NamedArg> RuntimeArgs { get; init; }
        
        /// <summary>
        /// Entry point or method of the contract to call.
        /// </summary>
        [JsonIgnore]
        public ITransactionV1EntryPoint EntryPoint { get; init; } 
        
        /// <summary>
        /// Target contract of the transaction (native, custom or session).
        /// </summary>
        [JsonIgnore]
        public ITransactionV1Target Target { get; init; }
        
        /// <summary>
        /// Scheduling of the transaction.
        /// </summary>
        [JsonIgnore]
        public ITransactionV1Scheduling Scheduling { get; init; }

        public TransactionV1Payload()
        {
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow);
            Ttl = 1800000;
        }

        public class TransactionV1PayloadConverter : JsonConverter<TransactionV1Payload>
        {
            public override TransactionV1Payload Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    reader.Read(); // skip start object
                    var payloadJson = JsonSerializer.Deserialize<TransactionV1PayloadJson>(ref reader, options);

                    return new TransactionV1Payload()
                    {
                        InitiatorAddr = payloadJson.InitiatorAddr,
                        Timestamp = payloadJson.Timestamp,
                        Ttl = payloadJson.Ttl,
                        PricingMode = payloadJson.PricingMode,
                        ChainName = payloadJson.ChainName,
                    }; //TODO: Deserialize fields!!!
                }
                catch (Exception e)
                {
                    throw new JsonException(e.Message);
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                TransactionV1Payload payload,
                JsonSerializerOptions options)
            {
                var payloadJson = new TransactionV1PayloadJson()
                {
                    InitiatorAddr = payload.InitiatorAddr,
                    Timestamp = payload.Timestamp,
                    Ttl = payload.Ttl,
                    PricingMode = payload.PricingMode,
                    ChainName = payload.ChainName,
                };
                var ms = new MemoryStream();
                var namedArgSerializer = new NamedArgByteSerializer();
                ms.Write(BitConverter.GetBytes(payload.RuntimeArgs.Count));
                foreach (var args in payload.RuntimeArgs)
                    ms.Write(namedArgSerializer.ToBytes(args));
                payloadJson.Fields.AddField(0, ms.ToArray());
                payloadJson.Fields.AddField(1, payload.Target.ToBytes());
                payloadJson.Fields.AddField(2, payload.EntryPoint.ToBytes());
                payloadJson.Fields.AddField(3, payload.Scheduling.ToBytes());
                JsonSerializer.Serialize(writer, payloadJson, options);
            }
        }
        
        const ushort INITIATOR_ADDR_FIELD_INDEX = 0;
        const ushort TIMESTAMP_FIELD_INDEX = 1;
        const ushort TTL_FIELD_INDEX = 2;
        const ushort CHAIN_NAME_FIELD_INDEX = 3;
        const ushort PRICING_MODE_FIELD_INDEX = 4;
        const ushort FIELDS_FIELD_INDEX = 5;

        const ushort ARGS_MAP_KEY = 0;
        const ushort TARGET_MAP_KEY = 1;
        const ushort ENTRY_POINT_MAP_KEY = 2;
        const ushort SCHEDULING_MAP_KEY = 3;
            
        public byte[] ToBytes()
        {
            var ms = new MemoryStream();
            var namedArgSerializer = new NamedArgByteSerializer();
            ms.Write(BitConverter.GetBytes(RuntimeArgs.Count));
            foreach (var args in RuntimeArgs)
                ms.Write(namedArgSerializer.ToBytes(args));
            
            var fields = new PayloadFields();
            fields.AddField(ARGS_MAP_KEY, ms.ToArray());
            fields.AddField(TARGET_MAP_KEY, Target.ToBytes());
            fields.AddField(ENTRY_POINT_MAP_KEY, EntryPoint.ToBytes());
            fields.AddField(SCHEDULING_MAP_KEY, Scheduling.ToBytes());
            
            return new CalltableSerialization()
                .AddField(INITIATOR_ADDR_FIELD_INDEX, InitiatorAddr.ToBytes())
                .AddField(TIMESTAMP_FIELD_INDEX, CLValue.U64(Timestamp))
                .AddField(TTL_FIELD_INDEX, CLValue.U64(Ttl))
                .AddField(CHAIN_NAME_FIELD_INDEX, CLValue.String(ChainName))
                .AddField(PRICING_MODE_FIELD_INDEX, PricingMode.ToBytes())
                .AddField(FIELDS_FIELD_INDEX, fields.ToBytes())
                .GetBytes();
        }
    }
}