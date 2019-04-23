﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App.Client.GameModules.Free;
using App.Client.GameModules.GamePlay.Free.Auto.Prefab;
using App.Client.GameModules.Ui.Models.Chicken;
using App.Client.GameModules.Ui.UiAdapter;
using App.Shared;
using App.Shared.Components.Ui;
using App.Shared.Components.Ui.UiAdapter;
using Assets.App.Client.GameModules.Ui;
using Assets.Sources.Free.Effect;
using Assets.Sources.Free.Render;
using Assets.Sources.Free.UI;
using Assets.UiFramework.Libs;
using Core.Components;
using Core.Enums;
using Core.GameModule.Interface;
using Core.SessionState;
using Core.Ui;
using Core.Ui.Map;
using Core.Utils;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;
using UIComponent.UI.Manager;
using UnityEngine;
using Utils.AssetManager;
using Utils.Singleton;

namespace App.Client.GameModules.Ui.System
{
    public class UiLoadSystem : IResourceLoadSystem
    {
        private static LoggerAdapter _logger = new LoggerAdapter(typeof(UiLoadSystem));
        private bool _loading;
        private Contexts _contexts;
        private ISessionState _sessionState;
        private int _uiCount;
        public UiLoadSystem(ISessionState sessionState, Contexts contexts)
        {
            _sessionState = sessionState;
            _contexts = contexts;

            _sessionState.CreateExitCondition(typeof(UiLoadSystem));
        }

        public void OnLoadResources(IUnityAssetManager assetManager)
        {
            if (!_loading)
            {
                CreateUIComponent(_contexts);
                InitLoxodon();
                InitUiGlobalData(_contexts);
                
                UiCreateFactory.Initialize(_contexts, OnModelInitialized);
                UiCreateFactory.RegisterAllUi(_contexts);
                _uiCount = UiCreateFactory.AddCreateUI(_contexts.session.clientSessionObjects.GameRule);
                UiCreateFactory.CreateUi(OnModelLoaded);

                InitPlayerData();

                CheckExit();
                _loading = true;

                SingletonManager.Get<SubProgressBlackBoard>().Add((uint)_uiCount);
            }

        }

        private void OnModelInitialized(AbstractModel model)
        {
            UiModule.AddModel(model);
        }

        private void OnModelLoaded(AbstractModel model)
        {
            SingletonManager.Get<SubProgressBlackBoard>().Step();
            _uiCount--;
            CheckExit();
        }

        private void CheckExit()
        {
            if (_uiCount == 0)
            {
                _sessionState.FullfillExitCondition(typeof(UiLoadSystem));
            }
        }

        private void InitPlayerData()
        {
            var adapters = _contexts.ui.uISession.UiAdapters;
            var playerEntity = _contexts.player.flagSelfEntity;
            if (adapters.ContainsKey(UiNameConstant.CommonHealthGroup))
            {
                var uiAdapter = adapters[UiNameConstant.CommonHealthGroup] as IPlayerStateUiAdapter;
                if (uiAdapter != null)
                {
                    uiAdapter.PlayerEntity = playerEntity;
                }
            }
            if (adapters.ContainsKey(UiNameConstant.CommonWeaponBagModel))
            {
                var uiAdapter = adapters[UiNameConstant.CommonWeaponBagModel] as IWeaponBagUiAdapter;
                if (uiAdapter != null)
                {
                    uiAdapter.Controller = playerEntity.WeaponController();
                }
            }
            if (adapters.ContainsKey(UiNameConstant.CommonWeaponBagTipModel))
            {
                var uiAdapter = adapters[UiNameConstant.CommonWeaponBagTipModel] as IWeaponBagTipUiAdapter;
                if (uiAdapter != null)
                {
                    uiAdapter.Player = playerEntity;
                }
            }

            /*喷漆*/
            var sprayLacquers = playerEntity.playerInfo.SprayLacquers;
            _contexts.ui.uI.PaintIdList.Clear();
            foreach (var id in sprayLacquers) {
                _contexts.ui.uI.PaintIdList.Add(id);
            }
        }

