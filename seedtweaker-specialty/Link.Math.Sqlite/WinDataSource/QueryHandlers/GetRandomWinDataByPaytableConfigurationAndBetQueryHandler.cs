// -----------------------------------------------------------------------
// <copyright file = "GetRandomWinDataByPaytableConfigurationAndBetQueryHandler.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.WinDataSource.QueryHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using Evaluation.Data;
    using Link.Math.WinDataSource;
    using Link.Math.WinDataSource.Queries;
    using Models;
    using Query.Handlers;
    using RandomNumbers;

    /// <summary>
    ///     Returns <see cref="GameData"/> based on the given
    ///     <see cref="GetWinDataByPaytableConfigurationAndBetQuery"/>.
    /// </summary>
    internal sealed class
        GetRandomWinDataByPaytableConfigurationAndBetQueryHandler :
            IQueryHandler<GetWinDataByPaytableConfigurationAndBetQuery, WinData>
    {
        #region Fields

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert between
        ///     <see cref="Bet"/> and <see cref="BetConfiguration"/>.
        /// </summary>
        private readonly IConverter<Bet, BetConfiguration> betConverter;

        /// <summary>
        ///     The <see cref="IConnectableWinDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </summary>
        private readonly IConnectableWinDataSource<DbConnection> dataSource;

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
        ///     Initialize <see cref="GetRandomWinDataByPaytableConfigurationAndBetQueryHandler"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The <see cref="IConnectableWinDataSource{T}"/> to use to
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
            GetRandomWinDataByPaytableConfigurationAndBetQueryHandler(
                IConnectableWinDataSource<DbConnection> dataSource,
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
        public WinData Handle(
            GetWinDataByPaytableConfigurationAndBetQuery query)
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
                var games = connection
                    .GetGamesWithoutRngs(gameConfig)
                    .ToList();
                var game = games.GetRandomGameInformation(
                    rng);

                game = connection.GetGame(game.Id);

                // Need to scale the total win, as gameConfig might be a linear
                // GameConfiguration.
                game.TotalWin *= queryBetConfig.TotalBet / gameConfig.TotalBet;

                return new WinData(
                    game.TotalWin,
                    game.GameSectionMask,
                    progressiveWinGroupConverter.Convert(game.ProgressiveInfo),
                    game.Id.ToString());
            }
        }

        #endregion
    }
}
