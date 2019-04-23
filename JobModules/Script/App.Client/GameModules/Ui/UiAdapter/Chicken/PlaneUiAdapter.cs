﻿using App.Client.GameModules.Ui.UiAdapter.Interface;

namespace App.Client.GameModules.Ui.UiAdapter
{

    public class PlaneUiAdapter : UIAdapter, IPlaneUiAdapter
    {
        private Contexts _contexts;
        public PlaneUiAdapter(Contexts contexts)
        {
            _contexts = contexts;
        }

        public int CurCount
        {
            get { return _contexts.ui.uI.CurPlayerCountInPlane; }

        }

        public int TotalCount
        {
            get
            {
                return  _contexts.ui.uI.TotalPlayerCountInPlane; 
            }
        }
    }
}
