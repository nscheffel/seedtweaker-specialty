// -----------------------------------------------------------------------
// <copyright file = "DbConnection.Extensions.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Dapper;
    using Models;

    /// <summary>
    ///     Data source Extension methods for <see cref="IDbConnection"/>.
    /// </summary>
    public static class DbConnectionExtensions
    {
        #region Methods

        /// <summary>
        ///     Find a <see cref="BetConfiguration"/> valid for
        ///     <paramref name="bet"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="bet">
        ///     The <see cref="BetConfiguration"/> against which the returning
        ///     <see cref="GameConfiguration"/>
        ///     should be valid.
        /// </param>
        /// <returns>
        ///     A <see cref="BetConfiguration"/> valid for
        ///     <paramref name="bet"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     One of the parameters is <see langword="null"/>.
        /// </exception>
        public static BetConfiguration FindBetConfiguration(
            this IDbConnection connection,
            BetConfiguration bet)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if(bet == null)
            {
                throw new ArgumentNullException("bet");
            }

            var betConfig = GetExactBetConfiguration(
                connection,
                bet);

            if(betConfig != null)
            {
                return betConfig;
            }

            return GetLinearBetConfiguration(
                connection,
                bet);
        }

        /// <summary>
        ///     Get all <see cref="BetConfiguration"/>s in the data source.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <returns>
        ///     All <see cref="BetConfiguration"/>s in the data source.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connection"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<BetConfiguration> GetAllBetConfigurations(
            this IDbConnection connection)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            const string query = "SELECT * FROM BetConfiguration";

            return connection.Query<BetConfiguration>(
                query,
                new { });
        }

        /// <summary>
        ///     Get all <see cref="BetConfiguration"/>s from the data source
        ///     belonging to
        ///     <paramref name="gameConfig"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> whose
        ///     <see cref="BetConfiguration"/> should be returned.
        /// </param>
        /// <returns>
        ///     All <see cref="BetConfiguration"/>s from the data source
        ///     belonging to <paramref name="gameConfig"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     One of the parameters is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<BetConfiguration> GetAllBetConfigurations(
            this IDbConnection connection,
            GameConfiguration gameConfig)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if(gameConfig == null)
            {
                throw new ArgumentNullException("gameConfig");
            }

            const string query =
                "SELECT BetConfiguration.*, GameConfiguration.TotalBet " +
                "FROM BetConfiguration " +
                "INNER JOIN GameConfiguration " +
                "ON BetConfigurationId = GameConfiguration.BetConfigurationId " +
                "WHERE GameConfiguration.PaytableIndex = @PaytableIndex";

            return connection.Query<BetConfiguration>(
                query,
                new
                {
                    gameConfig.PaytableIndex
                });
        }

        /// <summary>
        ///     Get all <see cref="GameConfiguration"/> in the data source.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <returns>
        ///     All <see cref="GameConfiguration"/>s in the data source.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connection"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<GameConfiguration> GetAllGameConfigurations(
            this IDbConnection connection)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            const string query = "SELECT PaytableIndex FROM GameConfiguration";

            return connection.Query<GameConfiguration>(
                query,
                new { });
        }

        /// <summary>
        ///     Get the <see cref="GameInformation"/> matching
        ///     <paramref name="id"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="id">
        ///     The ID of the <see cref="GameInformation"/> to retrieve.
        /// </param>
        /// <returns>
        ///     The <see cref="GameInformation"/> matching <paramref name="id"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connection"/> is <see langword="null"/>.
        /// </exception>
        public static GameInformation GetGame(
            this IDbConnection connection,
            int id)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            const string query =
                "SELECT * FROM Games " +
                "WHERE Id = @Id";

            return connection.Query<GameInformation>(
                    query,
                    new
                    {
                        Id = id
                    })
                .Single();
        }


        /// <summary>
        ///     Get the closest <see cref="GameConfiguration"/> whose
        ///     <see cref="GameConfiguration.PaytableIndex"/> matches
        ///     <paramref name="paytableIndex"/>. This will include linear a
        ///     <see cref="GameConfiguration"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="paytableIndex">
        ///     The paytable index of the <see cref="GameConfiguration"/> to
        ///     retrieve.
        /// </param>
        /// <returns>
        ///     The closest <see cref="GameConfiguration"/> whose
        ///     <see cref="GameConfiguration.PaytableIndex"/> matches
        ///     <paramref name="paytableIndex"/>. This will include linear a
        ///     <see cref="GameConfiguration"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connection"/> is <see langword="null"/>.
        /// </exception>
        public static GameConfiguration GetGameConfiguration(
            this IDbConnection connection,
            int paytableIndex)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            const string query =
                "SELECT * FROM GameConfiguration " +
                "WHERE PaytableIndex = @PaytableIndex " +
                "LIMIT 1";

            return connection.Query<GameConfiguration>(
                    query,
                    new
                    {
                        paytableIndex,
                    })
                .Single();
        }

        /// <summary>
        ///     Get the closest <see cref="GameConfiguration"/> matching
        ///     <paramref name="gameConfig"/> and <paramref name="bet"/>. This
        ///     will include linear a <see cref="GameConfiguration"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> to match.
        /// </param>
        /// <param name="bet">
        ///     The <see cref="BetConfiguration"/> whose returning
        ///     <see cref="GameConfiguration"/> should include.
        /// </param>
        /// <returns>
        ///     The closest <see cref="GameConfiguration"/> matching
        ///     <paramref name="gameConfig"/> and <paramref name="bet"/>. This
        ///     will include linear a <see cref="GameConfiguration"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     One of the parameters is <see langword="null"/>.
        /// </exception>
        public static GameConfiguration GetGameConfiguration(
            this IDbConnection connection,
            GameConfiguration gameConfig,
            BetConfiguration bet)
        {
            var betConfig = FindBetConfiguration(
                connection,
                bet);

            if(betConfig == null)
            {
                throw new InvalidOperationException(
                    "No matching bet configuration found in data source.");
            }

            const string query =
                "SELECT * FROM GameConfiguration " +
                "WHERE BetConfigurationId = @BetConfigurationId AND " +
                    "TotalBet = @TotalBet AND " +
                    "PersistenceId = @PersistenceId AND " +
                    "PaytableIndex = @PaytableIndex " +
                "LIMIT 1";

            var matchingGameConfig = connection.Query<GameConfiguration>(
                    query,
                    new
                    {
                        BetConfigurationId = betConfig.Id,
                        gameConfig.PaytableIndex,
                        gameConfig.PersistenceId,
                        betConfig.TotalBet
                    })
                .Single();

            if(matchingGameConfig == null)
            {
                throw new InvalidOperationException(
                    "No matching game configuration found in data source.");
            }

            //
            // The game configuration is not linear, however bet configuration
            // found assume a linear bet.
            //
            if(!matchingGameConfig.IsLinear &&
                betConfig.BetPerLine != bet.BetPerLine)
            {
                throw new InvalidOperationException(
                    "No matching non-linear game configuration found in data source.");
            }

            return matchingGameConfig;
        }

        /// <summary>
        ///     Get all <see cref="GameInformation"/> belonging to
        ///     <paramref name="gameConfig"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> of the
        ///     <see cref="GameInformation"/>s to retrieve.
        /// </param>
        /// <returns>
        ///     All <see cref="GameInformation"/> belonging to
        ///     <paramref name="gameConfig"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     One of the parameters is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<GameInformation> GetGames(
            this IDbConnection connection,
            GameConfiguration gameConfig)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if(gameConfig == null)
            {
                throw new ArgumentNullException("gameConfig");
            }

            const string query =
                "SELECT * " +
                "FROM Games " +
                "WHERE GameConfigurationId = @GameConfigurationId";

            return connection.Query<GameInformation>(
                query,
                new
                {
                    GameConfigurationId = gameConfig.Id
                });
        }

        /// <summary>
        ///     Get all <see cref="GameInformation"/> matching
        ///     <paramref name="gameConfig"/>, <paramref name="totalWin"/>, and
        ///     <paramref name="progInfo"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> of the
        ///     <see cref="GameInformation"/>s to retrieve.
        /// </param>
        /// <param name="totalWin">
        ///     The total win of the <see cref="GameInformation"/> to retrieve.
        /// </param>
        /// <param name="progInfo">
        ///     The progressive information of the <see cref="GameInformation"/>
        ///     to retrieve.
        /// </param>
        /// <returns>
        ///     Get all <see cref="GameInformation"/> matching
        ///     <paramref name="gameConfig"/>, <paramref name="totalWin"/>, and
        ///     <paramref name="progInfo"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     One of the parameters is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<GameInformation> GetGames(
            this IDbConnection connection,
            GameConfiguration gameConfig,
            long totalWin,
            string progInfo)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if(gameConfig == null)
            {
                throw new ArgumentNullException("gameConfig");
            }

            const string query =
                "SELECT * " +
                "FROM Games " +
                "WHERE GameConfigurationId = @GameConfigurationId AND " +
                    "TotalWin = @TotalWin AND " +
                    "ProgressiveInfo = @ProgressiveInfo";

            return connection.Query<GameInformation>(
                query,
                new
                {
                    GameConfigurationId = gameConfig.Id,
                    TotalWin = totalWin,
                    ProgressiveInfo = progInfo
                });
        }

        /// <summary>
        ///     Get all <see cref="GameInformation"/> belonging to
        ///     <paramref name="gameConfig"/> without retrieving the
        ///     <see cref="GameInformation.RawRandomNumbers"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> of the
        ///     <see cref="GameInformation"/>s to retrieve.
        /// </param>
        /// <returns>
        ///     All <see cref="GameInformation"/> belonging to
        ///     <paramref name="gameConfig"/> without retrieving the
        ///     <see cref="GameInformation.RawRandomNumbers"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     One of the parameters is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<GameInformation> GetGamesWithoutRngs(
            this IDbConnection connection,
            GameConfiguration gameConfig)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if(gameConfig == null)
            {
                throw new ArgumentNullException("gameConfig");
            }

            const string query =
                "SELECT Id, TotalWin, GameSectionMask, Occurrences, " +
                    "GameConfigurationId, ProgressiveInfo " +
                "FROM Games " +
                "WHERE GameConfigurationId = @GameConfigurationId";

            return connection.Query<GameInformation>(
                query,
                new
                {
                    GameConfigurationId = gameConfig.Id
                });
        }

        /// <summary>
        ///     Get all <see cref="GameInformation"/> matching
        ///     <paramref name="gameConfig"/>, <paramref name="totalWin"/>, and
        ///     <paramref name="progInfo"/> without retrieving the
        ///     <see cref="GameInformation.RawRandomNumbers"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> of the
        ///     <see cref="GameInformation"/>s to retrieve.
        /// </param>
        /// <param name="totalWin">
        ///     The total win of the <see cref="GameInformation"/> to retrieve.
        /// </param>
        /// <param name="progInfo">
        ///     The progressive information of the <see cref="GameInformation"/>
        ///     to retrieve.
        /// </param>
        /// <returns>
        ///     Get all <see cref="GameInformation"/> matching
        ///     <paramref name="gameConfig"/>, <paramref name="totalWin"/>, and
        ///     <paramref name="progInfo"/> without retrieving the
        ///     <see cref="GameInformation.RawRandomNumbers"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     One of the parameters is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<GameInformation>
            GetGamesWithoutRngs(
                this IDbConnection connection,
                GameConfiguration gameConfig,
                long totalWin,
                string progInfo)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if(gameConfig == null)
            {
                throw new ArgumentNullException("gameConfig");
            }

            const string query =
                "SELECT Id, TotalWin, GameSectionMask, Occurrences, " +
                    "GameConfigurationId, ProgressiveInfo " +
                "FROM Games " +
                "WHERE GameConfigurationId = @GameConfigurationId AND " +
                    "TotalWin = @TotalWin AND " +
                    "ProgressiveInfo = @ProgressiveInfo";

            return connection.Query<GameInformation>(
                query,
                new
                {
                    GameConfigurationId = gameConfig.Id,
                    TotalWin = totalWin,
                    ProgressiveInfo = progInfo
                });
        }

        /// <summary>
        ///     Get the exact <see cref="BetConfiguration"/> matching the values
        ///     in <paramref name="bet"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="bet">
        ///     The <see cref="BetConfiguration"/> that should be matched from
        ///     the data source.
        /// </param>
        /// <returns>
        ///     The exact <see cref="BetConfiguration"/> matching the values
        ///     in <paramref name="bet"/>.
        /// </returns>
        private static BetConfiguration GetExactBetConfiguration(
            IDbConnection connection,
            BetConfiguration bet)
        {
            // Get a bet configuration with an exact match.
            const string query =
                "SELECT BetConfiguration.*, GameConfiguration.TotalBet " +
                "FROM BetConfiguration " +
                "INNER JOIN GameConfiguration " +
                "ON BetConfiguration.Id = GameConfiguration.BetConfigurationId " +
                "WHERE Lines = @Lines AND " +
                    "BetPerLine = @BetPerLine AND " +
                    "SideBet = @SideBet AND " +
                    "ExtraBet = @ExtraBet AND " +
                    "CustomBetInfo = @CustomBetInfo AND " +
                    "TotalBet = @TotalBet " +
                "LIMIT 1";

            return connection.Query<BetConfiguration>(
                    query,
                    bet)
                .SingleOrDefault();
        }

        /// <summary>
        ///     Get a <see cref="BetConfiguration"/> that is the linear
        ///     equivalent of in <paramref name="bet"/>.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="IDbConnection"/> used to communicate with the
        ///     data source.
        /// </param>
        /// <param name="bet">
        ///     The <see cref="BetConfiguration"/> that should be matched from
        ///     the data source.
        /// </param>
        /// <returns>
        ///     A <see cref="BetConfiguration"/> that is the linear
        ///     equivalent of in <paramref name="bet"/>.
        /// </returns>
        private static BetConfiguration GetLinearBetConfiguration(
            IDbConnection connection,
            BetConfiguration bet)
        {
            // Get bets that match the lines and custom bet data.
            const string query =
                "SELECT BetConfiguration.*, GameConfiguration.TotalBet " +
                "FROM BetConfiguration " +
                "INNER JOIN GameConfiguration " +
                "ON BetConfiguration.Id = GameConfiguration.BetConfigurationId " +
                "WHERE Lines = @Lines AND " +
                    "CustomBetInfo = @CustomBetInfo AND " +
                    "IsLinear = 1";

            var potentialBets = connection.Query<BetConfiguration>(
                query,
                bet);

            return potentialBets.Single(
                betConfig =>
                {
                    var multiplier = bet.TotalBet / betConfig.TotalBet;

                    return bet.TotalBet == betConfig.TotalBet * multiplier &&
                        bet.BetPerLine == betConfig.BetPerLine * multiplier;
                });
        }

        /// <summary>
        ///     Get the value for a specific Key value from the
        ///     "GameData" table.
        /// </summary>
        /// <param name="connection">
        ///     The connection to the database.
        /// </param>
        /// <param name="key">
        ///     The key value to lookup data in the "GameData" table
        ///     of the database.
        /// </param>
        /// <returns>
        ///     The value found for the associated key.
        /// </returns>
        public static string GetCustomDataPropertyByKey(
            this IDbConnection connection,
            string key)
        {
            var queryString = "SELECT Value " +
                              "FROM GameData " +
                              "WHERE Key == @Key LIMIT 1";

            return connection.Query<string>(queryString,
                new
                {
                    Key = key
                }).Single();
        }

        /// <summary>
        ///     Get all of the custom data properties from the
        ///     "GameData" table.
        /// </summary>
        /// <param name="connection">
        ///     The connection to the database.
        /// </param>
        /// <returns>
        ///     The collection of custom data pairs from the database.
        /// </returns>
        public static IDictionary<string, string> GetAllCustomDataProperties(
            this IDbConnection connection)
        {
            var queryString = "SELECT Key, Value " +
                              "FROM GameData";

            return connection.Query<KeyValuePair<string, string>>(queryString, null)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        #endregion
    }
}
