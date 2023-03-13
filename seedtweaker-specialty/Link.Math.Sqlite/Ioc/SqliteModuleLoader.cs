// -----------------------------------------------------------------------
// <copyright file = "SqliteModuleLoader.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Ioc
{
    using System.Collections.Generic;
    using Evaluation.Data;
    using Link.Ioc;
    using Models;
    using Models.Converters;

    /// <summary>
    ///     Common component module loader.
    /// </summary>
    public class SqliteModuleLoader : IModuleLoader
    {
        #region Methods

        /// <inheritdoc/>
        public void Register(
            IContainer container)
        {
            container.Register<CustomBetDataEncoding>();
            container.Register<ICustomBetDataEncoding, CustomBetDataEncoding>();
            container.Register<IConverter<Bet, BetConfiguration>, BetConverter>();
            container.Register<IConverter<PaytableConfiguration, GameConfiguration>, PaytableConfigurationConverter>();
            container.Register<IConverter<KeyValuePair<PaytableConfiguration, Bet>, GameConfiguration>, PaytableConfigurationBetConverter>();
            container.Register<IConverter<IEnumerable<ProgressiveWinGroup>, string>, ProgressiveWinGroupConverter>();
        }

        #endregion
    }
}
