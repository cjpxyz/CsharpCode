﻿using App.Shared.GameModules.Camera.Utils;
using Core.GameModule.Interface;
using Core.Prediction.UserPrediction.Cmd;
using Core.Utils;

namespace Assets.App.Shared.GameModules.Player
{
    public class PlayerHoldBreathSystem : IUserCmdExecuteSystem
    {
        private static readonly LoggerAdapter Logger = new LoggerAdapter(typeof(PlayerHoldBreathSystem));
        public void ExecuteUserCmd(IUserCmdOwner owner, IUserCmd cmd)
        {
            var player = owner.OwnerEntity as PlayerEntity;
            if(null == player)
            {
                return;
            }
            if(cmd.IsHoldBreath && player.IsAiming()) 
            {
                if(null == player)
                {
                    Logger.Error("owner entity is not player entity ");
                    return;
                }
                if(!player.hasAppearanceInterface)
                {
                    Logger.Error("player has no appearance interface ");
                    return;
                }

                if (!player.oxygenEnergyInterface.Oxygen.InShiftState)
                {
                    player.oxygenEnergyInterface.Oxygen.ShiftVeryTime = player.time.ClientTime;
                }
                player.oxygenEnergyInterface.Oxygen.InShiftState = true;
            }
            else
            {
                if(!player.hasAppearanceInterface)
                {
                    Logger.Error("player has no appearance interface ");
                    return;
                }

                if (player.oxygenEnergyInterface.Oxygen.InShiftState)
                {
                    player.oxygenEnergyInterface.Oxygen.ShiftVeryTime = player.time.ClientTime;
                }
                player.oxygenEnergyInterface.Oxygen.InShiftState = false;
                
            }

            if (player.oxygenEnergyInterface.Oxygen.InShiftState)
            {
                player.appearanceInterface.FirstPersonAppearance.SightShift.SetHoldBreath(true);
            }
            else
            {
                player.appearanceInterface.FirstPersonAppearance.SightShift.SetHoldBreath(false);
            }
        }
    }
}
