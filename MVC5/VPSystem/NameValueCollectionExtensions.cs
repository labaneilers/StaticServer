using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Linq;

namespace VPSystem
{
    /// <summary>
    /// Extensions to NameValueCollection
    /// </summary>
    /// <remarks>
    /// NameValueCollection is the type of Request.QueryString
    /// </remarks>
    /// <futureDeprecation>
    /// This will be refactored into extension methods once we move to the 3.5 framework.
    /// </futureDeprecation>
    public static class NameValueCollectionExtensions
    {
		#region Get
		/// <summary>
		/// Gets a string value from the NameValueCollection
		/// </summary>
		/// <param name="collection">
		/// NameValueCollection from which the value should be retrieved
		/// </param>
		/// <param name="name">
		/// Name of the value to retrieve
		/// </param>
		/// <returns>
		/// String read from QueryString or Form, trimmed
		/// </returns>
		public static string Get(NameValueCollection collection, string name)
		{
			return Get(collection, name, null);
		}

		/// <summary>
		/// Gets a string value from the NameValueCollection
		/// </summary>
		/// <param name="collection">
		/// NameValueCollection from which the value should be retrieved
		/// </param>
		/// <param name="name">
		/// Name of the value to retrieve
		/// </param>
		/// <param name="defaultValue">
		/// Default value to return if a value with the specified name is not found
		/// </param>
		/// <returns>
		/// String read from QueryString or Form, trimmed
		/// </returns>
		public static string Get(NameValueCollection collection, string name, string defaultValue)
		{
			if (collection == null)
			{
				return defaultValue;
			}

			string value = collection[name];

			if (!String.IsNullOrEmpty(value))
			{
				value = value.Trim();
			}

			if (String.IsNullOrEmpty(value))
			{
				value = defaultValue;
			}

			return value;
		}

        /// <summary>
        /// Gets a value off of the collection, or returns a default
        /// </summary>
		public static T Get<T>(this NameValueCollection nvc, int index, T defaultValue)
		{
			return ConvertOrDefault<T>(nvc[index], defaultValue);
		}

        /// <summary>
        /// Gets a value off of the collection, or returns a default
        /// </summary>
        public static T Get<T>(this NameValueCollection nvc, string name, T defaultValue)
		{
			return ConvertOrDefault<T>(nvc[name], defaultValue);
		}

        public static IEnumerable<T> GetValues<T>(this NameValueCollection collection, string name, T defaultValue)
        {
            foreach (string value in collection.GetValues(name) ?? new string[] { })
            {
                T parsed;
                if (!TryConvert<T>(value, out parsed))
                {
                    parsed = defaultValue;
                }

                yield return parsed;
            }
        }

        private static T ConvertOrDefault<T>(string value, T defaultValue)
		{
			T result;
			if (TryConvert<T>(value, out result))
			{
				return result;
			}

			return defaultValue;
		}

        /// <summary>
        /// Gets a value off of the collection, or returns null
        /// </summary>
        public static T? Get<T>(NameValueCollection nvc, int index) where T : struct
        {
            return ConvertOrNull<T>(nvc[index]);
        }

        /// <summary>
        /// Gets a value off of the collection, or returns null
        /// </summary>
        public static T? Get<T>(NameValueCollection nvc, string name) where T : struct
        {
            return ConvertOrNull<T>(nvc[name]);
        }

        private static T? ConvertOrNull<T>(string value) where T : struct
        {
            T result;
            if (TryConvert<T>(value, out result))
            {
                return result;
            }

            return null;
        }


		#endregion Get

		#region TryGet
		/// <summary>
		/// Attempts to get a value from the NameValueCollection
		/// </summary>
		/// <param name="collection">
		/// NameValueCollection from which the value should be retrieved
		/// </param>
		/// <param name="name">
		/// Name of the value to retrieve
		/// </param>
		/// <param name="result">
		/// Result of the operation
		/// </param>
		/// <returns>
		/// Whether the value was successfully read
		/// </returns>
		/// <remarks>
		/// This method returns true even if the value is empty
		/// </remarks>
		public static bool TryGet(NameValueCollection collection, string name, out string result)
		{
			// NameValueCollection returns null if the item wasn't in 
			// the collection, String.Empty if it was in the collection with no value
			result = collection[name];
			return result != null;
		}

		/// <summary>
		/// Attempts to get a value from the NameValueCollection, converting to a specified type
		/// </summary>
		/// <typeparam name="T">
		/// Type to convert the value to
		/// </typeparam>
		/// <param name="collection">
		/// NameValueCollection from which the value should be retrieved
		/// </param>
		/// <param name="name">
		/// Name of the value to retrieve
		/// </param>
		/// <param name="result">
		/// Result of the operation
		/// </param>
		/// <returns>
		/// Whether the value was successfully read and converted
		/// </returns>
		public static bool TryGet<T>(NameValueCollection collection, string name, out T result)
		{
			return TryConvert<T>(collection[name], out result);
		}
		#endregion TryGet

		#region Get (with range constraint)
		/// <summary>
		/// Gets a value from the NameValueCollection, converting to a specified type
		/// </summary>
		/// <typeparam name="T">
		/// Type to convert the value to
		/// </typeparam>
		/// <param name="collection">
		/// NameValueCollection from which the value should be retrieved
		/// </param>
		/// <param name="name">
		/// Name of the value to retrieve
		/// </param>
		/// <param name="defaultValue">
		/// Default value to return if the named value is not found
		/// </param>
		/// <param name="minValue">
		/// Minimum allowed value
		/// </param>
		/// <param name="maxValue">
		/// Maximum allowed value
		/// </param>
		/// <returns>
		/// Value of the given name converted to the specified type or default(T) if no value is found
		/// </returns>
		public static T Get<T>(NameValueCollection collection, string name, T defaultValue, T minValue, T maxValue)
			where T : IComparable
		{
			T value = Get<T>(collection, name, defaultValue);

			if (value.CompareTo(minValue) < 0)
			{
				return minValue;
			}

			if (value.CompareTo(maxValue) > 0)
			{
				return maxValue;
			}

			return value;
		}
		#endregion Get (with range constraint)

		private static bool TryConvert<T>(string value, out T result)
		{
			// we convert in two locales
			return ConvertExtensions.TryConvert<T>(value, out result, CultureInfo.InvariantCulture) ||
				ConvertExtensions.TryConvert<T>(value, out result, CultureInfo.CurrentUICulture);
		}
    }
}
