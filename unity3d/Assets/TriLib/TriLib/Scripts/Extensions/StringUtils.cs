using System.Globalization;

namespace TriLib {
    /// <summary>
    /// Represents a series of string functions.
    /// </summary>
    public class StringUtils
    {
        /// <summary>
        /// Generates a new name for an object.
        /// </summary>
        /// <returns>Generated name.</returns>
        public static string GenerateUniqueName(object id)
        {
            return id.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }
    }
}

