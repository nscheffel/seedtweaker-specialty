// -----------------------------------------------------------------------
// <copyright file = "GetAllCustomDataPropertiesQueryHandler.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.GameDataSource.QueryHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using Link.Math.GameDataSource;
    using Link.Math.GameDataSource.Queries;
    using Link.Math.Sqlite;
    using Query.Handlers;

    /// <summary>
    ///     Query handler for <see cref="GetAllCustomDataPropertiesQuery"/>.
    /// </summary>
    internal sealed class GetAllCustomDataPropertiesQueryHandler :
        IQueryHandler<GetAllCustomDataPropertiesQuery, IDictionary<string, string>>
    {
        /// <summary>
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </summary>
        private readonly IConnectableGameDataSource<DbConnection> dataSource;

        /// <summary>
        ///     Create a <see cref="GetAllCustomDataPropertiesQueryHandler"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The <see cref="IConnectableGameDataSource{T}"/> to use to
        ///     establish a connection with the SQLite file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one of the parameters is <see langword="null"/>.
        /// </exception>
        public GetAllCustomDataPropertiesQueryHandler(
            IConnectableGameDataSource<DbConnection> dataSource)
        {
            if(dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }

            this.dataSource = dataSource;
        }

        /// <inheritdoc />
        public IDictionary<string, string> Handle(GetAllCustomDataPropertiesQuery query)
        {
            if(query == null)
            {
                throw new ArgumentNullException("query");
            }

            using(var connection = dataSource.Connect())
            {
                return connection.GetAllCustomDataProperties();
            }
        }
    }
}