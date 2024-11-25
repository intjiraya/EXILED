// -----------------------------------------------------------------------
// <copyright file="RespawningTeamEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Server
{
    using System.Collections.Generic;

    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Interfaces;

    using PlayerRoles;

    using Respawning;

    /// <summary>
    /// Contains all information before spawning a wave of <see cref="SpawnableTeamType.NineTailedFox" /> or
    /// <see cref="SpawnableTeamType.ChaosInsurgency" />.
    /// </summary>
    public class RespawningTeamEventArgs : IDeniableEvent
    {
        private SpawnableTeamType nextKnownTeam;
        private int maximumRespawnAmount;

        /// <summary>
        /// Initializes a new instance of the <see cref="RespawningTeamEventArgs" /> class.
        /// </summary>
        /// <param name="players">
        /// <inheritdoc cref="Players" />
        /// </param>
        /// <param name="maxRespawn">
        /// <inheritdoc cref="MaximumRespawnAmount" />
        /// </param>
        /// <param name="nextKnownTeam">
        /// <inheritdoc cref="NextKnownTeam" />
        /// </param>
        /// <param name="isAllowed">
        /// <inheritdoc cref="IsAllowed" />
        /// </param>
        public RespawningTeamEventArgs(List<Player> players, int maxRespawn, SpawnableTeamType nextKnownTeam, bool isAllowed = true)
        {
            Players = players;
            MaximumRespawnAmount = maxRespawn;

            this.nextKnownTeam = nextKnownTeam;
            SpawnQueue = new();
            SpawnableTeam.GenerateQueue(SpawnQueue, players.Count);
            IsAllowed = isAllowed;
        }

        /// <summary>
        /// Gets the list of players that are going to be respawned.
        /// </summary>
        public List<Player> Players { get; }

        /// <summary>
        /// Gets or sets the maximum amount of respawnable players.
        /// </summary>
        public int MaximumRespawnAmount
        {
            get => maximumRespawnAmount;
            set
            {
                if (value < maximumRespawnAmount)
                {
                    if (Players.Count > value)
                        Players.RemoveRange(value, Players.Count - value);
                }

                maximumRespawnAmount = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what the next respawnable team is.
        /// </summary>
        public SpawnableTeamType NextKnownTeam
        {
            get => nextKnownTeam;
            set
            {
                nextKnownTeam = value;

                if (!RespawnManager.SpawnableTeams.TryGetValue(value, out SpawnableTeamHandlerBase spawnableTeam))
                {
                    MaximumRespawnAmount = 0;
                    return;
                }

                MaximumRespawnAmount = spawnableTeam.MaxWaveSize;
                if (RespawnManager.SpawnableTeams.TryGetValue(nextKnownTeam, out SpawnableTeamHandlerBase @base))
                    @base.GenerateQueue(SpawnQueue, Players.Count);
            }
        }

        /// <summary>
        /// Gets the current spawnable team.
        /// </summary>
        public SpawnableTeamHandlerBase SpawnableTeam
            => RespawnManager.SpawnableTeams.TryGetValue(NextKnownTeam, out SpawnableTeamHandlerBase @base) ? @base : null;

        /// <summary>
        /// Gets or sets a value indicating whether the spawn can occur.
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// Gets or sets the RoleTypeId spawn queue.
        /// </summary>
        public Queue<RoleTypeId> SpawnQueue { get; set; }
    }
}
