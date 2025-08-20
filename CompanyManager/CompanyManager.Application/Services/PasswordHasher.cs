#nullable enable

using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Interfaces;
using System;

namespace CompanyManager.Application.Services
{
    /// <summary>
    /// Service for securely hashing and verifying passwords using BCrypt algorithm.
    /// </summary>
    /// <remarks>
    /// This service provides password hashing and verification capabilities using the
    /// BCrypt algorithm, which is designed to be computationally expensive and
    /// resistant to brute-force attacks. Each hash includes a salt automatically.
    /// </remarks>
    public sealed class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Generates a secure hash from a plain text password using BCrypt.
        /// </summary>
        /// <param name="plain">The plain text password to hash.</param>
        /// <returns>
        /// A cryptographically secure hash string that includes a salt and can be
        /// safely stored in a database or configuration file.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="plain"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="plain"/> is empty or whitespace.
        /// </exception>
        public string Hash(string plain)
        {
            if (plain is null)
                throw new ArgumentNullException(nameof(plain));

            if (string.IsNullOrWhiteSpace(plain))
                throw new ArgumentException("Password cannot be empty or whitespace.", nameof(plain));

            return BCrypt.Net.BCrypt.HashPassword(plain);
        }

        /// <summary>
        /// Verifies if a plain text password matches a stored BCrypt hash.
        /// </summary>
        /// <param name="plainTextPassword">The plain text password to verify.</param>
        /// <param name="hash">The stored BCrypt hash to compare against.</param>
        /// <returns>
        /// <see langword="true"/> if the password matches the hash;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="plainTextPassword"/> or <paramref name="hash"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="plainTextPassword"/> or <paramref name="hash"/> is empty or whitespace.
        /// </exception>
        public bool Verify(string plainTextPassword, string hash)
        {
            if (plainTextPassword is null)
                throw new ArgumentNullException(nameof(plainTextPassword));

            if (hash is null)
                throw new ArgumentNullException(nameof(hash));

            if (string.IsNullOrWhiteSpace(plainTextPassword))
                throw new ArgumentException("Password cannot be empty or whitespace.", nameof(plainTextPassword));

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Hash cannot be empty or whitespace.", nameof(hash));

            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hash);
        }
    }
}
