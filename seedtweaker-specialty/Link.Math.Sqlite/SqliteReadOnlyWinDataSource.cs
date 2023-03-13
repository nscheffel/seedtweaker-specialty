// -----------------------------------------------------------------------
// <copyright file = "SqliteReadOnlyWinDataSource.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.Linq;
    using Evaluation.Data;
    using Link.Math.WinDataSource;
    using Models;

    /// <summary>
    ///     Create a read-only SqLite connection to a
    ///     <see cref="IWinDataSource"/>.
    /// </summary>
    public class SqliteReadOnlyWinDataSource :
        IConnectableWinDataSource<DbConnection>
    {
        #region Fields

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> for converted between
        ///     <see cref="Bet"/> and <see cref="BetConfiguration"/>.
        /// </summary>
        private readonly IConverter<Bet, BetConfiguration> betConverter;

        /// <summary>
        ///     The <see cref="IWinDataSourceFinder"/> to use to determine
        ///     the <see cref="Uri"/> of the SqLite file.
        /// </summary>
        private readonly IWinDataSourceFinder finder;

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> for converting
        ///     between <see cref="PaytableConfiguration"/> and
        ///     <see cref="GameConfiguration"/>.
        /// </summary>
        private readonly IConverter<PaytableConfiguration, GameConfiguration>
            paytableConfigConverter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initialize <see cref="SqliteReadOnlyWinDataSource"/>.
        /// </summary>
        /// <param name="sourceFinder">
        ///     The <see cref="IWinDataSourceFinder"/> to use to determine
        ///     the <see cref="Uri"/> of the SqLite file.
        /// </param>
        /// <param name="betConverter">
        ///     <see cref="IConverter{TFirst,TSecond}"/> for converted between
        ///     <see cref="Bet"/> and <see cref="BetConfiguration"/>.
        /// </param>
        /// <param name="paytableConfigConverter">
        ///     <see cref="IConverter{TFirst,TSecond}"/> for converting
        ///     between <see cref="PaytableConfiguration"/> and
        ///     <see cref="GameConfiguration"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     One of the parameters is <see langword="null"/>.
        /// </exception>
        public SqliteReadOnlyWinDataSource(
            IWinDataSourceFinder sourceFinder,
            IConverter<Bet, BetConfiguration> betConverter,
            IConverter<PaytableConfiguration, GameConfiguration>
                paytableConfigConverter)
        {
            if(sourceFinder == null)
            {
                throw new ArgumentNullException("sourceFinder");
            }

            if(betConverter == null)
            {
                throw new ArgumentNullException("betConverter");
            }

            if(paytableConfigConverter == null)
            {
                throw new ArgumentNullException("paytableConfigConverter");
            }

            finder = sourceFinder;
            this.betConverter = betConverter;
            this.paytableConfigConverter = paytableConfigConverter;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public DbConnection Connect()
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = finder.ResolveUri().LocalPath,
                CacheSize = 10000,
                PageSize = 4096,
                Pooling = true,
                FailIfMissing = true,
                ReadOnly = true,
                SyncMode = SynchronizationModes.Off,
                JournalMode = SQLiteJournalModeEnum.Off,
            };

            return new SQLiteConnection(builder.ConnectionString);
        }

        /// <inheritdoc />
        public IEnumerable<Bet> GetAllBets(
            PaytableConfiguration paytableConfig)
        {
            using(var connection = Connect())
            {
                var gameConfig = connection.GetGameConfiguration(
                    paytableConfigConverter.Convert(paytableConfig)
                        .PaytableIndex);
                return connection.GetAllBetConfigurations(gameConfig)
                    .Select(bet =>
                        {
                            bet.PersistenceId =
                                connection.GetGameConfiguration(
                                        gameConfig,
                                        bet)
                                    .PersistenceId;
                            return betConverter.Convert(bet);
                        })
                    .ToList();
            }
        }

        /// <inheritdoc />
        public IEnumerable<PaytableConfiguration> GetAllPaytableConfigurations()
        {
            using(var connection = Connect())
            {
                return connection.GetAllGameConfigurations()
                    .Select(paytableConfigConverter.Convert)
                    .ToList();
            }
        }

        #endregion
    }
}
