﻿using System;
using System.Collections.Generic;
using Assets.XmlConfig;
using Core;
using Core.EntityComponent;
using Entitas;
using UnityEngine;

namespace Core
{
    public interface IMapObjectEntityFactory
    {
        IEntity CreateDoor(int objectId, GameObject gameObject);

        IEntity CreateDestructibleObject(int objectId, GameObject gameObject);

        IEntity CreateGlassyObject(int objectId, GameObject gameObject);
    }
    
    public interface ISceneObjectEntityFactory
    {
        List<int> FreeCastEntityToDestoryList { get; }

        IEntity CreateSimpleEquipmentEntity(
            ECategory category,
            int id,
            int count,
            Vector3 position);

        IEntity CreateSceneWeaponObjectEntity(WeaponScanStruct weaponInfo, Vector3 position);

        IEntity CreateCastEntity(Vector3 position, float size, int key, string tip);

        void DestroySceneWeaponObjectEntity(EntityKey entityKey);

        IEntity CreateDropSceneWeaponObjectEntity(WeaponScanStruct weaponInfo, Vector3 position, int lifeTime);

        IEntity GetSceneEntity(int value);
        IEntity CreateSceneAudioEmitterEntity(Vector3 positionValue, EntityKey entityKey);
        IEntity CreateSceneAudioBgEmitterEntity(Vector3 positionValue, EntityKey entityKey);

    }
}