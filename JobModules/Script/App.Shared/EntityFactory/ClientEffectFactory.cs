﻿using System;
using System.Collections.Generic;
using App.Shared.Components;
using App.Shared.Player.Events;
using Core.Configuration;
using Core.EntityComponent;
using Core.Event;
using Core.Utils;
using UnityEngine;
using Utils.Appearance;
using Utils.Configuration;
using Utils.Singleton;
using XmlConfig;

namespace App.Shared.EntityFactory
{
    public class ClientEffectFactory
    {
        private static readonly LoggerAdapter Logger = new LoggerAdapter(typeof(ClientEffectFactory));

        // 越后越新
        private static LinkedList<ClientEffectEntity> _bulletDropEntities = new LinkedList<ClientEffectEntity>();

        public static void OnDestroy(ClientEffectEntity entity)
        {
            if (entity.hasEffectType && entity.effectType.Value == (int) EClientEffectType.BulletDrop)
            {
                _bulletDropEntities.Remove(entity);
            }
        }

        public static ClientEffectEntity CreateBaseEntity(ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator, Vector3 pos, int type)
        {
            var effectEntity = context.CreateEntity();
            var nextId = entityIdGenerator.GetNextEntityId();
            effectEntity.AddEntityKey(new EntityKey(nextId, (int) EEntityType.ClientEffect));
            effectEntity.AddPosition();
            effectEntity.position.Value = pos;
            effectEntity.AddEffectType(type);
            effectEntity.AddAssets(false,false);
            effectEntity.AddLifeTime(DateTime.Now, 6000);
            effectEntity.isFlagSyncNonSelf = true;
            return effectEntity;
        }

        public static void CreateBulletDrop(ClientEffectContext context, IEntityIdGenerator idGenerator,
            EntityKey owner, Vector3 position, float Yaw, float pitch, int effectId,int weaponId,AudioGrp_FootMatType dropMatType)
        {
            while (_bulletDropEntities.Count >= SingletonManager.Get<ClientEffectCommonConfigManager>().GetBulletDropMaxCount(SharedConfig.IsServer))
            {
                var val = _bulletDropEntities.First.Value;
                if (val.isEnabled)
                {
                    val.isFlagDestroy = true;
                }

                _bulletDropEntities.RemoveFirst();
            }

            var type = (int) EClientEffectType.BulletDrop;
            var entity = CreateBaseEntity(context, idGenerator, position, type);
            entity.AddEffectId(effectId);
            entity.AddOwnerId(owner);
            entity.lifeTime.LifeTime = SingletonManager.Get<ClientEffectCommonConfigManager>().BulletDropLifeTime;

            entity.AddAudio((int)AudioClientEffectType.BulletDrop);
            entity.audio.AudioClientEffectArg1 = SingletonManager.Get<AudioWeaponManager>().FindById(weaponId).BulletDrop;
            entity.audio.AudioClientEffectArg2 = (int) dropMatType;
            entity.AddEffectRotation(Yaw, pitch);
            entity.AddFlagImmutability(0);
            _bulletDropEntities.AddLast(entity);
        }

       

        public static void AdHitEnvironmentEffectEvent(PlayerEntity srcPlayer, Vector3 hitPoint, Vector3 offset,  EEnvironmentType environmentType)
        {
            HitEnvironmentEvent e = (HitEnvironmentEvent)EventInfos.Instance.Allocate(EEventType.HitEnvironment, false);
            e.EnvironmentType = environmentType;
            e.Offset = offset;
          
            e.HitPoint = hitPoint;
            srcPlayer.localEvents.Events.AddEvent(e);
        }
        public static void CreateHitEnvironmentEffect(
            ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator,
            Vector3 hitPoint,
            Vector3 normal,
            EntityKey owner,
            EEnvironmentType environmentType)
        {
            Logger.InfoFormat("===>CreateBulletHitEnvironmentEffet ", environmentType);

            CreateHitEmitEffect(context, entityIdGenerator, hitPoint, normal, owner, environmentType);

            Logger.InfoFormat("EnvType {0} ", environmentType);
            
        }

        public static void CreateMuzzleSparkEffct(
            ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator,
            EntityKey owner,
            Transform parent,
            float pitch,
            float yaw,
            int effectId)
        {
            var entity = CreateBaseEntity(context, entityIdGenerator, parent.position,
                (int) EClientEffectType.MuzzleSpark);
            entity.AddEffectId(effectId);
            entity.AddOwnerId(owner);
            entity.lifeTime.LifeTime = 500;
            entity.AddAttachParent(owner, Vector3.zero);
            entity.AddEffectRotation(yaw, pitch);
            entity.isFlagSyncNonSelf = false;
        }

