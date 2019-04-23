﻿using App.Shared;
using App.Shared.GameModules.Camera;
using App.Shared.GameModules.Camera.Utils;
using Core.CameraControl.NewMotor;
using Core.GameModule.Interface;
using Core.Prediction.UserPrediction.Cmd;
using Core.Utils;
using System.Collections.Generic;
using Core.EntityComponent;
using XmlConfig;

namespace Assets.App.Shared.GameModules.Camera
{
    class CameraPreUpdateSystem: AbstractCameraUpdateSystem,IUserCmdExecuteSystem,IBeforeUserCmdExecuteSystem,ISimpleParallelUserCmdExecuteSystem
    {
        private Motors _motors = new Motors();
       
        private DummyCameraMotorState _state ;
        
        private static readonly LoggerAdapter Logger = new LoggerAdapter(typeof(CameraPreUpdateSystem));
         
        public CameraPreUpdateSystem(Contexts contexts, Motors m):base(contexts)
        {
            _motors = m;
            _state = new DummyCameraMotorState(m);
        }

        public void ExecuteUserCmd(IUserCmdOwner owner, IUserCmd cmd)
        {
            //Logger.InfoFormat("seq:{0}, delata yaw:{2},  client return judge:{1}", cmd.Seq, (!cmd.NeedStepPredication && !SharedConfig.IsServer), cmd.DeltaYaw);
            if (!cmd.NeedStepPredication && !SharedConfig.IsServer) 
                return;
            var player = owner.OwnerEntity as PlayerEntity;
            if (player == null) return;
            
            CommonUpdate(player,cmd);
        }

        public ISimpleParallelUserCmdExecuteSystem CreateCopy()
        {
            return new CameraPreUpdateSystem(_contexts, _motors);
        }

        public void BeforeExecuteUserCmd(IUserCmdOwner owner, IUserCmd cmd)
        {
            var player = owner.OwnerEntity as PlayerEntity;
            if (player == null) return;
            
            CommonUpdate(player,cmd);
        }
        
        protected override void ExecWhenObserving(PlayerEntity player, IUserCmd cmd)
        {
            var observedFreeMove =
                _contexts.freeMove.GetEntityWithEntityKey(new EntityKey(player.gamePlay.CameraEntityId,
                    (short) EEntityType.FreeMove));
            if (observedFreeMove == null) return;
            Handle(player, cmd);
            player.cameraStateOutputNew.ArchorEulerAngle = player.orientation.EulerAngle;
        }

        protected override void ExecWhenBeingObserved(PlayerEntity player, IUserCmd cmd)
        {
        }

        protected override void ExecWhenNormal(PlayerEntity player, IUserCmd cmd)
        {
            Handle(player, cmd);
        }

        private void Handle(PlayerEntity player, IUserCmd cmd)
        {
            if (!player.hasCameraStateNew) return;
            if (!player.hasCameraStateOutputNew) return;
            
            DummyCameraMotorState.Convert(player.cameraStateNew, _state);

            var archotRotation = player.cameraArchor.ArchorEulerAngle;
            if (player.cameraStateNew.CameraMotorInput == null)
                player.cameraStateNew.CameraMotorInput = new DummyCameraMotorInput();
            DummyCameraMotorInput _input = (DummyCameraMotorInput) player.cameraStateNew.CameraMotorInput;
            _input.Generate(_contexts, player, cmd, archotRotation.y, archotRotation.x, _state);

            for (int i=0;i<(int)SubCameraMotorType.End;i++)
            {
                var type = (SubCameraMotorType)i;
                PreProcessInput(player, _input, _motors.GetDict(type), _state.Get(type), _state);
            }
            DummyCameraMotorState.Convert(_state, player.cameraStateNew);
        }
        
        private void PreProcessInput(PlayerEntity player, DummyCameraMotorInput input,
            Dictionary<int, ICameraNewMotor> dict, SubCameraMotorState subState, DummyCameraMotorState state)
        {
            if (!dict.ContainsKey(subState.NowMode)) return;
            if (!dict.ContainsKey(subState.LastMode)) return;
            var nowMotor = dict[subState.NowMode];
            nowMotor.PreProcessInput(player, input, state);
        }
    }
}
