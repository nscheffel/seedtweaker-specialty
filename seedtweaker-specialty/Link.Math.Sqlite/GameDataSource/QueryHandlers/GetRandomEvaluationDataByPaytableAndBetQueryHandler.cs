// -----------------------------------------------------------------------
// <copyright file = "GetRandomEvaluationDataByPaytableAndBetQueryHandler.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.GameDataSource.QueryHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using Evaluation.Data;
    using Link.Math.GameDataSource;
    using Link.Math.GameDataSource.Queries;
    using Models;
    using Query.Handlers;
    using RandomNumbers;

    /// <summary>
    ///     Returns <see cref="EvaluationData"/> based on the given
    ///     <see cref="GetRandomEvaluationDataByPaytableAndBetQuery"/>.
    /// </summary>
    internal sealed class GetRandomEvaluationDataByPaytableAndBetQueryHandler :
        IQueryHandler<GetRandomEvaluationDataByPaytableAndBetQuery,
            EvaluationData>
    {
        #region Fields

        /// <summary>
        ///     The <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     to and from <see cref="ProgressiveWinGroup"/>.
        /// </summary>
        private readonly IConverter<IEnumerable<ProgressiveWinGroup>, string>
            progressiveWinGroupConverter;

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     between <see cref="Bet"/> and <see cref="BetConfiguration"/>.
        /// </summary>
        private readonly IConverter<Bet, BetConfiguration> betConverter;

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     between a<see cref="PaytableConfiguration"/>/ <see cref="Bet"/>
        ///     pairing and a <see cref="GameConfiguration"/>.
        /// </summary>
        private readonly
            IConverter<KeyValuePair<PaytableConfiguration, Bet>,
                GameConfiguration> paytableBetConverter;

        /// <summary>
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </summary>
        private readonly IConnectableGameDataSource<DbConnection> dataSource;

        /// <summary>
        ///     The <see cref="IRandomNumberGenerator"/> to use to generate
        ///     random numbers.
        /// </summary>
        private readonly IRandomNumberGenerator rng;

        /// <summary>
        ///     A cache of game data source responses used for speed. Note: It
        ///     is preferred to cache these for speed over memory usage.
        ///     Meaning speed is more important than using memory.
        /// </summary>
        private readonly
            Dictionary<GetRandomEvaluationDataByPaytableAndBetQuery,
                IList<GameInformation>> gameDataSourceResponses;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initialize <see cref="GetRandomEvaluationDataByPaytableAndBetQueryHandler"/>.
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
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     between <see cref="Bet"/> and <see cref="BetConfiguration"/>.
        /// </param>
        /// <param name="paytableBetConverter">
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     between a <see cref="PaytableConfiguration"/>/<see cref="Bet"/>
        ///     pairing and a <see cref="GameConfiguration"/>.
        /// </param>
        /// <param name="progressiveWinGroupConverter">
        ///     The <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     to and from <see cref="ProgressiveWinGroup"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters is <see langword="null"/>.
        /// </exception>
        public GetRandomEvaluationDataByPaytableAndBetQueryHandler(
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

            gameDataSourceResponses =
                new Dictionary<GetRandomEvaluationDataByPaytableAndBetQuery,
                    IList<GameInformation>>();
        }

        #endregion

        #region Methods

        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="query"/> is <see langword="null"/>.
        /// </exception>
        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when evaluation data could not be found.
        /// </exception>
        public EvaluationData Handle(
            GetRandomEvaluationDataByPaytableAndBetQuery query)
        {
            if(query == null)
            {
                throw new ArgumentNullException("query");
            }

            IList<GameInformation> games;

            // Check to see if the response associated with the query has 
            // been cached.
            if(!gameDataSourceResponses.TryGetValue(
                query,
                out games))
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
                    games = connection.GetGames(gameConfig).ToList();

                    foreach(var game in games)
                    {
                        game.ScaleTotalWin(
                            query.Bet,
                            gameConfig);
                    }

                    //cache the response.
                    gameDataSourceResponses.Add(
                        query,
                        games);
                }
            }

            if(games == null)
            {
                throw new InvalidOperationException(
                    "Unable to retrieve the games from the game data source.");
            }

            return GetRandomGame(games);
        }

        /// <summary>
        ///     Retrieve a random <see cref="EvaluationData"/> from
        ///     <paramref name="gameInfos"/>.
        /// </summary>
        /// <param name="gameInfos">
        ///     A list of <see cref="GameInformation"/> from which to choose a
        ///     <see cref="EvaluationData"/>.
        /// </param>
        /// <returns>
        ///     A random <see cref="EvaluationData"/> from
        ///     <paramref name="gameInfos"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="gameInfos"/> is
        ///     <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the evaluation data could not be found.
        /// </exception>
        private EvaluationData GetRandomGame(
            IList<GameInformation> gameInfos)
        {
            if(gameInfos == null)
            {
                throw new ArgumentNullException("gameInfos");
            }

            var gameInformation = gameInfos.GetRandomGameInformation(rng);

            if(gameInformation == null)
            {
                throw new InvalidOperationException(
                    "Unable to retrieve GameInformation.");
            }

            var gameData = new GameData(
                gameInformation.ConvertRandomNumbers(),
                gameInformation.Id.ToString());
            var winData = new WinData(
                gameInformation.TotalWin,
                gameInformation.GameSectionMask,
                progressiveWinGroupConverter.Convert(
                    gameInformation.ProgressiveInfo),
                gameInformation.Id.ToString());

            return new EvaluationData(
                winData,
                gameData);
        }

        #endregion
    }
}
