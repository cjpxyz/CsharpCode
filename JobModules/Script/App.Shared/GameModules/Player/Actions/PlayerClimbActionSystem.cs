﻿using App.Shared.Components.GenericActions;
using App.Shared.Components.Player;
using Core;
using Core.GameModule.Interface;
using Core.Prediction.UserPrediction.Cmd;
using Core.Utils;
using Utils.Configuration;
using Utils.Singleton;
using XmlConfig;

namespace App.Shared.GameModules.Player.Actions
{
    public class PlayerClimbActionSystem : IUserCmdExecuteSystem
    {
        private static LoggerAdapter _logger = new LoggerAdapter(typeof(PlayerClimbActionSystem));
        private IGenericAction _genericAction;

        public void ExecuteUserCmd(IUserCmdOwner owner, IUserCmd cmd)
        {
            var player = (PlayerEntity)owner.OwnerEntity;
            
            CheckPlayerLifeState(player);
            
            if (player.gamePlay.IsLifeState(EPlayerLifeState.Dead) ||
                !player.hasGenericActionInterface ||
                player.IsOnVehicle() ||
                IsUnique(player))
                return;

            _genericAction = player.genericActionInterface.GenericAction;
            _genericAction.Update(player);

            if (cmd.IsJump && CanClimb(player))
            {
           
                TriggerActionInput(player);
                
            }
        }

        private void TriggerActionInput(PlayerEntity player)
        {
            _genericAction.ActionInput(player);
        }

        private static bool CanClimb(PlayerEntity player)
        {
            var postureState = player.stateInterface.State.GetCurrentPostureState();
            return PostureInConfig.Jump != postureState && PostureInConfig.Climb != postureState;
        }

        private static bool IsUnique(PlayerEntity player)
        {
            if (null == player || !player.hasPlayerInfo) return false;

            var roleId = player.playerInfo.RoleModelId;
            return SingletonManager.Get<RoleConfigManager>().GetRoleItemById(roleId).Unique;
        }
        
        #region LifeState

        private void CheckPlayerLifeState(PlayerEntity player)
        {
            if (null == player || null == player.playerGameState) return;
            var gameState = player.playerGameState;
            switch (gameState.CurrentPlayerLifeState)
            {
                case PlayerLifeStateEnum.Reborn:
                    Reborn(player);
                    break;
                case PlayerLifeStateEnum.Dead:
                    Dead(player);
                    break;
            }
        }

        private void Reborn(PlayerEntity player)
        {
            if (null == player) return;
            var genericAction = player.genericActionInterface.GenericAction;
            if (null == genericAction) return;
            genericAction.PlayerReborn(player);
        }
        
        private void Dead(PlayerEntity player)
        {
            if (null == player) return;
            var genericAction = player.genericActionInterface.GenericAction;
            if (null == genericAction) return;
            genericAction.PlayerDead(player);
            _logger.InfoFormat("PlayerClimbDead");
        }

        #endregion
    }
}
