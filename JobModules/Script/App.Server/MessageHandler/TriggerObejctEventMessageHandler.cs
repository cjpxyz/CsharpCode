﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App.Server.GameModules.SceneObject;
using App.Shared;
using Core.EntityComponent;
using Core.Network;
using App.Shared.SceneTriggerObject;
using App.Shared.Util;
using Core;
using Core.SceneTriggerObject;
using Core.Utils;
using Utils.Singleton;

namespace App.Server.MessageHandler
{
    internal class TriggerObejctEventMessageHandler : AbstractServerMessageHandler<PlayerEntity, TriggerObjectSyncEvent>
    {
        private static LoggerAdapter _logger = new LoggerAdapter(typeof(TriggerObejctEventMessageHandler));

        private Contexts _contexts;
        public TriggerObejctEventMessageHandler(Contexts contexts, IPlayerEntityDic<PlayerEntity> converter) : base(converter)
        {
            _contexts = contexts;
        }

        public override void DoHandle(INetworkChannel channel, PlayerEntity entity, EClient2ServerMessage eClient2ServerMessage, TriggerObjectSyncEvent messageBody)
        {
            var sourceKey = new EntityKey(messageBody.SourceObjectId, (short)EEntityType.MapObject);
            var mapObject = _contexts.mapObject.GetEntityWithEntityKey(sourceKey);
            if (mapObject != null)
            {
                mapObject.triggerObjectEvent.SyncEvents.Enqueue(messageBody);
                messageBody.AcquireReference();
                if (!mapObject.isTriggerObjectEventFlag)
                {
                    //_logger.ErrorFormat("Received Trigger Object Event {0}", messageBody.EType);
                    mapObject.isTriggerObjectEventFlag = true;
                }
            }
            else
            {
                _logger.InfoFormat("Can not found SceneObject {0} for trigger object sync event {1}", sourceKey.EntityId, messageBody.EType);
            }
        }
    }
}