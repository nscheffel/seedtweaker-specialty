// -----------------------------------------------------------------------
// <copyright file = "GetIncrementalEvaluationDataByPaytableAndBetQueryHandler.cs" company = "IGT">
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

    /// <summary>
    ///     Returns <see cref="EvaluationData"/> based on the given
    ///     <see cref="GetIncrementalEvaluationDataByPaytableAndBetQuery"/>.
    /// </summary>
    internal sealed class
        GetIncrementalEvaluationDataByPaytableAndBetQueryHandler :
            IQueryHandler<GetIncrementalEvaluationDataByPaytableAndBetQuery,
                EvaluationData>
    {
        #region Fields

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     between <see cref="Bet"/> and <see cref="BetConfiguration"/>.
        /// </summary>
        private readonly IConverter<Bet, BetConfiguration> betConverter;

        /// <summary>
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </summary>
        private readonly IConnectableGameDataSource<DbConnection> dataSource;

        /// <summary>
        ///     A cache of game data source responses used for speed.
        /// 
        ///     Note: It is preferred to cache these for speed over memory
        ///     usage. Meaning speed is more important than using memory.
        /// </summary>
        private readonly
            Dictionary<GetIncrementalEvaluationDataByPaytableAndBetQuery,
                IList<GameInformation>> gameDataSourceResponses;

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     between a <see cref="PaytableConfiguration"/>/<see cref="Bet"/>
        ///     pairing and a <see cref="GameConfiguration"/>.
        /// </summary>
        private readonly
            IConverter<KeyValuePair<PaytableConfiguration, Bet>,
                GameConfiguration> paytableBetConverter;

        /// <summary>
        ///     The <see cref="IConverter{TFirst,TSecond}"/> to use to convert
        ///     to and from <see cref="ProgressiveWinGroup"/>.
        /// </summary>
        private readonly IConverter<IEnumerable<ProgressiveWinGroup>, string>
            progressiveWinGroupConverter;

        /// <summary>
        ///     Structure used to keep track of current index for each query.
        /// </summary>
        private readonly
            Dictionary<GetIncrementalEvaluationDataByPaytableAndBetQuery, int>
            queryTracker;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initialize <see cref="GetIncrementalEvaluationDataByPaytableAndBetQueryHandler"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
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
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters is <see langword="null"/>.
        /// </exception>
        public GetIncrementalEvaluationDataByPaytableAndBetQueryHandler(
            IConnectableGameDataSource<DbConnection> dataSource,
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
            this.betConverter = betConverter;
            this.paytableBetConverter = paytableBetConverter;
            this.progressiveWinGroupConverter = progressiveWinGroupConverter;

            gameDataSourceResponses =
                new Dictionary<
                    GetIncrementalEvaluationDataByPaytableAndBetQuery,
                    IList<GameInformation>>();
            queryTracker =
                new Dictionary<
                    GetIncrementalEvaluationDataByPaytableAndBetQuery,
                    int>();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public EvaluationData Handle(
            GetIncrementalEvaluationDataByPaytableAndBetQuery query)
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
                using(var connection = dataSource.Connect())
                {
                    var queryGameConfig = paytableBetConverter.Convert(
                        new KeyValuePair<PaytableConfiguration, Bet>(
                            query.PaytableConfiguration,
                            query.Bet));
                    var queryBetConfig = betConverter.Convert(query.Bet);

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

                    // Cache the response.
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

            return GetIncrementalGame(
                query,
                games);
        }

        /// <summary>
        ///     Retrieve the next <see cref="EvaluationData"/> from <paramref name="games"/>.
        /// </summary>
        /// <param name="query">
        ///     The <see cref="GetIncrementalEvaluationDataByPaytableAndBetQuery"/> used.
        /// </param>
        /// <param name="games">
        ///     A list of <see cref="GameInformation"/> from which to choose the next game.
        /// </param>
        /// <returns>
        ///     The next <see cref="EvaluationData"/> from <paramref name="games"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the evaluation data could not be found.
        /// </exception>
        private EvaluationData GetIncrementalGame(
            GetIncrementalEvaluationDataByPaytableAndBetQuery query,
            IList<GameInformation> games)
        {
            var gameInformation = GetNextGameInformation(
                query,
                games);

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

        /// <summary>
        ///     Retrieve the next <see cref="GameInformation"/> from <paramref name="games"/>.
        /// </summary>
        /// <param name="query">
        ///     The <see cref="GetIncrementalEvaluationDataByPaytableAndBetQuery"/> used.
        /// </param>
        /// <param name="games">
        ///     A list of <see cref="GameInformation"/> from which to choose the next game.
        /// </param>
        /// <returns>
        ///     The next <see cref="GameInformation"/> from <paramref name="games"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the evaluation data could not be found.
        /// </exception>
        private GameInformation GetNextGameInformation(
            GetIncrementalEvaluationDataByPaytableAndBetQuery query,
            IList<GameInformation> games)
        {
            GameInformation gameInformation = null;

            var currentGameIndex = RetrieveNextIndex(query);

            // if a valid index
            if(currentGameIndex >= 0 && currentGameIndex < games.Count)
            {
                gameInformation = games[currentGameIndex];
            }

            return gameInformation;
        }

        /// <summary>
        ///     Retrieve the next index associated with the
        ///     <see cref="GetIncrementalEvaluationDataByPaytableAndBetQuery"/>.
        /// </summary>
        /// <param name="query">
        ///     The <see cref="GetIncrementalEvaluationDataByPaytableAndBetQuery"/> used.
        /// </param>
        /// <returns>
        ///     The next index associated with the
        ///     <see cref="GetIncrementalEvaluationDataByPaytableAndBetQuery"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when<paramref name="query"/> is <see langword="null"/>.
        /// </exception>
        private int RetrieveNextIndex(
            GetIncrementalEvaluationDataByPaytableAndBetQuery query)
        {
            if(query == null)
            {
                throw new ArgumentNullException("query");
            }

            int currentGameIndex;

            // if we haven't seen this query before, add it
            if(!queryTracker.TryGetValue(
                query,
                out currentGameIndex))
            {
                queryTracker.Add(
                    query,
                    0);
            }

            // move to the next outcome
            queryTracker[query]++;
            return currentGameIndex;
        }

        #endregion
    }
}
