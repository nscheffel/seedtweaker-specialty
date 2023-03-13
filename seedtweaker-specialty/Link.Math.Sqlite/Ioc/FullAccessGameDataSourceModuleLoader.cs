// -----------------------------------------------------------------------
// <copyright file = "FullAccessGameDataSourceModuleLoader.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Ioc
{
    using System;
    using System.Data.Common;
    using Evaluation.Data;
    using GameDataSource.CommandHandlers;
    using GameDataSource.QueryHandlers;
    using Link.Ioc;
    using Link.Math.GameDataSource;
    using Link.Math.GameDataSource.Queries;
    using Link.Math.GameDataSource.Commands;
    using Query.Handlers;
    using Command.Handlers;

    /// <summary>
    ///     <see cref="IModuleLoader"/> that will register the necessary
    ///     objects for a <see cref="SqliteFullAccessGameDataSource"/>.
    /// </summary>
    [ModuleFilter(typeof(SqliteFullAccessGameDataSource))]
    public class FullAccessGameDataSourceModuleLoader :
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
                typeof(SqliteFullAccessGameDataSource),
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

            container.Register(
                typeof(ICommandHandler<
                    CreateGameDataSourceAtUriCommand>),
                typeof(
                CreateGameDataSourceAtUriCommandHandler
                ));
        }

        #endregion
    }
}
