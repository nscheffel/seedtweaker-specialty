// -----------------------------------------------------------------------
// <copyright file = "BetConfiguration.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models
{
    /// <summary>
    ///     DTO representing a <see cref="Bet"/> in a SQLite database.
    /// </summary>
    public class BetConfiguration
    {
        #region Properties

        /// <summary>
        ///     The bet per line.
        /// </summary>
        public int BetPerLine { get; set; }

        /// <summary>
        ///     Custom bet information.
        /// </summary>
        public string CustomBetInfo { get; set; }

        /// <summary>
        ///     The extra bet.
        /// </summary>
        public int ExtraBet { get; set; }

        /// <summary>
        ///     The ID of this <see cref="BetConfiguration"/>.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Selected number of lines.
        /// </summary>
        public int Lines { get; set; }

        /// <summary>
        ///     The persistence ID for this <see cref="BetConfiguration"/>.
        /// </summary>
        public int PersistenceId { get; set; }

        /// <summary>
        ///     The side bet.
        /// </summary>
        public int SideBet { get; set; }

        /// <summary>
        ///     The bet configuration total bet.
        /// </summary>
        public int TotalBet { get; set; }

        #endregion
    }
}