        private static void CreateHitEmitEffect(
            ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator,
            Vector3 hitPoint,
            Vector3 normal,
            EntityKey owner,
            EEnvironmentType environmentType)
        {
            int type = (int) EClientEffectType.End;
            AudioGrp_HitMatType audioGrpHitMatType = AudioGrp_HitMatType.Concrete;
            switch (environmentType)
            {
                case EEnvironmentType.Wood:
                    type = (int) EClientEffectType.WoodHit;
                    audioGrpHitMatType = AudioGrp_HitMatType.Wood;
                    
                    break;
                case EEnvironmentType.Steel:
                    type = (int) EClientEffectType.SteelHit;
                    audioGrpHitMatType = AudioGrp_HitMatType.Metal;
                    break;
                case EEnvironmentType.Soil:
                    type = (int) EClientEffectType.SoilHit;
                    break;
                case EEnvironmentType.Stone:
                    type = (int) EClientEffectType.StoneHit;
                    break;
                case EEnvironmentType.Glass:
                    type = (int) EClientEffectType.GlassHit;
                    break;
                case EEnvironmentType.Water:
                    type = (int) EClientEffectType.WaterHit;
                    audioGrpHitMatType = AudioGrp_HitMatType.Water;

                    break;
                default:
                    type = (int) EClientEffectType.DefaultHit;
                    break;
            }

            var effectEntity = CreateBaseEntity(context, entityIdGenerator, hitPoint, type);
            
            effectEntity.AddOwnerId(owner);
            effectEntity.AddNormal(normal);
            
            effectEntity.AddAudio((int)AudioClientEffectType.BulletHit);
            effectEntity.audio.AudioClientEffectArg1 =  (int) audioGrpHitMatType;
            effectEntity.lifeTime.LifeTime = SingletonManager.Get<ClientEffectCommonConfigManager>().DecayLifeTime;
            effectEntity.AddFlagImmutability(0);
            effectEntity.isFlagSyncNonSelf = false;
        }

        public static void AddBeenHitEvent(PlayerEntity srcPlayer, PlayerEntity target, int damageId, int triggerTime)
        {
            if (CanPlayBeenHit(target))
            {
                BeenHitEvent e = (BeenHitEvent) EventInfos.Instance.Allocate(EEventType.BeenHit, false);
                e.Target = target.entityKey.Value;
                e.UniqueId = damageId;
                e.TriggerTime = triggerTime;
                srcPlayer.localEvents.Events.AddEvent(e);
            }
           
        }

        private static bool CanPlayBeenHit(PlayerEntity srcPlayer)
        {
            if (srcPlayer.isFlagSelf)
            {
                return true;
            }
            else
            {
                return srcPlayer.hasThirdPersonAppearance &&
                       ((int) srcPlayer.thirdPersonAppearance.Posture >= (int) ThirdPersonPosture.Stand &&
                        (int) srcPlayer.thirdPersonAppearance.Posture <= (int) ThirdPersonPosture.ProneToCrouch)
                     && srcPlayer.thirdPersonAppearance.Action == ThirdPersonAction.Null;
            }
        }


        public static void AddHitPlayerEffectEvent(PlayerEntity srcPlayer, EntityKey target, Vector3 hitPoint, Vector3 offset)
        {
            HitPlayerEvent e = (HitPlayerEvent)EventInfos.Instance.Allocate(EEventType.HitPlayer, false);
            e.Target = target;
            e.Offset = offset;
          
            e.HitPoint = hitPoint;
            srcPlayer.localEvents.Events.AddEvent(e);
        }
        
        public static void CreateHitPlayerEffect(
            ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator,
            Vector3 hitPoint,
            EntityKey owner,
            EntityKey target,
            Vector3 offset)
        {
            int type = (int) EClientEffectType.HumanHitEffect;
            var effectEntity = CreateBaseEntity(context, entityIdGenerator, hitPoint, type);
            effectEntity.AddOwnerId(owner);
            effectEntity.AddAttachParent(target, offset);
            effectEntity.isFlagSyncNonSelf = false;
            Logger.DebugFormat("CreateHitPlayerEffect {0} {1}", effectEntity.entityKey.Value,effectEntity.isFlagSyncNonSelf );
        }

       
        

