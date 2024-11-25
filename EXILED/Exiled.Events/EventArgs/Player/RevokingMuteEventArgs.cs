// -----------------------------------------------------------------------
// <copyright file="RevokingMuteEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using API.Features;

    /// <summary>
    /// Contains all information before unmuting a player.
    /// </summary>
    public class RevokingMuteEventArgs : IssuingMuteEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RevokingMuteEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        ///    The player who's being unmuted.
        /// </param>
        /// <param name="isIntercom">
        ///    Indicates whether the player is being intercom unmuted.
        /// </param>
        /// <param name="isAllowed">
        ///    Indicates whether the player can be unmuted.
        /// </param>
        public RevokingMuteEventArgs(Player player, bool isIntercom, bool isAllowed = true)
            : base(player, isIntercom, isAllowed)
        {
        }
    }
}