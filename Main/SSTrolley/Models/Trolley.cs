using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SSTrolley.Models
{
    public class Trolley : Point
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public int RouteId { get; set; }
        public double Longitude { get; set; } = 0;
        public double Latitude { get; set; } = 0;
        public double Heading { get; set; } = -1;
        public double LastLongitude { get; set; } = 0;
        public double LastLatitude { get; set; } = 0;
        public double AverageSpeed { get; set; }
        public short MaxPassengers { get; set; }
        public short PassengerCount { get; set; } = 0;
        public double TotalPassengers { get; set; } = 0;
        public double TotalDistance { get; set; } = 0;

        public bool InService { get; set; } = true;
        public string ServiceString { get; set; }

        [JsonIgnore]
        [NotMapped]
        public const string DefaultServiceString = "The Trolley is temporarily out of service. Sorry for the inconvenience.";

        [JsonIgnore]
        public byte[] Hash { get; set; }
        [JsonIgnore]
        public byte[] Salt { get; set; }

        public bool CheckLogin(byte[] login)
        {
            return GenerateHash(login, Salt).SequenceEqual(Hash);
        }

        public static byte[] GenerateHash(byte[] login, byte[] salt)
        {
            char[] chars = new char[((login.Length - 1) / sizeof(char)) + 1];
            Buffer.BlockCopy(login, 0, chars, 0, login.Length);
            string password = new string(chars);
            int l = password.Length;

            return KeyDerivation.Pbkdf2(
               password: password,  
               salt: salt,
               prf: KeyDerivationPrf.HMACSHA512,
               iterationCount: 50000,
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
