﻿using App.Shared.Audio;
using App.Shared.Components.Player;

using App.Shared.GameModules.Weapon;
using App.Shared.Player;
using App.Shared.Util;
using Core;

using Core.EntityComponent;

using System;
using System.Collections.Generic;
using App.Shared.GameModules.Player;
using Utils.Configuration;
using Utils.Singleton;
using WeaponConfigNs;
using XmlConfig;

namespace App.Shared.GameModules.Weapon
{

    public partial class PlayerWeaponController : ModuleLogicActivator<PlayerWeaponController>, IPlayerWeaponSharedGetter

    {
        public GameModeControllerBase ModeController
        {
            get { return GameModuleManagement.Get<GameModeControllerBase>(Owner.EntityId); }
        }


        public PlayerAudioController AudioController
        {
            get { return GameModuleManagement.Get<PlayerAudioController>(Owner.EntityId).Value; }
        }
        public PlayerStateInteractController InteractController
        {
            get { return GameModuleManagement.Get<PlayerStateInteractController>(Owner.EntityId); }
        }

        //  private readonly WeaponSlotsAux slotsAux;
      

        private readonly WeaponProcessHelper processHelper;

        private GrenadeCacheHandler _grenadeHandler;

        private WeaponBaseAgent[] slotWeaponAgents;



        public IGrenadeCacheHandler GrenadeHandler
        {
            get { return _grenadeHandler; }
        }

        //     private readonly Dictionary<EWeaponSlotType, System.Type> weaponAgentAssTypeDict = new Dictionary<EWeaponSlotType, Type>();

        /// <summary>
        /// 槽位武器监听事件
        /// </summary>
        private WeaponBaseAgent CreateWeaponAgent(EWeaponSlotType slotType, System.Type t)
        {
            if (slotWeaponAgents[(int)slotType] == null)
            {
                
                var func1 = GenerateBagWeaponKeyExtractor(slotType);
                var func2 = GenerateBagEmptyKeyExtractor();
                var newAgent = (WeaponBaseAgent)Activator.CreateInstance(t,
                    func1, func2, slotType, _grenadeHandler);
                newAgent.Owner = Owner;
                slotWeaponAgents[(int)slotType] = newAgent;
            }

            return slotWeaponAgents[(int)slotType];
        }

        //public override string ToString()
        //{
        //    //string s = "";
        //    //foreach(WeaponBagContainer val in playerWeaponAgent.BagSetCache.WeaponBags)
        //    //{
        //    //    s += val.ToString();
        //    //}
        //    ////return s;
        //}
        public PlayerWeaponController()
        {
            processHelper = new WeaponProcessHelper(this);
            slotWeaponAgents = new WeaponBaseAgent[GlobalConst.WeaponSlotMaxLength];
            //     interruptUpdateHandler = new InterruptUpdateHandler(this);
        }

        private void OnDerivedTypeInstanceProcess(Type t)
        {
            var attributes = Attribute.GetCustomAttributes(t, false);
            WeaponSpeciesAttribute speciesAttr;
            foreach (Attribute attr in attributes)
            {
                speciesAttr = attr as WeaponSpeciesAttribute;
                CreateWeaponAgent(speciesAttr.slotType, t);
                //  weaponAgentAssTypeDict.Add(speciesAttr.slotType, t);
            }
        }

        public void Initialize(PlayerEntity entity, GrenadeCacheHandler handler)
        {
            SetEnity(entity);
            _grenadeHandler = handler;
            CommonUtil.ProcessDerivedTypes(typeof(WeaponBaseAgent), true, (Type t) => OnDerivedTypeInstanceProcess(t));

            //  int modeBagLength = ModeController.GetUsableWeapnBagLength(RelatedPlayerInfo);
            ////多个背包共享一份投掷武器代理
            //if (modeBagLength > 1)
            //{
            //    for (int i = 1; i < modeBagLength; i++)
            //        slotWeaponAgents[i, (int)EWeaponSlotType.ThrowingWeapon] = throwWeaponAgent;
            //}
        }

        public void ResetAllComponents()
        {
            if (RelatedOrientation != null)
                RelatedOrientation.Reset();
        }

        public void ResetBagLockState()
        {
            BagLockState = false;
            BagOpenLimitTIme = RelatedTime +
                               SingletonManager.Get<GameModeConfigManager>().GetBagLimitTime(ModeController.ModeId);
        }


        public EntityKey Owner { get; private set; }

        public EntityKey EmptyWeaponKey
        {
            get { return RelatedCustomize.EmptyConstWeaponkey; }
        }

        public WeaponBaseAgent HeldWeaponAgent
        {
            get { return this[HeldSlotType]; }
        }

