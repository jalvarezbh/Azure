using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ru.core.integrations.customer.core.Services.Hash;

public interface IGenericObjectHasher<T> where T : class
{
    /// <summary>
    /// Generates a hash for the given object.
    /// </summary>
    /// <param name="obj">The object to hash.</param>
    /// <returns>A string representing the hash of the object.</returns>
    string GenerateHash(T obj);
    /// <summary>
    /// Validates if the given hash matches the object's hash.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <param name="hash">The hash to validate against.</param>
    /// <returns>True if the hash matches, otherwise false.</returns>
    bool ValidateHash(T obj, string hash);
}

