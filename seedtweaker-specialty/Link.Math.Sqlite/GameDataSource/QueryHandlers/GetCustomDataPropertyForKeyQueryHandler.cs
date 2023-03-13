// -----------------------------------------------------------------------
// <copyright file = "GetCustomDataPropertyForKeyQueryHandler.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.GameDataSource.QueryHandlers
{
    using System;
    using System.Data.Common;
    using Link.Math.GameDataSource;
    using Link.Math.GameDataSource.Queries;
    using Link.Math.Sqlite;
    using Query.Handlers;

    /// <summary>
    ///     Query handler for the <see cref="GetCustomDataPropertyForKeyQuery"/>.
    /// </summary>
    internal sealed class GetCustomDataPropertyForKeyQueryHandler :
        IQueryHandler<GetCustomDataPropertyForKeyQuery, string>
    {
        /// <summary>
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </summary>
        private readonly IConnectableGameDataSource<DbConnection> dataSource;

        /// <summary>
        ///     Create a <see cref="GetCustomDataPropertyForKeyQueryHandler"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters is <see langword="null"/>.
        /// </exception>
        public GetCustomDataPropertyForKeyQueryHandler(
            IConnectableGameDataSource<DbConnection> dataSource)
        {
            if(dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }

            this.dataSource = dataSource;
        }

        /// <inheritdoc />
        public string Handle(GetCustomDataPropertyForKeyQuery query)
        {
            if(query == null)
            {
                throw new ArgumentNullException("query");
            }

            using(var connection = dataSource.Connect())
            {
                return connection.GetCustomDataPropertyByKey(query.Key);
            }
        }
    }
}