// -----------------------------------------------------------------------
// <copyright file = "GameConfiguration.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models
{
    /// <summary>
    ///     DTO representing a general game configuration data in a SQLite
    ///     database.
    /// </summary>
    public class GameConfiguration
    {
        #region Properties

        /// <summary>
        ///     The ID of this <see cref="GameConfiguration"/>.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Signifies if the game is linear.
        /// </summary>
        public bool IsLinear { get; set; }

        /// <summary>
        ///     The persistence id.
        /// </summary>
        public int PersistenceId { get; set; }

        /// <summary>
        ///     The corresponding <see cref="BetConfiguration"/>.
        /// </summary>
        public int BetConfigurationId { get; set; }

        /// <summary>
        ///     The total bet associated with this <see cref="GameConfiguration"/>.
        /// </summary>
        public int TotalBet { get; set; }

        /// <summary>
        ///     The paytable index associated with this
        ///     <see cref="GameConfiguration"/>.
        /// </summary>
        public int PaytableIndex { get; set; }

        #endregion
    }
}
