// -----------------------------------------------------------------------
// <copyright file = "CreateGameDataSourceAtUriCommandHandler.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.GameDataSource.CommandHandlers
{
    using Command.Handlers;
    using Dapper;
    using Link.Math.GameDataSource.Commands;
    using System;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.IO;
    using System.Reflection;

    /// <summary>
    ///     Command handler for the <see cref="CreateGameDataSourceAtUriCommand"/>.
    /// </summary>
    internal sealed class CreateGameDataSourceAtUriCommandHandler :
        ICommandHandler<CreateGameDataSourceAtUriCommand>
    {
        /// <inheritdoc />
        public void Handle(CreateGameDataSourceAtUriCommand command)
        {
            if(command == null)
            {
                throw new ArgumentNullException("command");
            }
            
            using(var connection = Connect(command.Location.LocalPath))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "LinkDbSchema.sql";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    var sql = reader.ReadToEnd();
                    connection.Execute(sql, new { });
                }
            }
        }
        /// <summary>
        ///     Connect to the data source.
        /// </summary>
        /// <param name="location">
        ///     Location of the data source.
        /// </param>
        /// <returns>
        ///     The object returned upon successful connection.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the system is unable to connect to the data source.
        /// </exception>
        private DbConnection Connect(string location)
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = location,
                CacheSize = 10000,
                PageSize = 4096,
                Pooling = true,
                FailIfMissing = false,
                ReadOnly = false,
                SyncMode = SynchronizationModes.Normal,
                JournalMode = SQLiteJournalModeEnum.Wal,
            };

            return new SQLiteConnection(builder.ConnectionString);
        }
    }
}