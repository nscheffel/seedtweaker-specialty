// -----------------------------------------------------------------------
// <copyright file = "GetRandomGameDataByPaytableConfigurationBetAndWinDataQueryHandler.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.GameDataSource.QueryHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using Evaluation.Data;
    using Link.Math.GameDataSource;
    using Link.Math.GameDataSource.Queries;
    using Models;
    using Query.Handlers;
    using RandomNumbers;

    /// <summary>
    ///     Returns <see cref="GameData"/> based on the given
    ///     <see cref="GetRandomGameDataByPaytableConfigurationBetAndWinDataQuery"/>.
    /// </summary>
    internal sealed class
        GetRandomGameDataByPaytableConfigurationBetAndWinDataQueryHandler :
            IQueryHandler<                
                GetRandomGameDataByPaytableConfigurationBetAndWinDataQuery,
                GameData>
    {
        #region Fields

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert between
        ///     <see cref="Bet"/> and <see cref="BetConfiguration"/>.
        /// </summary>
        private readonly IConverter<Bet, BetConfiguration> betConverter;

        /// <summary>
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </summary>
        private readonly IConnectableGameDataSource<DbConnection> dataSource;

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to
        ///     convert between a <see cref="PaytableConfiguration"/>/
        ///     <see cref="Bet"/> pairing and a <see cref="GameConfiguration"/>.
        /// </summary>
        private readonly
            IConverter<KeyValuePair<PaytableConfiguration, Bet>,
                GameConfiguration> paytableBetConverter;

        /// <summary>
        ///     The <see cref="IConverter{TFirst,TSecond}"/>
        ///     to use to convert to and from <see cref="ProgressiveWinGroup"/>.
        /// </summary>
        private readonly IConverter<IEnumerable<ProgressiveWinGroup>, string>
            progressiveWinGroupConverter;

        /// <summary>
        ///     The <see cref="IRandomNumberGenerator"/> to use to generate
        ///     random numbers.
        /// </summary>
        private readonly IRandomNumberGenerator rng;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initialize <see cref="GetRandomGameDataByPaytableConfigurationBetAndWinDataQueryHandler"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </param>
        /// <param name="rng">
        ///     The <see cref="IRandomNumberGenerator"/> to use to generate
        ///     random numbers.
        /// </param>
        /// <param name="betConverter">
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert between
        ///     <see cref="Bet"/> and <see cref="BetConfiguration"/>.
        /// </param>
        /// <param name="paytableBetConverter">
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to
        ///     convert between a <see cref="PaytableConfiguration"/>/
        ///     <see cref="Bet"/> pairing and a <see cref="GameConfiguration"/>.
        /// </param>
        /// <param name="progressiveWinGroupConverter">
        ///     The <see cref="IConverter{TFirst,TSecond}"/>
        ///     to use to convert to and from <see cref="ProgressiveWinGroup"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters is <see langword="null"/>.
        /// </exception>
        public
            GetRandomGameDataByPaytableConfigurationBetAndWinDataQueryHandler(
                IConnectableGameDataSource<DbConnection> dataSource,
                IRandomNumberGenerator rng,
                IConverter<Bet, BetConfiguration> betConverter,
                IConverter<KeyValuePair<PaytableConfiguration, Bet>,
                    GameConfiguration> paytableBetConverter,
                IConverter<IEnumerable<ProgressiveWinGroup>, string>
                    progressiveWinGroupConverter)
        {
            if(dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }

            if(rng == null)
            {
                throw new ArgumentNullException("rng");
            }

            if(betConverter == null)
            {
                throw new ArgumentNullException("betConverter");
            }

            if(paytableBetConverter == null)
            {
                throw new ArgumentNullException("paytableBetConverter");
            }

            if(progressiveWinGroupConverter == null)
            {
                throw new ArgumentNullException("progressiveWinGroupConverter");
            }

            this.dataSource = dataSource;
            this.rng = rng;
            this.betConverter = betConverter;
            this.paytableBetConverter = paytableBetConverter;
            this.progressiveWinGroupConverter = progressiveWinGroupConverter;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a game could not be found.
        /// </exception>
        public GameData Handle(
            GetRandomGameDataByPaytableConfigurationBetAndWinDataQuery query)
        {
            var queryGameConfig = paytableBetConverter.Convert(
                new KeyValuePair<PaytableConfiguration, Bet>(
                    query.PaytableConfiguration,
                    query.Bet));
            var queryBetConfig = betConverter.Convert(query.Bet);

            using(var connection = dataSource.Connect())
            {
                var gameConfig = connection.GetGameConfiguration(
                    queryGameConfig,
                    queryBetConfig);

                var adjustedTotalWin = GetAdjustedTotalWin(
                    gameConfig,
                    query.Bet,
                    query.Win);

                var gameInfo = connection.GetGamesWithoutRngs(
                        gameConfig,
                        adjustedTotalWin,
                        progressiveWinGroupConverter.Convert(
                            query.Win.Progressives))
                    .ToList();
                    
                return GetRandomGame(
                    gameInfo,
                    connection);
            }
        }

        /// <summary>
        ///     Get the total win for <paramref name="win"/> adjusted by any
        ///     linear considerations of <paramref name="gameConfig"/>.
        /// </summary>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> to check against for
        ///     linearity.
        /// </param>
        /// <param name="bet">
        ///     The desired <see cref="Bet"/>. Used to compare against
        ///     <paramref name="gameConfig"/> to determine scaling amount.
        /// </param>
        /// <param name="win">
        ///     The <see cref="WinData"/> containing the total win that should
        ///     bet adjusted.
        /// </param>
        /// <returns>
        ///     The adjusted total win amount of <paramref name="win"/>.
        /// </returns>
        private static long GetAdjustedTotalWin(
            GameConfiguration gameConfig,
            Bet bet,
            WinData win)
        {
            if(!gameConfig.IsLinear)
            {
                return win.TotalWin;
            }

            var dbMultiplier = bet.TotalBet / gameConfig.TotalBet;
            var scaledWin = win.TotalWin / dbMultiplier;

            if(scaledWin * dbMultiplier != win.TotalWin)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} does not scale evenly with a divisor of {1}.",
                        win.TotalWin,
                        scaledWin));
            }

            return scaledWin;
        }

        /// <summary>
        ///     Get a random <see cref="GameData"/> based on
        ///     <paramref name="gameInfos"/>.
        /// </summary>
        /// <param name="gameInfos">
        ///     The <see cref="GameInformation"/> containing the ID/Occurrences
        ///     to use to determine the ID to retrieve.
        /// </param>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> to use to query the database.
        /// </param>
        /// <returns>
        ///     A random <see cref="GameData"/> based on
        ///     <paramref name="gameInfos"/>.
        /// </returns>
        private GameData GetRandomGame(
            IList<GameInformation> gameInfos,
            IDbConnection connection)
        {
            var game = gameInfos.GetRandomGameInformation(
                rng);
            game = connection.GetGame(game.Id);
            var randomNumbers = game.ConvertRandomNumbers();

            return new GameData(
                randomNumbers,
                game.Id.ToString());
        }

        #endregion
    }
}
