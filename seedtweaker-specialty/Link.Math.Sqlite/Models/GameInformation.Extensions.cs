// -----------------------------------------------------------------------
// <copyright file = "GameInformation.Extensions.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Evaluation.Data;
    using RandomNumbers;
    using RandomNumbers.Models;

    /// <summary>
    ///     Extension methods for <see cref="GameInformation"/>.
    /// </summary>
    public static class GameInformationExtensions
    {
        /// <summary>
        ///     Find the <see cref="GameInformation"/> associated
        ///     with the <paramref name="occurrenceIndex"/>.
        /// </summary>
        /// <param name="gameInfos">
        ///     The <see cref="GameInformation"/> associated
        ///     with the <paramref name="occurrenceIndex"/>.
        /// </param>
        /// <param name="occurrenceIndex">
        ///     The occurrence value to use to index into the games.
        /// </param>
        /// <returns>
        ///     The <see cref="GameInformation"/> to retrieve based
        ///     on <paramref name="occurrenceIndex"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a game could not be found.
        /// </exception>
        public static GameInformation GetGameFromOccurrenceIndex(
            this IEnumerable<GameInformation> gameInfos,
            int occurrenceIndex)
        {
            var currentWeight = 0;

            foreach(var info in gameInfos)
            {
                currentWeight += info.Occurrences;
                if(occurrenceIndex < currentWeight)
                {
                    return info;
                }
            }

            throw new InvalidOperationException(
                "Could not find game.");
        }

        /// <summary>
        ///     Retrieve a random <see cref="GameInformation"/> from
        ///     <paramref name="gameInfos"/>.
        /// </summary>
        /// <param name="gameInfos">
        ///     A list of <see cref="GameInformation"/> from which to
        ///     choose a random game.
        /// </param>
        /// <param name="rng">
        ///     The <see cref="IRandomNumberGenerator"/> to use to generate
        ///     random numbers.
        /// </param>
        /// <returns>
        ///     A random <see cref="GameData"/> from <paramref name="gameInfos"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a game could not be found.
        /// </exception>
        public static GameInformation GetRandomGameInformation(
            this IList<GameInformation> gameInfos,
            IRandomNumberGenerator rng)
        {
            if(rng == null)
            {
                throw new ArgumentNullException("rng");
            }

            var totalOccurrences = gameInfos.Sum(g => g.Occurrences);

            var request = new RandomNumberRequest(
                1,
                0,
                totalOccurrences - 1);
            var index = rng.GetRandomNumbers(request).First();
            return gameInfos.GetGameFromOccurrenceIndex(index);
        }
    }
}