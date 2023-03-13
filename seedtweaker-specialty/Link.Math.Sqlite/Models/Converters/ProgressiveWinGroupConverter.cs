// -----------------------------------------------------------------------
// <copyright file = "SqliteProgressiveWinGroupConverter.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Evaluation.Data;

    /// <summary>
    ///     Converts between an <see cref="IEnumerable{T}"/> of
    ///     <see cref="ProgressiveWinGroup"/> and <see cref="string"/>.
    /// </summary>
    public class ProgressiveWinGroupConverter :
        IConverter<IEnumerable<ProgressiveWinGroup>, string>
    {
        #region Methods

        /// <inheritdoc/>
        /// <remarks>
        ///     It is assumed that the <paramref name="obj"/> is sorted from
        ///     lowest level to highest.
        /// </remarks>
        public IEnumerable<ProgressiveWinGroup> Convert(
            string obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var progressiveWinGroups = new List<ProgressiveWinGroup>();

            foreach(Match match in Regex.Matches(
                obj,
                @"((\d+):(\d+)(,*))"))
            {
                var progressiveLevel =
                    System.Convert.ToInt32(match.Groups[2].Value);
                var progressiveHitCount =
                    System.Convert.ToInt32(match.Groups[3].Value);
                progressiveWinGroups.Add(
                    new ProgressiveWinGroup(
                        progressiveLevel,
                        progressiveHitCount));
            }

            return progressiveWinGroups;
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     It is assumed that the <paramref name="obj"/> is pre sorted
        ///     from lowest level to highest.
        /// </remarks>
        public string Convert(
            IEnumerable<ProgressiveWinGroup> obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if(!obj.Any())
            {
                return string.Empty;
            }

            var progressiveInfo = new StringBuilder();

            foreach(var progressiveWinGroup in obj)
            {
                if(progressiveInfo.Length != 0)
                {
                    progressiveInfo.Append(",");
                }

                progressiveInfo.AppendFormat(
                    "{0}:{1}",
                    progressiveWinGroup.Level,
                    progressiveWinGroup.Count);
            }

            return progressiveInfo.ToString();
        }

        #endregion
    }
}
