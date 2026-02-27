using Daibitx.AspNetCore.Extensions.Utils;
using Daibitx.DomainCore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace XinMenu.Entitys
{
    [Table("userrefreshtokens")]
    public class UserRefreshToken : Entity<long>
    {
        public long UserId { get; set; }

        public string TokenHash { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.Now;

        public UserRefreshToken() { }

        public UserRefreshToken(string userAgent, long userId)
        {
            UserAgent = userAgent;
            UserId = userId;
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Create a new refresh token and return it.
        /// </summary>
        /// <returns></returns>
        public string CreateRefreshToken()
        {
            var token = GenerateRefreshToken();
            TokenHash = EncryptionUtil.Sha256(token);
            ExpiresAt = DateTime.Now.AddDays(7);
            return token;
        }

        public static string GetTokenHash(string token)
        {
            return EncryptionUtil.Sha256(token);
        }
    }

    public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.ToTable("userrefreshtokens");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
