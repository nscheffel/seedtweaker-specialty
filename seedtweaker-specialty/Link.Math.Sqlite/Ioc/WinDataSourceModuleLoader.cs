// -----------------------------------------------------------------------
// <copyright file = "WinDataSourceModuleLoader.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Ioc
{
    using System;
    using System.Data.Common;
    using Evaluation.Data;
    using Link.Ioc;
    using Link.Math.WinDataSource;
    using Link.Math.WinDataSource.Queries;
    using Query.Handlers;
    using WinDataSource.QueryHandlers;

    /// <summary>
    ///     <see cref="IModuleLoader"/> that will register the necessary
    ///     objects for a <see cref="SqliteReadOnlyWinDataSource"/>.
    /// </summary>
    [ModuleFilter(typeof(SqliteReadOnlyWinDataSource))]
    public class WinDataSourceModuleLoader :
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
                typeof(IWinDataSource),
                typeof(SqliteReadOnlyWinDataSource),
                Lifetime.Singleton);

            //
            // Register via a predicate to ensure that anything that asked for
            // a IConnectableWinDataSource<DbConnection> uses the same instance
            // that was registered for IWinDataSource.
            //
            container.Register(
                () => (IConnectableWinDataSource<DbConnection>)container
                    .Instance<IWinDataSource>(),
                Lifetime.Singleton);

            container.Register(
                typeof(IQueryHandler<
                    GetWinDataByPaytableConfigurationAndBetQuery,
                    WinData>),
                typeof(
                GetRandomWinDataByPaytableConfigurationAndBetQueryHandler
                ));
        }

        #endregion
    }
}
