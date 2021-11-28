using System;
using Casper.Network.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Casper.Network.SDK.Types
{
    public class URef : GlobalStateKey
    {
        public AccessRights AccessRights { get; }

        public URef(string value) : base(value)
        {
            KeyIdentifier = KeyIdentifier.URef;

            var parts = value.Substring(5).Split(new char[] {'-'});
            if (parts.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "An URef object must end with an access rights suffix.");
            if (parts[0].Length != 64)
                throw new ArgumentOutOfRangeException(nameof(value), "An Uref object must contain a 32 byte value.");
            if (parts[1].Length != 3)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "An URef object must contain a 3 digits access rights suffix.");

            CEP57Checksum.Decode(parts[0], out int checksumResult);
            if (checksumResult == CEP57Checksum.InvalidChecksum)
                throw new ArgumentException("URef checksum mismatch.");
            
            AccessRights = (AccessRights) uint.Parse(parts[1]);
        }
        
        public URef(byte[] bytes)
            : this($"uref-{Hex.ToHexString(bytes[..32])}-{(int)bytes[32]:000}")
        {
        }
        
        public URef(byte[] rawBytes, AccessRights accessRights)
            : this($"uref-{Hex.ToHexString(rawBytes)}-{(int)accessRights:000}")
        {
        }

        protected override byte[] _GetRawBytesFromKey(string key)
        {
            key = key.Substring(0, key.LastIndexOf('-'));
            return Hex.Decode(key.Substring(key.LastIndexOf('-')+1));
        }

        public override string ToString()
        {
            return "uref-" + CEP57Checksum.Encode(RawBytes) + $"-{(byte) AccessRights:000}";
        }
    }
}