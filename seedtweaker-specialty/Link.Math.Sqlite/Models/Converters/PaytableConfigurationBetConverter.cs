// -----------------------------------------------------------------------
// <copyright file = "PaytableConfigurationBetConverter.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models.Converters
{
    using System;
    using System.Collections.Generic;
    using Evaluation.Data;

    /// <summary>
    ///     Converts between <see cref="PaytableConfiguration"/>/
    ///     <see cref="Bet"/> pairing and a <see cref="GameConfiguration"/>.
    /// </summary>
    /// <remarks>
    ///     This is used in addition to
    ///     <see cref="PaytableConfigurationBetConverter"/> as the addition of
    ///     <see cref="Bet"/> information can better fill out a
    ///     <see cref="GameConfiguration"/>. It has no effect on converting
    ///     from a <see cref="GameConfiguration"/>.
    ///     A <see cref="KeyValuePair{TKey,TValue}"/> is used as there is no
    ///     tuple support in .NET 3.5.
    /// </remarks>
    public class PaytableConfigurationBetConverter :
        IConverter<KeyValuePair<PaytableConfiguration, Bet>, GameConfiguration>
    {
        #region Fields

        /// <summary>
        ///     <see cref="IConverter{TFirst,TSecond}"/> for converting between
        ///     a <see cref="PaytableConfiguration"/> and
        ///     <see cref="GameConfiguration"/>.
        /// </summary>
        private readonly IConverter<PaytableConfiguration, GameConfiguration>
            paytableConfigConverter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initialize <see cref="PaytableConfigurationBetConverter"/>.
        /// </summary>
        /// <param name="paytableConfigConverter">
        ///     <see cref="IConverter{TFirst,TSecond}"/> for converting between
        ///     a <see cref="PaytableConfiguration"/> and
        ///     <see cref="GameConfiguration"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="paytableConfigConverter"/> is
        ///     <see langword="null"/>.
        /// </exception>
        public PaytableConfigurationBetConverter(
            IConverter<PaytableConfiguration, GameConfiguration>
                paytableConfigConverter)
        {
            if(paytableConfigConverter == null)
            {
                throw new ArgumentNullException("paytableConfigConverter");
            }

            this.paytableConfigConverter = paytableConfigConverter;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public KeyValuePair<PaytableConfiguration, Bet> Convert(
            GameConfiguration obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException("gameConfig");
            }

            // There's not enough information to construct a
            // valid bet, so not going to bother.
            return new KeyValuePair<PaytableConfiguration, Bet>(
                paytableConfigConverter.Convert(obj),
                null);
        }

        /// <inheritdoc/>
        public GameConfiguration Convert(
            KeyValuePair<PaytableConfiguration, Bet> obj)
        {
            if(obj.Key == null)
            {
                throw new ArgumentNullException("obj.Key");
            }

            if(obj.Value == null)
            {
                throw new ArgumentNullException("obj.Value");
            }

            var config = paytableConfigConverter.Convert(obj.Key);
            config.PersistenceId =
                System.Convert.ToInt32(obj.Value.PersistenceIdentifier);
            config.TotalBet = (int)obj.Value.TotalBet;

            return config;
        }

        #endregion
    }
}
