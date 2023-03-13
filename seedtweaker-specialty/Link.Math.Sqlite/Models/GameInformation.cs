// -----------------------------------------------------------------------
// <copyright file = "GameInformation.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     DTO representing game information in a SQLite database.
    /// </summary>
    public class GameInformation
    {
        #region Properties

        /// <summary>
        ///     The active <see cref="GameConfiguration"/> of this game.
        /// </summary>
        public int GameConfigurationId { get; set; }

        /// <summary>
        ///     The game section mask, used to mask out different designer
        ///     specified sections.
        /// </summary>
        public int GameSectionMask { get; set; }

        /// <summary>
        ///     The ID of this <see cref="GameInformation"/>.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The number of times this game is present.
        /// </summary>
        public int Occurrences { get; set; }

        /// <summary>
        ///     Blob of RNG data.
        /// </summary>
        public byte[] RawRandomNumbers { get; set; }

        /// <summary>
        ///     The total win amount, unscaled and in base credits.
        /// </summary>
        public long TotalWin { get; set; }

        /// <summary>
        ///     A comma separated list of ProgressiveLevel:HitCount
        ///     going from lowest level to highest level.
        /// </summary>
        /// <example>
        ///     ProgressiveInfo = 0:1,3:2,4:5,6:1
        ///
        ///     1. Progressive level 0 hits 1 time
        ///     2. Progressive level 3 hits 2 times
        ///     3. Progressive level 4 hits 5 times
        ///     4. Progressive level 6 hist 1 time
        /// </example>
        public string ProgressiveInfo { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Convert <see cref="RawRandomNumbers"/> into an
        ///     <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <returns>
        ///     A <see cref="IEnumerable{T}"/> converted from
        ///     <see cref="RawRandomNumbers"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when      <see cref="RawRandomNumbers"/> is
        ///     <see langword="null"/>.
        /// </exception>
        public IEnumerable<int> ConvertRandomNumbers()
        {
            if(RawRandomNumbers == null)
            {
                throw new ArgumentException(
                    "RawRandomNumbers cannot be null.",
                    "");
            }

            var rngs = new int[RawRandomNumbers.Length / sizeof(int)];
            for(var i = 0; i < rngs.Length; i++)
            {
                rngs[i] = BitConverter.ToInt32(
                    RawRandomNumbers,
                    i * sizeof(int));
            }

            return rngs;
        }

        /// <summary>
        ///     Check to see if <paramref name="gameConfig"/> is linear and
        ///     scale the total win of <see cref="TotalWin"/> accordingly.
        /// </summary>
        /// <param name="bet">
        ///     The original <see cref="Bet"/> of the request.
        /// </param>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> to which the
        ///     <see cref="TotalWin"/> belong.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when <see cref="TotalWin"/> cannot be scaled because
        ///     <paramref name="gameConfig"/> is not linear.
        /// </exception>
        public void ScaleTotalWin(
            Bet bet,
            GameConfiguration gameConfig)
        {
            var scalar = bet.TotalBet / gameConfig.TotalBet;
            if(scalar == 1)
            {
                return;
            }

            if(!gameConfig.IsLinear)
            {
                throw new InvalidOperationException(
                    "Cannot scale games for a game configuration that is not linear.");
            }

            TotalWin *= scalar;
        }

        #endregion
    }
}
