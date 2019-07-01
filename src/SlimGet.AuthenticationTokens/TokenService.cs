using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using SlimGet.Data;
using SlimGet.Data.Configuration;

#pragma warning disable IDE0046 // Convert to conditional expression
namespace SlimGet.Services
{
    public sealed class TokenService
    {
        private byte[] TokenHmacKey { get; }
        private Encoding TextEncoding { get; }

        public TokenService(ITokenConfigurationProvider tcfgProvider, IEncodingProvider encProvider)
        {
            this.TextEncoding = encProvider.TextEncoding;

            var tkc = tcfgProvider.GetTokenConfiguration();
            using (var sha512 = SHA512.Create())
                this.TokenHmacKey = sha512.ComputeHash(this.TextEncoding.GetBytes(tkc.TokenHmacKey));
        }

        public string EncodeToken(AuthenticationToken token)
            => string.Create(32 /* length of N-format GUID */ + 64 /* Length of HMAC-SHA256 output */, token, this.CreateString);

        public bool TryReadTokenId(string tokenString, out Guid guid)
        {
            guid = default;
            if (tokenString.Length != 96)
                return false;

            if (!tokenString.All(xc => (xc <= '9' && xc >= '0') || (xc >= 'a' && xc <= 'f')))
                return false;

            return Guid.TryParseExact(tokenString.AsSpan(0, 32), "N", out guid);
        }

        public bool ValidateToken(string tokenString, string userId, DateTimeOffset issuedAt, Guid guid, out AuthenticationToken token)
        {
            var issued = issuedAt.ToUnixTimeMilliseconds();

            token = default;
            if (tokenString.Length != 96)
                return false;

            if (!tokenString.All(xc => (xc <= '9' && xc >= '0') || (xc >= 'a' && xc <= 'f')))
                return false;

            var stampString = tokenString.AsSpan(32, 64);
            var stampDst = this.ComputeHmac(guid, issued, userId);
            Span<byte> stampSrc = stackalloc byte[stampDst.Length];
            for (var i = 0; i < stampString.Length; i += 2)
                if (!byte.TryParse(stampString.Slice(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out stampSrc[i / 2]))
                    return false;

            if (stampSrc.SequenceEqual(stampDst))
            {
                token = new AuthenticationToken(userId, issuedAt, guid);
                return true;
            }

            return false;
        }

        private void CreateString(Span<char> buffer, AuthenticationToken state)
        {
            // optimize lookups
            var guid = state.Guid;
            var issued = state.IssuedAt.ToUnixTimeMilliseconds();
            var userId = state.UserId;

            // skip bound checks
            buffer[95 /* 32 + 64 - 1 */] = '0';

            // write guid. part
            guid.TryFormat(buffer, out _, "N");

            // write hmac part in reverse
            var stampHmac = this.ComputeHmac(guid, issued, userId);
            for (var i = stampHmac.Length - 1; i >= 0; i--)
            {
                var offset = 32 + i * 2;
                stampHmac[i].TryFormat(buffer.Slice(offset), out _, "x2", CultureInfo.InvariantCulture);
            }
        }

        private byte[] CreateTokenStamp(long issued, string userId)
        {
            var len = this.TextEncoding.GetByteCount(userId) + Unsafe.SizeOf<long>();
            var stamp = new byte[len];

            BinaryPrimitives.WriteInt64LittleEndian(stamp, issued);
            this.TextEncoding.GetBytes(userId, 0, userId.Length, stamp, 8);

            return stamp;
        }

        private byte[] ComputeHmac(Guid guid, long issued, string userId)
        {
            var salt = ArrayPool<byte>.Shared.Rent(Unsafe.SizeOf<Guid>());
            try
            {
                guid.TryWriteBytes(salt);
                var cycles = BinaryPrimitives.ReadUInt16LittleEndian(salt);

                // compute hmac
                byte[] stampHmac = null;
                using (var pbkdf2 = new Rfc2898DeriveBytes(this.TokenHmacKey, salt, cycles, HashAlgorithmName.SHA512))
                using (var hmac = new HMACSHA256(pbkdf2.GetBytes(64))) // HMAC-SHA256 uses 64-byte (512-bit) keys
                    stampHmac = hmac.ComputeHash(this.CreateTokenStamp(issued, userId));

                return stampHmac;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(salt, true);
            }
        }
    }
}
#pragma warning restore IDE0046 // Convert to conditional expression
