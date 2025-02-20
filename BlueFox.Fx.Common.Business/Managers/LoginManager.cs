﻿//-----------------------------------------------------------------------
// <copyright file="LoginManager.cs" company="BlueFox">
// Copyright (c) BlueFox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace BlueFox.Fx.Common.Managers
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using BlueFox.Fx.Common.Managers.Interfaces;

    /// <inheritdoc />
    public sealed class LoginHandler : ILoginHandler
    {
        private const int SaltSize = 16; // 128 bit.
        private const int KeySize = 32; // 256 bit.
        private const int Iterations = 10000;

        /// <inheritdoc />
        public string DecodeBase64(string value)
        {
            byte[] data = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(data);
        }

        /// <inheritdoc />
        public string EncodeBase64(string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(data);
        }

        /// <inheritdoc />
        public string HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
              password,
              SaltSize,
              Iterations,
              HashAlgorithmName.SHA512))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{Iterations}.{salt}.{key}";
            }
        }

        /// <inheritdoc />
        public (bool Verified, bool NeedsUpgrade) CheckPassword(string password, string hash)
        {
            var parts = hash.Split('.', 3);

            if (parts.Length != 3)
            {
                throw new FormatException("Unexpected hash format. " +
                  "Should be formatted as `{iterations}.{salt}.{hash}`");
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var needsUpgrade = iterations != iterations;

            using (var algorithm = new Rfc2898DeriveBytes(
              password,
              salt,
              iterations,
              HashAlgorithmName.SHA512))
            {
                var keyToCheck = algorithm.GetBytes(KeySize);

                var verified = keyToCheck.SequenceEqual(key);

                return (verified, needsUpgrade);
            }
        }
    }
}