// -----------------------------------------------------------------------
// <copyright file="CustomWeapon.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.API.Features
{
    using System;

    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.Events.EventArgs;

    using InventorySystem.Items;
    using InventorySystem.Items.Firearms;
    using InventorySystem.Items.Pickups;

    using UnityEngine;

    using YamlDotNet.Serialization;

    using static CustomItems;

    using Firearm = Exiled.API.Features.Items.Firearm;
    using Player = Exiled.API.Features.Player;

    /// <inheritdoc />
    public abstract class CustomWeapon : CustomItem
    {
        /// <summary>
        /// Gets or sets the weapon modifiers.
        /// </summary>
        public abstract Modifiers Modifiers { get; set; }

        /// <inheritdoc/>
        public override ItemType Type
        {
            get => base.Type;
            set
            {
                if (!value.IsWeapon())
                    throw new ArgumentOutOfRangeException("Type", value, "Invalid weapon type.");

                base.Type = value;
            }
        }

        /// <summary>
        /// Gets or sets the weapon damage.
        /// </summary>
        public abstract float Damage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how big of a clip the weapon will have.
        /// </summary>
        public virtual byte ClipSize { get; set; }

        /// <inheritdoc/>
        public override void Spawn(Vector3 position, out Pickup pickup)
        {
            pickup = new Item(Type).Spawn(position);
            pickup.Weight = Weight;
            if (pickup.Base is FirearmPickup firearmPickup)
            {
                firearmPickup.Status = new FirearmStatus(ClipSize, FirearmStatusFlags.MagazineInserted, firearmPickup.NetworkStatus.Attachments);
                firearmPickup.NetworkStatus = firearmPickup.Status;
            }

            TrackedSerials.Add(pickup.Serial);
        }

        /// <inheritdoc/>
        public override void Give(Player player, bool displayMessage)
        {
            Item item = player.AddItem(Type);

            if (item is Firearm firearm)
            {
                firearm.Ammo = ClipSize;
            }

            TrackedSerials.Add(item.Serial);

            if (displayMessage)
                ShowPickedUpMessage(player);
        }

        /// <inheritdoc/>
        protected override void SubscribeEvents()
        {
            Events.Handlers.Player.ReloadingWeapon += OnInternalReloading;
            Events.Handlers.Player.Shooting += OnInternalShooting;
            Events.Handlers.Player.Shot += OnInternalShot;
            Events.Handlers.Player.Hurting += OnInternalHurting;

            base.SubscribeEvents();
        }

        /// <inheritdoc/>
        protected override void UnsubscribeEvents()
        {
            Events.Handlers.Player.ReloadingWeapon -= OnInternalReloading;
            Events.Handlers.Player.Shooting -= OnInternalShooting;
            Events.Handlers.Player.Shot -= OnInternalShot;
            Events.Handlers.Player.Hurting -= OnInternalHurting;

            base.UnsubscribeEvents();
        }

        /// <summary>
        /// Handles reloading for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ReloadingWeaponEventArgs"/>.</param>
        protected virtual void OnReloading(ReloadingWeaponEventArgs ev)
        {
        }

        /// <summary>
        /// Handles shooting for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ShootingEventArgs"/>.</param>
        protected virtual void OnShooting(ShootingEventArgs ev)
        {
        }

        /// <summary>
        /// Handles shot for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ShotEventArgs"/>.</param>
        protected virtual void OnShot(ShotEventArgs ev)
        {
        }

        /// <summary>
        /// Handles hurting for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="HurtingEventArgs"/>.</param>
        protected virtual void OnHurting(HurtingEventArgs ev)
        {
            if (ev.IsAllowed)
                ev.Amount = ev.Target.Role == RoleType.Scp106 ? Damage * 0.1f : Damage;
        }

        private void OnInternalReloading(ReloadingWeaponEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            OnReloading(ev);

            if (!ev.IsAllowed)
                return;

            ev.IsAllowed = false;

            byte remainingClip = ((Firearm)ev.Player.CurrentItem).Ammo;

            if (remainingClip >= ClipSize)
                return;

            Log.Debug($"{ev.Player.Nickname} ({ev.Player.UserId}) [{ev.Player.Role}] is reloading a {Name} ({Id}) [{Type} ({remainingClip}/{ClipSize})]!", Instance.Config.Debug);

            AmmoType ammoType = ((Firearm)ev.Player.CurrentItem).AmmoType;
            ushort amountToReload = (ushort)Math.Min(ClipSize - remainingClip, ev.Player.Ammo[(global::ItemType)ammoType.GetItemType()]);

            if (amountToReload <= 0)
                return;

            ev.Player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Invisible>().Intensity = 0;

            ev.Player.Ammo[(global::ItemType)ammoType.GetItemType()] -= amountToReload;
            ev.Player.Ammo[(global::ItemType)ammoType.GetItemType()] -= amountToReload;
            ((Firearm)ev.Player.CurrentItem).Ammo += (byte)amountToReload;

            Log.Debug($"{ev.Player.Nickname} ({ev.Player.UserId}) [{ev.Player.Role}] reloaded a {Name} ({Id}) [{Type} ({(Firearm)ev.Player.CurrentItem}/{ClipSize})]!", Instance.Config.Debug);
        }

        private void OnInternalShooting(ShootingEventArgs ev)
        {
            if (!Check(ev.Shooter.CurrentItem))
                return;

            OnShooting(ev);
        }

        private void OnInternalShot(ShotEventArgs ev)
        {
            if (!Check(ev.Shooter.CurrentItem))
                return;

            OnShot(ev);
        }

        private void OnInternalHurting(HurtingEventArgs ev)
        {
            if (!Check(ev.Attacker.CurrentItem) || ev.Attacker == ev.Target || ev.DamageType != ((Firearm)ev.Attacker.CurrentItem).DamageType)
                return;

            OnHurting(ev);
        }
    }
}
