// -----------------------------------------------------------------------
// <copyright file = "BetConverter.cs" company = "IGT">
//     Copyright (c) 2021 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Link.Math.Sqlite.Models.Converters
{
    using System;
    using System.Linq;

    /// <summary>
    ///     Converts between <see cref="Bet"/> and
    ///     <see cref="BetConfiguration"/>.
    /// </summary>
    public class BetConverter :
        IConverter<Bet, BetConfiguration>
    {
        #region Fields

        /// <summary>
        ///     <see cref="ICustomBetDataEncoding"/> for converting custom bet
        ///     information.
        /// </summary>
        private readonly ICustomBetDataEncoding customBetEncoder;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initialize <see cref="BetConverter"/>.
        /// </summary>
        /// <param name="customBetEncoder">
        ///     <see cref="ICustomBetDataEncoding"/> for converting custom bet
        ///     information.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="customBetEncoder"/> is <see langword="null"/>.
        /// </exception>
        public BetConverter(
            ICustomBetDataEncoding customBetEncoder)
        {
            if(customBetEncoder == null)
            {
                throw new ArgumentNullException("customBetEncoder");
            }

            this.customBetEncoder = customBetEncoder;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public BetConfiguration Convert(
            Bet bet)
        {
            return new BetConfiguration
            {
                TotalBet = (int)bet.TotalBet,
                Lines = (int)bet.SubBet,
                BetPerLine = (int)bet.BetPerSubBet,
                ExtraBet = (int)bet.ExtraBet,
                SideBet = (int)bet.SideBet,
                PersistenceId =
                    System.Convert.ToInt32(bet.PersistenceIdentifier),
                CustomBetInfo = customBetEncoder.Encode(bet.CustomBetData)
            };
        }

        /// <inheritdoc />
        public Bet Convert(
            BetConfiguration bet)
        {
            return new Bet(
                bet.Id.ToString(),
                bet.TotalBet,
                bet.Lines,
                bet.BetPerLine,
                bet.ExtraBet,
                bet.SideBet,
                bet.PersistenceId.ToString(),
                customBetEncoder.Decode(bet.CustomBetInfo)
                    .ToDictionary(
                        dict => dict.Key,
                        dict => dict.Value));
        }

        #endregion
    }
}