        private void CreateUIComponent(Contexts contexts)
        {
            contexts.ui.SetUI();
            contexts.ui.SetMap();
            contexts.ui.SetChat();
            contexts.ui.SetUISession();

            contexts.ui.SetBlast();
            contexts.ui.SetGroup();
            contexts.ui.SetChicken();

            contexts.ui.uISession.UiState = new Dictionary<string, bool>();
            contexts.ui.uISession.UiAdapters = new Dictionary<string, IUiAdapter>();
            contexts.ui.uISession.CreateUi = new List<string>();
            contexts.ui.uISession.HideGroup = new List<UiGroup>();
            contexts.ui.uISession.UiGroup = new Dictionary<UiGroup, List<IUiGroupController>>();

            contexts.ui.uI.HurtedDataList = new Dictionary<int, Shared.Components.Ui.CrossHairHurtedData>();
            contexts.ui.uI.KillInfos = new List<Shared.Components.Ui.IKillInfoItem>();
            contexts.ui.uI.KillFeedBackList = new List<int>();
            contexts.ui.uI.ScoreByCampTypeDict = new Dictionary<EUICampType, int>
            {
                {EUICampType.CT,0},
                {EUICampType.T,0},
                {EUICampType.None,0 }
            };
            contexts.ui.uIEntity.uI.NoticeInfoItem = new NoticeInfoItem();


            contexts.ui.map.RouteLineStartPoint = new MapFixedVector2(100, 60);
            contexts.ui.map.RouteLineEndPoint = new MapFixedVector2(300, 140);


#if !UNITY_EDITOR
            contexts.ui.uI.IsShowCrossHair = true;
#endif

            contexts.ui.map.OffLineLevel = 0;
            contexts.ui.map.CurPlayer = new MiniMapTeamPlayInfo();
            contexts.ui.map.TeamInfos = new List<MiniMapTeamPlayInfo>();
            contexts.ui.map.CurDuquan = new DuQuanInfo(contexts.ui.map.OffLineLevel, new MapFixedVector2(200, 200), 100, 5, 60);
            contexts.ui.map.NextDuquan = new DuQuanInfo(contexts.ui.map.OffLineLevel, new MapFixedVector2(200, 200), 10, 10, 140);
            contexts.ui.map.BombArea = new BombAreaInfo(new MapFixedVector2(100, 100), 1f, contexts.ui.map.OffLineLevel);
            contexts.ui.map.PlaneData = new AirPlaneData();
            contexts.ui.map.TeamPlayerMarkInfos = new List<TeamPlayerMarkInfo>();
            contexts.ui.map.MapMarks = new Dictionary<long, MiniMapPlayMarkInfo>();
            contexts.ui.uI.GroupBattleDataDict = new Dictionary<EUICampType, List<IGroupBattleData>>
            {
                {EUICampType.None, new List<IGroupBattleData>()},
                {EUICampType.CT, new List<IGroupBattleData>()},
                {EUICampType.T, new List<IGroupBattleData>()},
            };
            contexts.ui.uI.PlayerCountByCampTypeDict = new Dictionary<EUICampType, IPlayerCountData>
            {
                {EUICampType.None,new PlayerCountData()},
                {EUICampType.CT,new PlayerCountData()},
                {EUICampType.T,new PlayerCountData()},
            };

            contexts.ui.uI.CountdownTipDataList = new List<ICountdownTipData>();

            contexts.ui.uI.TaskTipDataList = new List<ITaskTipData>();

            contexts.ui.uI.LoadingRate = 0;
            contexts.ui.uI.LoadingText = "";
            contexts.ui.uI.PaintIdList = new List<int>();
            contexts.ui.uI.ChickenBagItemDataList = new List<IBaseChickenBagItemData>();

            //TestBagData(contexts.ui.uI.ChickenBagItemDataList);
            //TestPaintData(contexts.ui.uI.PaintIdList);
//            TestMapData(contexts);
        }

