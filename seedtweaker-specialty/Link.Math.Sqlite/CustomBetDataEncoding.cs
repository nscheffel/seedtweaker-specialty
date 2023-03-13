// -----------------------------------------------------------------------
// <copyright file = "CustomBetDataEncoding.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Collection.Generic;

    /// <summary>
    ///     Bet level custom bet encoding/decoding for sql lite.
    /// </summary>
    public class CustomBetDataEncoding : ICustomBetDataEncoding
    {
        #region Methods

        /// <inheritdoc />
        public IReadOnlyDictionary<string, long> Decode(string customBet)
        {
            var results = new Dictionary<string, long>();

            if(!string.IsNullOrEmpty(customBet))
            {
                if(!customBet.StartsWith("{") || !customBet.EndsWith("}"))
                {
                    throw new FormatException("customBet encoding should start with '{' and end with '}'.");
                }

                var trimmed = customBet.Trim('{', '}');
                var split = trimmed.Split(',');
                foreach(var pair in split)
                {
                    var keyVal = pair.Split(':');

                    if(keyVal.Length != 2)
                    {
                        throw new ArgumentException(
                            string.Format("Failed to generate key value pair for string: '{0}'\n" +
                                          "In source string: '{1}'", pair, customBet));
                    }

                    results.Add(keyVal[0], long.Parse(keyVal[1]));
                }
            }

            return new ReadOnlyDictionary<string, long>(results);
        }

        /// <inheritdoc />
        public string Encode(IReadOnlyDictionary<string, long> customBetInfo)
        {
            var result = string.Empty;

            if(customBetInfo != null && customBetInfo.Any())
            {
                var tempIdentifier = "";
                foreach(var customInfo in customBetInfo.OrderBy(data => data.Key))
                {
                    if (customInfo.Key.Contains(",") || customInfo.Key.Contains(":"))
                    {
                        throw new FormatException("customBetInfo keys cannot include ',' or ':' they are string format delimiter characters.");
                    }

                    var pair = string.Format("{0}:{1}", customInfo.Key, customInfo.Value);

                    if(!string.IsNullOrEmpty(tempIdentifier))
                    {
                        tempIdentifier += ",";
                    }

                    tempIdentifier += pair;
                }

                result = string.Format("{{{0}}}", tempIdentifier);
            }

            return result;
        }

        #endregion
    }
}