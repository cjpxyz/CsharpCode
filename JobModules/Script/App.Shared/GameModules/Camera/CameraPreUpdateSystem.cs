﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App.Shared;
using App.Shared.Components.Player;
using App.Shared.GameModules.Camera;
using App.Shared.GameModules.Camera.Utils;
using Core.CameraControl.NewMotor;
using Core.GameModule.Interface;
using Core.Prediction.UserPrediction.Cmd;
using Core.Utils;
using UnityEngine;
using XmlConfig;

namespace Assets.App.Shared.GameModules.Camera
{
    class CameraPreUpdateSystem: IUserCmdExecuteSystem,IBeforeUserCmdExecuteSystem,ISimpleParallelUserCmdExecuteSystem
    {
        private Motors _motors = new Motors();
       
        private DummyCameraMotorState _state ;
        private VehicleContext _vehicleContext;

        private FreeMoveContext _freeMoveContext;
        
        private static readonly LoggerAdapter Logger = new LoggerAdapter(typeof(CameraPreUpdateSystem));
         
        public CameraPreUpdateSystem(VehicleContext vehicleContext, FreeMoveContext freeMoveContext, Motors m)
        {
            _vehicleContext = vehicleContext;
            _freeMoveContext = freeMoveContext;
            _motors = m;
            _state = new DummyCameraMotorState(m);
        }

        private void PreProcessInput(PlayerEntity player, DummyCameraMotorInput input,
            Dictionary<int, ICameraNewMotor> dict, SubCameraMotorState subState, DummyCameraMotorState state)
        {
            if (!dict.ContainsKey(subState.NowMode)) return;
            if (!dict.ContainsKey(subState.LastMode)) return;
            var oldMotor = dict[subState.LastMode];
            var nowMotor = dict[subState.NowMode];
            nowMotor.PreProcessInput(player, input, state);
        }

        public void ExecuteUserCmd(IUserCmdOwner owner, IUserCmd cmd)
        {
            
            //Logger.InfoFormat("seq:{0}, delata yaw:{2},  client return judge:{1}", cmd.Seq, (!cmd.NeedStepPredication && !SharedConfig.IsServer), cmd.DeltaYaw);
            if (!cmd.NeedStepPredication && !SharedConfig.IsServer) return;
            
            UpdateCamera(owner, cmd);
        }

        public ISimpleParallelUserCmdExecuteSystem CreateCopy()
        {
            return new CameraPreUpdateSystem(_vehicleContext,_freeMoveContext,_motors);
        }

        private void UpdateCamera(IUserCmdOwner owner, IUserCmd cmd)
        {
            PlayerEntity player = owner.OwnerEntity as PlayerEntity;
            ;
            if (!player.hasCameraStateNew) return;
            if (!player.hasCameraStateOutputNew) return;
//            var finalOutput = player.cameraStateOutputNew;
            DummyCameraMotorState.Convert(player.cameraStateNew, _state);


            var archotRotation = player.cameraArchor.ArchorEulerAngle;
            if (player.cameraStateNew.CameraMotorInput == null)
                player.cameraStateNew.CameraMotorInput = new DummyCameraMotorInput();
            DummyCameraMotorInput _input = (DummyCameraMotorInput) player.cameraStateNew.CameraMotorInput;
            _input.Generate(player, cmd, archotRotation.y, archotRotation.x);
            for (int i=0;i<(int)SubCameraMotorType.End;i++)
            {
                var type = (SubCameraMotorType)i;
                PreProcessInput(player, _input, _motors.GetDict(type), _state.Get(type), _state);
            }
            
            DummyCameraMotorState.Convert(_state, player.cameraStateNew);
        }

        public void BeforeExecuteUserCmd(IUserCmdOwner owner, IUserCmd cmd)
        {
            UpdateCamera(owner, cmd);
        }
    }
}
