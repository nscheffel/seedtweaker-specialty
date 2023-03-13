// -----------------------------------------------------------------------
// <copyright file = "PaytableConfigurationConverter.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models.Converters
{
    using System;
    using Evaluation.Data;

    /// <summary>
    ///     Converts between <see cref="PaytableConfiguration"/> and
    ///     <see cref="GameConfiguration"/>.
    /// </summary>
    public class PaytableConfigurationConverter :
        IConverter<PaytableConfiguration, GameConfiguration>
    {
        #region Methods

        /// <summary>
        ///     Convert from a <see cref="GameConfiguration"/> to a
        ///     <see cref="PaytableConfiguration"/>.
        /// </summary>
        /// <param name="gameConfig">
        ///     The <see cref="GameConfiguration"/> to convert.
        /// </param>
        /// <returns>
        ///     Converted <see cref="GameConfiguration"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="gameConfig"/> is <see langword="null"/>.
        /// </exception>
        public PaytableConfiguration Convert(
            GameConfiguration gameConfig)
        {
            if(gameConfig == null)
            {
                throw new ArgumentNullException("gameConfig");
            }

            // NOTE: Denomination is not stored in a SqLite data store (yet).
            // NOTE: So denomination is hardcoded to 1.
            return new PaytableConfiguration(
                gameConfig.PaytableIndex,
                1);
        }

        /// <summary>
        ///     Convert from a <see cref="PaytableConfiguration"/> to a
        ///     <see cref="GameConfiguration"/>.
        /// </summary>
        /// <param name="paytableConfig">
        ///     The <see cref="PaytableConfiguration"/> to convert.
        /// </param>
        /// <returns>
        ///     Converted <see cref="PaytableConfiguration"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="paytableConfig"/> is <see langword="null"/>.
        /// </exception>
        public GameConfiguration Convert(
            PaytableConfiguration paytableConfig)
        {
            if(paytableConfig == null)
            {
                throw new ArgumentNullException("paytableConfig");
            }

            return new GameConfiguration
            {
                PaytableIndex = paytableConfig.PaytableId
            };
        }

        #endregion
    }
}