        private void TestBagData(List<IBaseChickenBagItemData> chickenBagItemDataList)
        {
            var item1 = new BaseChickenBagItemData { id = 11, key = "4|2", cat = 2};
            var item2 = new BaseChickenBagItemData { id = 11, key = "3|7", cat = 9,count = 11};
            var item3 = new BaseChickenBagItemData { id = 11, key = "5|22", cat = 5,count = 15};
            chickenBagItemDataList.Add(item1);
            chickenBagItemDataList.Add(item2);
            chickenBagItemDataList.Add(item3);
        }

        private void TestPaintData(List<int> list)
        {
            list.Add(3001);
            list.Add(0);
            list.Add(3003);
            list.Add(2001);
            list.Add(0);
            list.Add(3008);
        }

        private static void InitLoxodon()
        {
            var context = Context.GetApplicationContext();
            try
            {
                var bindingService = new BindingServiceBundle(context.GetContainer());
                bindingService.Start();
            }
            catch (Exception e)
            {

            }
        }

        private static void InitUiGlobalData(Contexts contexts)
        {
            contexts.ui.uI.IsShowCrossHair = true; //默认进入战斗是开启准心， 锁定鼠标
        }

        private void TestMapData(Contexts contexts)
        {
            contexts.ui.map.RouteLineStartPoint = new MapFixedVector2(10, 6);
            contexts.ui.map.RouteLineEndPoint = new MapFixedVector2(50, 30);

            contexts.ui.map.OffLineLevel = 1;
            contexts.ui.map.CurPlayer = new MiniMapTeamPlayInfo();
            contexts.ui.map.TeamInfos = new List<MiniMapTeamPlayInfo>();

            contexts.ui.map.CurDuquan = new DuQuanInfo(contexts.ui.map.OffLineLevel, new MapFixedVector2(28, 28), 5, 5, 60);
            contexts.ui.map.NextDuquan = new DuQuanInfo(contexts.ui.map.OffLineLevel, new MapFixedVector2(28, 28), 3, 10, 140);
            contexts.ui.map.BombArea = new BombAreaInfo(new MapFixedVector2(10, 10), 5, contexts.ui.map.OffLineLevel);
            contexts.ui.map.PlaneData = new AirPlaneData(){Type = 1, Pos = new MapFixedVector2(28, 28), Direction = 90f};
            contexts.ui.map.TeamPlayerMarkInfos = new List<TeamPlayerMarkInfo>();
            contexts.ui.map.MapMarks = new Dictionary<long, MiniMapPlayMarkInfo>();

            var go = new GameObject("plane");
            var plane = go.AddComponent<FreeRenderObject>();
            plane.raderImage = new RaderImage();
            plane.key = "plane";
            plane.model3D.x = 14;
            plane.model3D.z = 14;
            plane.AddEffect(FreeUIUtil.GetInstance().GetEffect(1));
            SingletonManager.Get<FreeEffectManager>().AddEffect(plane);

//            var go = new GameObject("plane");
//            var plane = go.AddComponent<FreeRenderObject>();
//            plane.raderImage = new RaderImage();
//            plane.key = "plane1";
//            plane.model3D.x = 14;
//            plane.model3D.z = 14;
//            plane.AddEffect(FreeUIUtil.GetInstance().GetEffect(1));
//            SingletonManager.Get<FreeEffectManager>().AddEffect(plane);

            contexts.ui.map.IsShowRouteLine = true;
            contexts.ui.map.RouteLineStartPoint = new MapFixedVector2(0,0);
            contexts.ui.map.RouteLineEndPoint = new MapFixedVector2(28,28);

        }

    }
}