        public WeaponBaseAgent this[EWeaponSlotType slot]
        {
            get { return slotWeaponAgents[(int)slot]; }
        }

        public int HeldConfigId
        {
            get { return HeldWeaponAgent.ConfigId; }
        }

        public WeaponBaseAgent GetWeaponAgent(EWeaponSlotType slotType = EWeaponSlotType.Pointer)
        {
            if (slotType == EWeaponSlotType.Pointer) slotType = HeldSlotType;
            else if (slotType == EWeaponSlotType.LastPointer) slotType = LastSlotType;
            return this[slotType];
        }

        public WeaponBaseAgent GetWeaponAgent(int configId)
        {
            EWeaponSlotType slotType = processHelper.GetMatchedSlotType(configId);
            return this[slotType];
        }


        public EWeaponSlotType HeldSlotType
        {
            get { return (EWeaponSlotType)RelatedBagSet.HeldSlotIndex; }
        }

        public bool IsHeldSlotEmpty
        {
            get { return !HeldWeaponAgent.IsValid(); }
        }

        public EWeaponSlotType LastSlotType
        {
            get { return (EWeaponSlotType)RelatedBagSet.LastSlotIndex; }
        }


        public int HeldBagPointer
        {
            get { return RelatedBagSet.HeldBagPointer; }
            set { RelatedBagSet.HeldBagPointer = value; }
        }


        public bool IsWeaponSlotEmpty(EWeaponSlotType slot)
        {
            return !GetWeaponAgent(slot).IsValid();
        }

        public bool IsHeldBagSlotType(EWeaponSlotType slot)
        {
            return slot == HeldSlotType;
        }

        public EWeaponSlotType PollGetLastSlotType(bool excludeLast = true)
        {
            if (!excludeLast)
            {
                EWeaponSlotType last = LastSlotType;
                if (last != EWeaponSlotType.None && !IsWeaponSlotEmpty(last))
                {
                    return last;
                }
            }

            for (EWeaponSlotType s = EWeaponSlotType.None + 1; s < EWeaponSlotType.Length; s++)
            {
                if (!IsWeaponSlotEmpty(s))
                    return s;
            }

            return EWeaponSlotType.None;
        }


        public int BagOpenLimitTIme
        {
            get { return RelatedCustomize.BagOpenLimitTime; }
            set { RelatedCustomize.BagOpenLimitTime = value; }
        }

        ///overridebag components
        public int OverrideBagTactic
        {
            get { return RelatedCustomize.TacticWeapon; }
            set { RelatedCustomize.TacticWeapon = value; }
        }

        public bool BagLockState
        {
            get { return RelatedCustomize.BagLockState; }
            set { RelatedCustomize.BagLockState = value; }
        }

        public bool CanSwitchWeaponBag
        {
            get { return ModeController.CanModeSwitchBag && !BagLockState && (BagOpenLimitTIme > RelatedTime); }
        }

        //        public void PlayFireAudio()
        //        {
        //            if (!IsHeldSlotEmpty)
        //                GameAudioMedia.PlayWeaponAudio(HeldConfigId, RelatedAppearence.WeaponHandObject(), (config) => config.Fire);
        //        }
        //        public void PlayPullBoltAudio()
        //        {
        //            if (!IsHeldSlotEmpty)
        //                GameAudioMedia.PlayWeaponAudio(HeldConfigId, RelatedAppearence.WeaponHandObject(), (config) => config.PullBolt);
        //        }
        //        public void PlayReloadAudio()
        //        {
        //            if (!IsHeldSlotEmpty)
        //                GameAudioMedia.PlayWeaponAudio(HeldConfigId, RelatedAppearence.WeaponHandObject(), (config) => config.ReloadStart);
        //        }


        public List<PlayerBulletData> BulletList
        {
            get { return RelatedWeaponAux.BulletList; }
        }

        public List<EClientEffectType> EffectList
        {
            get { return RelatedWeaponAux.EffectList; }
        }





        public int GetReservedBullet(EBulletCaliber caliber)
        {
            return ModeController.GetReservedBullet(this, caliber);
        }

        public int GetReservedBullet()
        {
            return ModeController.GetReservedBullet(this, HeldSlotType);
        }

        public int GetReservedBullet(EWeaponSlotType slot)
        {
            if (slot.IsSlotWithBullet())
                return ModeController.GetReservedBullet(this, slot);
            return 0;
        }

        public bool IsWeaponInSlot(int configId)
        {
            EWeaponSlotType slotType = WeaponUtil.GetEWeaponSlotTypeById(configId);
            if (slotType == EWeaponSlotType.None) return false;
            return this[slotType].ConfigId == configId;
        }
    }

  
}