        public static void AddHitVehicleEffectEvent(PlayerEntity srcPlayer, EntityKey target, Vector3 hitPoint, Vector3 offset,
            Vector3 normal)
        {
            HitVehicleEvent e = (HitVehicleEvent)EventInfos.Instance.Allocate(EEventType.HitVehicle, false);
            e.Target = target;
            e.Offset = offset;
            e.Normal = normal;
            e.HitPoint = hitPoint;
            srcPlayer.localEvents.Events.AddEvent(e);
        }
        public static void CreateHitVehicleEffect(
            ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator,
            Vector3 hitPoint,
            EntityKey owner,
            EntityKey target,
            Vector3 offset,
            Vector3 normal)
        {
            int type = (int) EClientEffectType.SteelHit;
            var effectEntity = CreateBaseEntity(context, entityIdGenerator, hitPoint, type);
            effectEntity.AddOwnerId(owner);
            effectEntity.AddNormal(normal);
            effectEntity.AddAttachParent(target, offset);
            effectEntity.lifeTime.LifeTime = SingletonManager.Get<ClientEffectCommonConfigManager>().DecayLifeTime;
            effectEntity.AddFlagImmutability(0);
            effectEntity.isFlagSyncNonSelf = false;
        }

        public static void CreateHitFracturedChunkEffect(
            ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator,
            Vector3 hitPoint,
            EntityKey owner,
            EntityKey target,
            int fragmentId,
            Vector3 offset,
            Vector3 normal)
        {
            int type = (int) EClientEffectType.SteelHit;
            var effectEntity = CreateBaseEntity(context, entityIdGenerator, hitPoint, type);
            effectEntity.AddOwnerId(owner);
            effectEntity.AddNormal(normal);
            effectEntity.AddAttachParent(target, offset);
            effectEntity.attachParent.FragmentId = fragmentId;
            effectEntity.lifeTime.LifeTime = SingletonManager.Get<ClientEffectCommonConfigManager>().DecayLifeTime;
            effectEntity.AddFlagImmutability(0);
        }

        public static ClientEffectEntity CreateGrenadeExplosionEffect(ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator,
            EntityKey owner, Vector3 position, float yaw, float pitch, int effectId, int effectTime,
            EClientEffectType effectType)
        {
            var entity = CreateBaseEntity(context, entityIdGenerator, position, (int) effectType);
            entity.AddOwnerId(owner);
            entity.lifeTime.LifeTime = effectTime;
            entity.AddEffectId(effectId);
            entity.AddEffectRotation(yaw, pitch);
            entity.AddFlagImmutability(0);
            return entity;
        }

        /// <summary>
        /// create spray paint
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityIdGenerator"></param>
        /// <param name="sprayPaintPos">起始位置</param>
        /// <param name="sprayPaintForward">朝向</param>
        /// <param name="sprayPrintMask">掩码</param>
        /// <param name="sprayPrintSize">大小</param>
        /// <param name="sprayPrintType">类型</param>
        /// <param name="sprayPrintSpriteId">贴图</param>
        /// <param name="lifeTime">生命周期</param>
        public static void CreateSprayPaint(ClientEffectContext context,
            IEntityIdGenerator entityIdGenerator,
            Vector3 sprayPaintPos, 
            Vector3 sprayPaintForward,
            int sprayPrintMask,
            Vector3 sprayPrintSize,
            ESprayPrintType sprayPrintType,
            int sprayPrintSpriteId,
            int lifeTime
            )
        {
            int type = (int)EClientEffectType.SprayPrint;
            var effectEntity = context.CreateEntity();
            var nextId = entityIdGenerator.GetNextEntityId();
            effectEntity.AddEntityKey(new EntityKey(nextId, (int)EEntityType.ClientEffect));
            effectEntity.AddPosition();
            effectEntity.position.Value = sprayPaintPos;
            effectEntity.AddSprayPaint();
            effectEntity.sprayPaint.SprayPaintPos = sprayPaintPos;
            effectEntity.sprayPaint.SprayPaintForward = sprayPaintForward;
            effectEntity.sprayPaint.SprayPrintMask = sprayPrintMask;
            effectEntity.sprayPaint.SprayPrintSize = sprayPrintSize;
            effectEntity.sprayPaint.SprayPrintType = (int)sprayPrintType;
            effectEntity.sprayPaint.SprayPrintSpriteId = sprayPrintSpriteId;

            effectEntity.AddEffectType(type);
            effectEntity.AddAssets(false, false);
            effectEntity.AddLifeTime(DateTime.Now, lifeTime);
            effectEntity.isFlagSyncNonSelf = true;
            /*effectEntity.lifeTime.LifeTime = SingletonManager.Get<ClientEffectCommonConfigManager>().DecayLifeTime;*/
            effectEntity.AddFlagImmutability(0);
        }
    }
}