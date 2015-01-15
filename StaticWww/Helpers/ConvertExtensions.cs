using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace StaticWww.Helpers
{
    /// <summary>
    /// Holds extensions for the System.Convert class
    /// </summary>
    public static class ConvertExtensions
    {
        #region ToUpcastEnumerable

        /// <summary>
        /// Converts an IEnumerable of on type to an IEnumerable of its parent type
        /// </summary>
        /// <typeparam name="ToType">The type of item the resulting IEnumerable will contain</typeparam>
        /// <typeparam name="FromType">The type of item the source IEnumerable contains</typeparam>
        /// <param name="fromEnumerable">The source IEnumerable</param>
        /// <returns>A new IEnumerable that contains all items from the source IEnumerable, down-cast as 
        /// their parent type</returns>
        public static IEnumerable<ToType> ToUpcastEnumerable<ToType, FromType>(IEnumerable<FromType> fromEnumerable)
            where FromType : ToType
        {
            return new EnumerableConverter<FromType, ToType>(fromEnumerable);
        }
        
        #endregion
    
        #region ToBoolean
        ///  <summary>
        ///  Converts an object to a boolean. If the object cannot be converted, returns false.
        ///  </summary>
        ///  <param name="obj">The object to convert</param>
        ///  <returns>A boolean representing the input value</returns>
        public static bool ToBoolean(object obj)
        {
            return ToBoolean(obj, false);
        }

        ///  <summary>
        ///  Converts an object to a boolean. If the object cannot be converted, returns the specified default value.
        ///  </summary>
        ///  <param name="obj">The object to convert</param>
        ///  <param name="defaultValue">A boolean value to return if the object cannot be converted.</param>
        ///  <returns>A boolean representing the input value</returns>
        ///  <remarks></remarks>
        public static bool ToBoolean(object obj, bool defaultValue)
        {
            bool value;
            if (!TryToBoolean(obj, out value))
            {
                value = defaultValue;
            }
            return value;
        }

        ///  <summary>
        ///  Converts an object to a boolean
        ///  </summary>
        ///  <param name="obj">The object to convert</param>
        ///  <param name="value">Receives parsed boolean value</param>
        ///  <returns>A boolean indicate </returns>
        ///  <remarks></remarks>
		public static bool TryToBoolean(object obj, out bool value)
		{
			return TryToBoolean(
				obj,
				out value,
				CultureInfo.InvariantCulture);
		}

		///  <summary>
		///  Converts an object to a boolean
		///  </summary>
		///  <param name="obj">The object to convert</param>
		///  <param name="value">Receives parsed boolean value</param>
		///  <param name="formatProvider">
		///  IFormatProvider to use
		///  </param>
		///  <returns>A boolean indicate </returns>
		///  <remarks></remarks>
		public static bool TryToBoolean(object obj, out bool value, IFormatProvider formatProvider)
        {
            if (obj == null)
            {
                value = false;
                return false;
            }

            if (obj is bool)
            {
                value = (bool)obj;
                return true;
            }

			return Conversions.TryParseBoolean(
				obj.ToString(),
				out value,
				formatProvider);
        }
        #endregion

        #region ToInt
        ///  <summary>
        ///  Converts an object to an integer. Returns 0 if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <returns>An integer representing the input value.</returns>
        ///  <remarks></remarks>
        public static int ToInt(object obj)
        {
            return ToInt(obj, 0);
        }

        ///  <summary>
        ///  Converts an object to an integer. Returns the default value if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <param name="defaultValue">The value to return if the object cannot be converted.</param>
        ///  <returns>An integer representing the input value.</returns>
        ///  <remarks></remarks>
        public static int ToInt(object obj, int defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            if (obj is bool)
            {
                return Convert.ToBoolean(obj) ? 1 : 0;
            }

            int value;
            if (int.TryParse(obj.ToString(), out value))
            {
                return value;
            }
            return defaultValue;
        }

        #endregion

        #region ToInt32
        ///  <summary>
        ///  Converts an object to an integer. Returns 0 if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <returns>An integer representing the input value.</returns>
        ///  <remarks></remarks>
        public static Int32 ToInt32(object obj)
        {
            return ToInt32(obj, 0);
        }

        ///  <summary>
        ///  Converts an object to an integer. Returns the default value if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <param name="defaultValue">The value to return if the object cannot be converted.</param>
        ///  <returns>An integer representing the input value.</returns>
        ///  <remarks></remarks>
        public static Int32 ToInt32(object obj, Int32 defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            if (obj is bool)
            {
                return Convert.ToBoolean(obj) ? 1 : 0;
            }

            Int32 value;
            if (Int32.TryParse(obj.ToString(), out value))
            {
                return value;
            }

            return defaultValue;
        }

        #endregion

        #region ToInt64
        ///  <summary>
        ///  Converts an object to an integer. Returns 0 if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <returns>An integer representing the input value.</returns>
        ///  <remarks></remarks>
        public static Int64 ToInt64(object obj)
        {
            return ToInt64(obj, 0);
        }

        ///  <summary>
        ///  Converts an object to an integer. Returns the default value if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <param name="defaultValue">The value to return if the object cannot be converted.</param>
        ///  <returns>An integer representing the input value.</returns>
        ///  <remarks></remarks>
        public static Int64 ToInt64(object obj, Int64 defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            if (obj is bool)
            {
                return Convert.ToBoolean(obj) ? 1 : 0;
            }

            Int64 value;
            if (Int64.TryParse(obj.ToString(), out value))
            {
                return value;
            }

            return defaultValue;
        }

        #endregion

        #region ToDouble
        ///  <summary>
        ///  Converts an object to a double. Returns 0 if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <returns>A double representing the input value.</returns>
        ///  <remarks></remarks>
        public static double ToDouble(object obj)
        {
            return ToDouble(obj, 0);
        }

        ///  <summary>
        ///  Converts an object to a double. Returns the default value if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <param name="defaultValue">The value to return if the object cannot be converted.</param>
        ///  <returns>A double representing the input value.</returns>
        ///  <remarks></remarks>
        public static double ToDouble(object obj, double defaultValue)
        {
            return ToDouble(obj, defaultValue, NumberFormatInfo.CurrentInfo);
        }

        ///  <summary>
        ///  Converts an object to a double. Returns the default value if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <param name="defaultValue">The value to return if the object cannot be converted.</param>
        ///  <param name="formatProvider"></param>
        ///  <returns>A double representing the input value.</returns>
        ///  <remarks></remarks>
        public static double ToDouble(object obj, double defaultValue, IFormatProvider formatProvider)
        {
            return ToDouble(obj, defaultValue, NumberStyles.Float | NumberStyles.AllowThousands, formatProvider);
        }

        ///  <summary>
        ///  Converts an object to a double. Returns the default value if it cannot be converted.
        ///  </summary>
        ///  <param name="obj">The object to be converted.</param>
        ///  <param name="defaultValue">The value to return if the object cannot be converted.</param>
        ///  <param name="styles">A bitwise combination of NumberStyles values that indicates the permitted format of obj.</param>
        ///  <param name="formatProvider"></param>
        ///  <returns>A double representing the input value.</returns>
        ///  <remarks></remarks>
        public static double ToDouble(object obj, double defaultValue, NumberStyles styles, IFormatProvider formatProvider)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            if (obj is bool)
            {
                return Convert.ToBoolean(obj) ? 1.0D : 0.0D;
            }

            double value;
            if (double.TryParse(obj.ToString(), styles, formatProvider, out value))
            {
                return double.IsNaN(value) ? defaultValue : value;
            }

            return defaultValue;
        }

        #endregion

        #region TryConvert

        /// <summary>
        /// Performs type conversions in an enum-safe way
        /// </summary>
        /// <typeparam name="T">
        /// Type to convert to
        /// </typeparam>
        /// <param name="value">
        /// String to convert from
        /// </param>
        /// <param name="result">
        /// Result of the conversion
        /// </param>
        /// <returns>
        /// Whether the conversion was successful
        /// </returns>
        public static bool TryConvert<T>(
            string value,
            out T result)
        {
			return TryConvert<T>(
				value,
				out result,
				CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Performs type conversions in an enum-safe way
		/// </summary>
		/// <typeparam name="T">
		/// Type to convert to
		/// </typeparam>
		/// <param name="value">
		/// String to convert from
		/// </param>
		/// <param name="result">
		/// Result of the conversion
		/// </param>
		/// <param name="formatProvider">
		/// IFormatProvider to use for the conversion
		/// </param>
		/// <returns>
		/// Whether the conversion was successful
		/// </returns>
		public static bool TryConvert<T>(
			string value,
			out T result,
			IFormatProvider formatProvider)
		{
			if (value != null)
			{
				var converter = Conversions.GetConverter<T>();
				return converter(
					value,
					out result,
					formatProvider);
			}

            result = default(T);
			return false;
		}

        /// <summary>
        /// Non-generic version of TryConvert. Useful for callers that
        /// have only a runtime type.
        /// </summary>
        public static bool TryConvert(
            string value,
            Type targetType,
            out object result,
            IFormatProvider formatProvider)
        {
            if (value != null)
            {
                var converter = Conversions.GetConverter(targetType);
                return converter(
                    value,
                    out result,
                    formatProvider);
            }

            result = targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            return false;
        }

        /// <summary>
        /// Converts the value to the type of the specified default value, 
        /// and returns the default value if the conversion fails.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T ConvertOrDefault<T>(string value, T defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }

            T result;
            if (TryConvert<T>(value, out result))
            {
                return result;
            }
            
            return defaultValue;
        }

        #endregion TryConvert        
        
        #region ToList

        /// <summary>
        /// Convert an ICollection to a typed List.  The items in the collection must actually
        /// be convertible to the specified type or a runtime error will result.
        /// Notably useful when applied to DomainData Item() collections.
        /// </summary>
        /// <typeparam name="ListItemType">the type of item in the collection</typeparam>
        /// <param name="collection">the collection to convert</param>
        /// <returns>a typed List containing all items in the collection</returns>
        public static List<ListItemType> ToList<ListItemType>(IEnumerable collection)
        {
            return collection.Cast<ListItemType>().ToList();
        }

        /// <summary>
        /// Convert an ICollection to a typed List.  The items in the collection must actually
        /// be convertible to the specified type or a runtime error will result.
        /// Notably useful when applied to DomainData Item() collections.
        /// </summary>
        /// <typeparam name="ListItemType">the type of item in the collection</typeparam>
        /// <param name="collection">the collection to convert</param>
        /// <returns>a typed List containing all items in the collection</returns>
        public static List<ListItemType> ToList<ListItemType>(ICollection<ListItemType> collection)
        {
            return collection.ToList();
        }

        /// <summary>
        /// Copies the enumerable to a list of the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns></returns>
        public static List<T> ToList<T>(IEnumerable<T> enumerable)
        {
            return enumerable.ToList();
        }

        #endregion

        #region EnumerableConverter Class

        /// <summary>
        /// A wrapper to work around generic variance problems. 
        /// Provides a way to cast derived type to it's base within IEnumerable.
        /// </summary>
        /// <typeparam name="Source">Source type</typeparam>
        /// <typeparam name="Destination">Destination type 
        /// (must be a base type of the source type)</typeparam>
        private class EnumerableConverter<Source, Destination>
            : IEnumerable<Destination> where Source : Destination
        {
            //Holds source Enumerable
            private readonly IEnumerable<Source> _source;

            #region Constructor
            /// <summary>
            /// Creates new instance of the EnumerableConverter class.
            /// </summary>
            /// <param name="source">Source enumerable to convert.</param>
            public EnumerableConverter(IEnumerable<Source> source)
            {
                _source = source;
            }
            #endregion

            #region IEnumerable<Destination> Members

            ///<summary>
            ///Returns an enumerator that iterates through the collection.
            ///</summary>
            ///<returns>
            ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
            ///</returns>
            ///<filterpriority>1</filterpriority>
            public IEnumerator<Destination> GetEnumerator()
            {
                return new EnumeratorWrapper(_source.GetEnumerator());
            }

            #endregion

            #region IEnumerable Members

            ///<summary>
            ///Returns an enumerator that iterates through a collection.
            ///</summary>
            ///<returns>
            ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
            ///</returns>
            ///<filterpriority>2</filterpriority>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            private class EnumeratorWrapper : IEnumerator<Destination>
            {
                //holds source Enumerator
                private readonly IEnumerator<Source> _source;


                #region Constructor
                /// <summary>
                /// Creates new instance of the EnumeratorWrapper class.
                /// </summary>
                /// <param name="source">Source enumerator to convert.</param>
                public EnumeratorWrapper(IEnumerator<Source> source)
                {
                    _source = source;
                }
                #endregion

                #region IEnumerator<Destination> Members

                ///<summary>
                ///Gets the element in the collection at the current position of the enumerator.
                ///</summary>
                ///<returns>
                ///The element in the collection at the current position of the enumerator.
                ///</returns>
                Destination IEnumerator<Destination>.Current
                {
                    get { return _source.Current; }
                }

                #endregion

                #region IDisposable Members

                ///<summary>
                ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
                ///</summary>
                ///<filterpriority>2</filterpriority>
                public void Dispose()
                {
                    _source.Dispose();
                }

                #endregion

                #region IEnumerator Members

                ///<summary>
                ///Advances the enumerator to the next element of the collection.
                ///</summary>
                ///<returns>
                ///true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
                ///</returns>
                ///<exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
                public bool MoveNext()
                {
                    return _source.MoveNext();
                }

                ///<summary>
                ///Sets the enumerator to its initial position, which is before the first element in the collection.
                ///</summary>
                ///<exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
                public void Reset()
                {
                    _source.Reset();
                }

                ///<summary>
                ///Gets the current element in the collection.
                ///</summary>
                ///<returns>
                ///The current element in the collection.
                ///</returns>
                ///<exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.-or- The collection was modified after the enumerator was created.</exception><filterpriority>2</filterpriority>
                public object Current
                {
                    get { return _source.Current; }
                }

                #endregion
            }
        }

        #endregion

        public static class Conversions
		{
			public delegate bool Converter<T>(string s, out T value, IFormatProvider formatProvider);

            /// <summary>
            /// Non generic version of converter, for use with conversion methods that have only a runtime type.
            /// </summary>
            public delegate bool Converter(string s, out object value, IFormatProvider formatProvider);

			private static readonly Dictionary<Type, object> _converters = new Dictionary<Type, object>();

            /// <summary>
            /// Non generic version of converters map, for use with conversion methods that have only a runtime type.
            /// </summary>
            private static readonly Dictionary<Type, object> _nonGenericConverters = new Dictionary<Type, object>();

			static Conversions()
			{
				// custom conversions
				_converters.Add(typeof(Color), (Converter<Color>) TryParseColor);
				_converters.Add(typeof(Guid), (Converter<Guid>) TryParseGuid);

				_converters.Add(typeof(sbyte), (Converter<sbyte>) TryParseSByte);
				_converters.Add(typeof(short), (Converter<short>) TryParseInt16);
				_converters.Add(typeof(int), (Converter<int>) TryParseInt32);
				_converters.Add(typeof(long), (Converter<long>) TryParseInt64);

				_converters.Add(typeof(byte), (Converter<byte>) TryParseByte);
				_converters.Add(typeof(ushort), (Converter<ushort>) TryParseUInt16);
				_converters.Add(typeof(uint), (Converter<uint>) TryParseUInt32);
				_converters.Add(typeof(ulong), (Converter<ulong>) TryParseUInt64);

				_converters.Add(typeof(bool), (Converter<bool>) TryParseBoolean);
                _converters.Add(typeof(bool?), (Converter<bool?>)TryParseNullableBoolean);

				_converters.Add(typeof(DateTime), (Converter<DateTime>) TryParseDateTime);

				_converters.Add(typeof(string), (Converter<string>) TryParseString);

                // Populate a corresponding map of non-generic converters, for use by consumers that only have a runtime type
                foreach (KeyValuePair<Type, object> pair in _converters)
                {
                    _nonGenericConverters.Add(pair.Key, GetNonGenericConverter(pair.Key, pair.Value));
                }
			}

            /// <summary>
            /// Given a type and a generic converter, produces a non-generic version of the converter.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="genericConverter"></param>
            /// <returns></returns>
            private static Converter GetNonGenericConverter(Type type, object genericConverter)
            {
                var nonGenericConverterCreator = (INonGenericConverter)Activator.CreateInstance(typeof(NonGenericConverter<>).MakeGenericType(type));
                return nonGenericConverterCreator.Get(genericConverter);
            }

            /// <summary>
            /// Utility class that creates a non-generic version of a generic converter
            /// </summary>
            /// <typeparam name="T"></typeparam>
            private class NonGenericConverter<T> : INonGenericConverter
            {
                public Converter Get(object genericConverter)
                {
                    var converter = (Converter<T>)genericConverter;
                    return delegate(string s, out object value, IFormatProvider formatProvider) 
                    {
                        T outValue;
                        bool success = converter(s, out outValue, formatProvider);
                        value = outValue;
                        return success;
                    };
                }
            }

            /// <summary>
            /// Interface to NonGenericConverter that allows us to interact with it without a generic Type param.
            /// </summary>
            private interface INonGenericConverter
            {
                Converter Get(object genericConverter);
            }

            #region Conversions
			/// <summary>
			/// Attempts to parse a color
			/// </summary>
			/// <param name="value"></param>
			/// <param name="color"></param>
			/// <param name="formatProvider"></param>
			/// <returns></returns>
			private static bool TryParseColor(
				string value,
				out Color color,
				IFormatProvider formatProvider)
			{
				int argb;

                if (value != null)
                {
                    value = value.TrimStart('#');
                }
                else
                {
                    color = new Color();
                    return false;
                }

			    //Identify known color names (i.e. "blue")
                color = Color.FromName(value);
                if (color.IsKnownColor)
                {
                    return true;
                }

				if (!int.TryParse(value, NumberStyles.HexNumber, formatProvider, out argb))
				{
					color = new Color();
					return false;
				}

                if (value.Length == 3)
                {
                    value = string.Format("{0}{0}{1}{1}{2}{2}", value[0], value[1], value[2]);

                    if (!int.TryParse(value, NumberStyles.HexNumber, formatProvider, out argb))
                    {
                        color = new Color();
                        return false;
                    }
                }

				// If color parameter is only 6 digits long then assume an Alpha value of FF
				if (value.Length == 6)
				{
					argb = argb | unchecked((int) 0xFF000000);
				}
				else if (value.Length != 8)
				{
					color = new Color();
					return false;
				}

				color = Color.FromArgb(argb);
				return true;
			}

			private static bool TryParseGuid(
				string value,
				out Guid guid,
				IFormatProvider formatProvider)
			{
				// it has to be at least 32 characters or it can't
				// possibly be valid
				if (value == null || value.Length < 0x20)
				{
					guid = Guid.Empty;
					return false;
				}

				try
				{
					guid = new Guid(value);
				}
				catch
				{
					guid = Guid.Empty;
					return false;
				}

				return true;
			}

			private static bool TryParseString(string s, out string value, IFormatProvider formatProvider)
			{
				value = s;
				return true;
			}

			private static bool TryParseDateTime(string s, out DateTime value, IFormatProvider formatProvider)
			{
				return DateTime.TryParse(
					s,
					formatProvider,
					DateTimeStyles.None,
					out value);
			}

			private static bool TryParseByte(string s, out byte value, IFormatProvider formatProvider)
			{
				return byte.TryParse(
					s,
					NumberStyles.Integer,
					formatProvider,
					out value);
			}

			private static bool TryParseSByte(string s, out sbyte value, IFormatProvider formatProvider)
			{
				return sbyte.TryParse(
					s,
					NumberStyles.Integer,
					formatProvider,
					out value);
			}

			private static bool TryParseInt16(string s, out short value, IFormatProvider formatProvider)
			{
				return short.TryParse(
					s,
					NumberStyles.Integer,
					formatProvider,
					out value);
			}

			private static bool TryParseInt32(string s, out int value, IFormatProvider formatProvider)
			{
				return int.TryParse(
					s,
					NumberStyles.Integer,
					formatProvider,
					out value);
			}

			private static bool TryParseInt64(string s, out long value, IFormatProvider formatProvider)
			{
				return long.TryParse(
					s,
					NumberStyles.Integer,
					formatProvider,
					out value);
			}

			private static bool TryParseUInt16(string s, out ushort value, IFormatProvider formatProvider)
			{
				return ushort.TryParse(
					s,
					NumberStyles.Integer,
					formatProvider,
					out value);
			}

			private static bool TryParseUInt32(string s, out uint value, IFormatProvider formatProvider)
			{
				return uint.TryParse(
					s,
					NumberStyles.Integer,
					formatProvider,
					out value);
			}

			private static bool TryParseUInt64(string s, out ulong value, IFormatProvider formatProvider)
			{
				return ulong.TryParse(
					s,
					NumberStyles.Integer,
					formatProvider,
					out value);
			}

			public static bool TryParseBoolean(string s, out bool value, IFormatProvider formatProvider)
			{
				if (s == null)
				{
					value = false;
					return false;
				}

				double doubleValue;
				if (double.TryParse(s, out doubleValue))
				{
					value = doubleValue > 0;
					return true;
				}

				switch (s.ToLower())
				{
					case "true":
					case "on":
					case "yes":
					case "y":
					case "ok":
					case "enabled":
						value = true;
						return true;
					case "false":
					case "off":
					case "no":
					case "n":
					case "disabled":
						value = false;
						return true;
					default:
						return bool.TryParse(
							s,
							out value);
				}
			}

            public static bool TryParseNullableBoolean(string s, out bool? value, IFormatProvider formatProvider)
            {
                if (s == null)
                {
                    value = null;
                    return false;
                }

                double doubleValue;
                if (double.TryParse(s, out doubleValue))
                {
                    value = doubleValue > 0;
                    return true;
                }

                switch (s.ToLower())
                {
                    case "true":
                    case "on":
                    case "yes":
                    case "y":
                    case "ok":
                    case "enabled":
                        value = true;
                        return true;
                    case "false":
                    case "off":
                    case "no":
                    case "n":
                    case "disabled":
                        value = false;
                        return true;
                    default:
                        // try to parse as a bool, if that fails, we're null.
                        bool temp;
                        bool result = bool.TryParse(s, out temp);
                        if (result)
                        {
                            value = temp;
                        }
                        else
                        {
                            value = null;
                        }
                        return result;

                }
            }

            [DebuggerHidden]
			private static bool TryParseEnum<T>(string s, out T value, IFormatProvider formatProvider)
			{
				try
				{
                    if (!string.IsNullOrEmpty(s))
                    {
                        value = (T)Enum.Parse(typeof(T), s, true);
                        return true;
                    }
				}
				catch
				{
				}

				value = default(T);
				return false;
			}

			/// <summary>
			/// Wraps Convert.ChangeType and doesnt throw exceptions
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="value">The value.</param>
			/// <param name="result">The result.</param>
			/// <param name="formatProvider">
			/// IFormatProvider to use
			/// </param>
			/// <returns></returns>
			[DebuggerStepThrough]
			private static bool TryChangeType<T>(string value, out T result, IFormatProvider formatProvider)
			{
				if (string.IsNullOrEmpty(value))
				{
					result = default(T);
					return false;
				}

				try
				{
					// always use the invariant culture, otherwise number formats may be incorrect
					result = (T) Convert.ChangeType(value, typeof(T), formatProvider);
					return true;
				}
				catch (Exception)
				{
					result = default(T);
					return false;
				}
			}

			#endregion Conversions

            /// <summary>
            /// Gets a generic converter for the specified type
            /// </summary>
			public static Converter<T> GetConverter<T>()
			{
				var type = typeof(T);
				object converter;

				if (_converters.TryGetValue(type, out converter))
				{
					return (Converter<T>) converter;
				}

				if (type.IsEnum)
				{
					return TryParseEnum<T>;
				}

				return TryChangeType;
			}

            /// <summary>
            /// Gets a non generic converter for the specified runtime type
            /// </summary>
            public static Converter GetConverter(Type type)
            {
                object converter;

                if (_nonGenericConverters.TryGetValue(type, out converter))
                {
                    return (Converter)converter;
                }

                if (type.IsEnum)
                {
                    return TryParseEnum;
                }

                return TryChangeType;
            }
		}
    }

}
