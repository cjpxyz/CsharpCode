﻿using com.wd.free.action;
using com.wd.free.@event;
using com.wd.free.map.position;
using com.wd.free.unit;
using com.wd.free.util;
using Core.Free;
using System;

namespace App.Server.GameModules.GamePlay.Free.player
{
    [Serializable]
    public class PlayerMoveAction : AbstractPlayerAction, IRule
    {
        private IPosSelector pos;
        private String pitch;

        public override void DoAction(IEventArgs args)
        {
            PlayerEntity p = GetPlayerEntity(args);

            UnitPosition up = pos.Select(args);
            if (up != null)
            {
                p.position.Value = new UnityEngine.Vector3(up.GetX(), up.GetY(), up.GetZ());
                p.orientation.Yaw = up.GetYaw();
                p.orientation.Pitch = FreeUtil.ReplaceFloat(pitch, args);
                
                p.latestAdjustCmd.SetPos(new UnityEngine.Vector3(up.GetX(), up.GetY(), up.GetZ()));
                p.latestAdjustCmd.ServerSeq = p.userCmdSeq.LastCmdSeq;

//                SimpleProto msg = FreePool.Allocate();
//                msg.Key = FreeMessageConstant.EntityMoveTo;
//                msg.Fs.Add(p.position.Value.x);
//                msg.Fs.Add(p.position.Value.y);
//                msg.Fs.Add(p.position.Value.z);
//                FreeMessageSender.SendMessage(p, msg);
            }
        }

        public IPosSelector getPos()
        {
            return pos;
        }

        public void setPos(IPosSelector pos)
        {
            this.pos = pos;
        }

        public int GetRuleID()
        {
            return (int)ERuleIds.PlayerMoveAction;
        }
    }
}
