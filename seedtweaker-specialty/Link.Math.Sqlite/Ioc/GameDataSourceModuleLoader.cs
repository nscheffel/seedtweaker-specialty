// -----------------------------------------------------------------------
// <copyright file = "GameDataSourceModuleLoader.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Ioc
{
    using System;
    using System.Data.Common;
    using Evaluation.Data;
    using GameDataSource.QueryHandlers;
    using Link.Ioc;
    using Link.Math.GameDataSource;
    using Link.Math.GameDataSource.Queries;
    using Query.Handlers;

    /// <summary>
    ///     <see cref="IModuleLoader"/> that will register the necessary
    ///     objects for a <see cref="SqliteReadOnlyGameDataSource"/>.
    /// </summary>
    [ModuleFilter(typeof(SqliteReadOnlyGameDataSource))]
    public class GameDataSourceModuleLoader :
        IModuleLoader
    {
        #region Methods

        /// <inheritdoc />
        public void Register(
            IContainer container)
        {
            if(container == null)
            {
                throw new ArgumentNullException("container");
            }

            //
            // To take advantage of caching, register this as a singleton.
            // Caching is optimal due to the time it takes to establish a new
            // connection to the database.
            //
            container.Register(
                typeof(IGameDataSource),
                typeof(SqliteReadOnlyGameDataSource),
                Lifetime.Singleton);

            //
            // Register via a predicate to ensure that anything that asked for
            // a IConnectableGameDataSource<DbConnection> uses the same instance
            // that was registered for IGameDataSource.
            //
            container.Register(
                () => (IConnectableGameDataSource<DbConnection>)container
                    .Instance<IGameDataSource>(),
                Lifetime.Singleton);

            //
            // It'd be swell to register these generically (via IQueryHandler<,>)
            // however that would also register the win data source handlers, 
            // which may be undesired behavior. So here we are, registering them
            // manually like some pleb.
            //

            container.Register(
                typeof(IQueryHandler<
                    GetRandomGameDataByPaytableConfigurationBetAndWinDataQuery,
                    GameData>),
                typeof(
                GetRandomGameDataByPaytableConfigurationBetAndWinDataQueryHandler
                ));

            container.Register(
                typeof(IQueryHandler<
                    GetIncrementalEvaluationDataByPaytableAndBetQuery,
                    EvaluationData>),
                typeof(
                GetIncrementalEvaluationDataByPaytableAndBetQueryHandler
                ),
                Lifetime.Singleton);

            container.Register(
                typeof(IQueryHandler<
                    GetRandomEvaluationDataByPaytableAndBetQuery,
                    EvaluationData>),
                typeof(
                GetRandomEvaluationDataByPaytableAndBetQueryHandler
                ));
        }

        #endregion
    }
}
