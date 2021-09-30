using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Username { get; set; }

        [JsonIgnore]
        public byte[] Hash { get; set; }
        [JsonIgnore]
        public byte[] Salt { get; set; }

        public User() { }
        public User(string username, string password, RandomNumberGenerator rng)
        {
            Salt = GenerateSalt(rng);
            Hash = GenerateHash(password, Salt);
            Username = username;
        }

        public bool CheckLogin(string password)
        {
            return GenerateHash(password, Salt).SequenceEqual(Hash);
        }

        public static byte[] GenerateHash(string password, byte[] salt)
        {
            return KeyDerivation.Pbkdf2(
               password: password,
               salt: salt,
               prf: KeyDerivationPrf.HMACSHA512,
               iterationCount: 100000,
               numBytesRequested: 256 / 8);
        }
        
        public static byte[] GenerateSalt(RandomNumberGenerator rng)
        {
            byte[] salt = new byte[128 / 8];
            rng.GetBytes(salt);
            return salt;
        }
    }
}
