using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ElephantSocial.Chat.Util
{
    public static class JwtHelper
    {
        public static string GenerateJwtToken(string elephantId, string gameId, string gameSecret)
        {
            var header = new Dictionary<string, string>
            {
                { "alg", "HS256" },
                { "typ", "JWT" }
            };

            var issuedAt = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var expiresAt = issuedAt + 3600 * 24 * 30 * 12;

            var payload = new Dictionary<string, object>
            {
                { "sub", elephantId },
                { "iss", gameId },
                { "iat", issuedAt },
                { "exp", expiresAt }
            };

            string encodedHeader = Base64UrlEncode(SerializeJson(header));
            string encodedPayload = Base64UrlEncode(SerializeJson(payload));

            string signatureInput = $"{encodedHeader}.{encodedPayload}";
            string signature = CreateSignature(signatureInput, gameSecret);

            return $"{encodedHeader}.{encodedPayload}.{signature}";
        }

        private static string SerializeJson(Dictionary<string, object> dict)
        {
            var entries = new List<string>();
            foreach (var kvp in dict)
            {
                string value;
                if (kvp.Value is string strValue)
                {
                    value = $"\"{strValue}\"";
                }
                else
                {
                    value = kvp.Value.ToString();
                }
                entries.Add($"\"{kvp.Key}\":{value}");
            }
            return "{" + string.Join(",", entries) + "}";
        }

        private static string SerializeJson(Dictionary<string, string> dict)
        {
            var entries = new List<string>();
            foreach (var kvp in dict)
            {
                entries.Add($"\"{kvp.Key}\":\"{kvp.Value}\"");
            }
            return "{" + string.Join(",", entries) + "}";
        }

        private static string Base64UrlEncode(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            string base64 = Convert.ToBase64String(bytes);
            return base64
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string CreateSignature(string input, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Base64UrlEncode(hash);
            }
        }
    }
}