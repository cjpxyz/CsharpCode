﻿using App.Client.GameModules.Ui.UiAdapter.Interface;
using App.Shared.Components.Ui;
using Core.Enums;

namespace App.Client.GameModules.Ui.UiAdapter
{
    public class PlayerCountData : IPlayerCountData
    {
        public int PlayerCount { get; set; }
        public int DeadPlayerCount { get; set; }
        //public int Score { get; set; }
    }

    public class BlastScoreUiAdapter : UIAdapter, IBlastScoreUiAdapter
    {
        private Contexts _contexts;
        private Shared.Components.Ui.UIComponent _ui;

        public BlastScoreUiAdapter(Contexts contexts)
        {
            _contexts = contexts;
            _ui = contexts.ui.uI;
        }
        public int GameTime
        {
            get { return _ui.GameTime; }
            set { _ui.GameTime = value; }
        }

        public EUIBombInstallState C4InstallState
        {
            get { return _ui.C4InstallState; }
            set { _ui.C4InstallState = value; }
        }

        public int PlayerCapacityPerCamp
        {
            get { return _ui.PlayerCapacity / 2; }
        }

        public int ScoreForWin
        {
            get { return _ui.ScoreForWin; }
        }

        public int GetScoreByCamp(EUICampType type)
        {
            return _ui.ScoreByCampTypeDict[(int)type];
        }

        public IPlayerCountData GetDataByCampType(EUICampType campType)
        {
            return _ui.PlayerCountByCampTypeDict[(int)campType];
        }

        public bool NeedPause
        {
            get
            {
                return _contexts.ui.uI.IsPause;
            }
        }

    }
}