﻿using App.Server.GameModules.GamePlay.free.player;
using App.Server.GameModules.GamePlay.Free.chicken;
using App.Server.GameModules.GamePlay.Free.item.config;
using App.Server.GameModules.GamePlay.Free.ui;
using App.Server.GameModules.GamePlay.Free.weapon;
using Assets.App.Server.GameModules.GamePlay.Free;
using Assets.Utils.Configuration;
using com.cpkf.yyjd.tools.util;
using com.wd.free.action;
using com.wd.free.item;
using com.wd.free.para;
using com.wd.free.skill;
using Core.Free;
using Free.framework;
using gameplay.gamerule.free.ui;
using Sharpen;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Singleton;

namespace App.Server.GameModules.GamePlay.Free.item
{
    [Serializable]
    public class UnityOneInventoryUi : IInventoryUI, IRule
    {
        private IGameAction errorAction;

        private IGameAction moveOutAction;

        private IGameAction canNotMoveAction;

        private IGameAction moveAction;

        private string uiKey;
        private string image;
        private string name;
        private string count;

        [System.NonSerialized]
        private long lastErrorTime;

        public IGameAction MoveAction
        {
            get { return moveAction; }
        }

        public IGameAction CanNotMoveAction
        {
            get { return canNotMoveAction; }
        }

        public IGameAction MoveOutAction
        {
            get { return MoveOutAction; }
        }

        public IGameAction ErrorAction
        {
            get { return errorAction; }
        }

        public void AddItem(ISkillArgs args, ItemInventory inventory, ItemPosition ip)
        {
            ReDraw(args, inventory, true);
        }

        public void DeleteItem(ISkillArgs args, ItemInventory inventory, ItemPosition ip)
        {
            ReDraw(args, inventory, true);
        }

        public void Error(ISkillArgs args, ItemInventory inventory, string msg)
        {
            if (errorAction != null)
            {
                if (Runtime.CurrentTimeMillis(false) - lastErrorTime > 2000)
                {
                    if (args != null)
                    {
                        args.GetDefault().GetParameters().TempUse(new StringPara("message", msg));
                        errorAction.Act(args);
                        args.GetDefault().GetParameters().Resume("message");
                    }
                    lastErrorTime = Runtime.CurrentTimeMillis(false);
                }
            }
        }

        private FreeUIUpdateAction update;

        public void ReDraw(ISkillArgs args, ItemInventory inventory, bool includeBack)
        {
            SimpleProto itemInfo = FreePool.Allocate();
            itemInfo.Key = FreeMessageConstant.ItemInfo;
            itemInfo.Ss.Add(inventory.name);

            if (inventory.posList.Count > 0)
            {
                ItemPosition ip = inventory.posList[0];
                itemInfo.Bs.Add(false);
                FreeItemInfo info = FreeItemConfig.GetItemInfo(ip.key.GetKey());
                itemInfo.Ins.Add(info.cat);
                itemInfo.Ins.Add(info.id);
                itemInfo.Ins.Add(ip.GetCount());
            }
            else
            {
                itemInfo.Bs.Add(true);
            }

            FreeMessageSender.SendMessage(args, "current", itemInfo);
        }

        private void clearPart(ItemInventory inventory, ISkillArgs args, FreeData fd)
        {
            if (inventory.name == ChickenConstant.BagPrimeWeapon || inventory.name == ChickenConstant.BagSecondaryWeapon || inventory.name == ChickenConstant.BagPistolWeapon)
            {
                ShowPartAction show = new ShowPartAction();
                show.show = false;
                show.SetPlayer("current");
                show.SetScope(1);

                if (inventory.name == ChickenConstant.BagPrimeWeapon)
                {
                    show.weaponKey = "1";
                    ClearPart(args, fd, 1);
                }
                else if (inventory.name == ChickenConstant.BagSecondaryWeapon)
                {
                    show.weaponKey = "2";
                    ClearPart(args, fd, 2);
                }
                else if (inventory.name == ChickenConstant.BagPistolWeapon)
                {
                    show.weaponKey = "3";
                    ClearPart(args, fd, 3);
                }
                show.parts = "1,2,3,4,5";
                show.Act(args);
            }
        }

        private void redrawPart(ItemInventory inventory, ISkillArgs args, ItemPosition ip, FreeData fd)
        {
            if (inventory.name == ChickenConstant.BagPrimeWeapon || inventory.name == ChickenConstant.BagSecondaryWeapon || inventory.name == ChickenConstant.BagPistolWeapon)
            {
                int id = (int)((IntPara)ip.GetParameters().Get("itemId")).GetValue();

                WeaponAllConfigs configs = SingletonManager.Get<WeaponConfigManagement>().FindConfigById(id);
                if (configs != null)
                {
                    HashSet<int> list = new HashSet<int>();

                    foreach (XmlConfig.EWeaponPartType part in configs.ApplyPartsSlot)
                    {
                        list.Add(FreeWeaponUtil.GetWeaponPart(part));
                    }

                    List<string> showP = new List<string>();
                    List<string> hideP = new List<string>();

                    for (int i = 1; i <= 5; i++)
                    {
                        if (list.Contains(i))
                        {
                            showP.Add(i.ToString());
                        }
                        else
                        {
                            hideP.Add(i.ToString());
                        }
                    }

                    ShowPartAction show = new ShowPartAction();
                    show.show = true;
                    show.SetPlayer("current");
                    show.SetScope(1);
                    if (inventory.name == ChickenConstant.BagPrimeWeapon)
                    {
                        show.weaponKey = "1";
                    }
                    else if (inventory.name == ChickenConstant.BagSecondaryWeapon)
                    {
                        show.weaponKey = "2";
                    }
                    else
                    {
                        show.weaponKey = "3";
                    }

                    show.parts = StringUtil.GetStringFromStrings(showP, ",");
                    show.Act(args);

                    show.show = false;
                    show.parts = StringUtil.GetStringFromStrings(hideP, ",");
                    show.Act(args);
                }
                else
                {
                    Debug.LogError(ip.key.GetName() + " 没有定义配件.");
                }
            }
        }

        private void ClearPart(ISkillArgs fr, FreeData fd, int key)
        {
            for (int i = 1; i <= 5; i++)
            {
                ItemInventory ii = fd.freeInventory.GetInventoryManager().GetInventory("w" + key + "" + i);
                if (ii != null)
                {
                    ii.Clear();
                    ii.GetInventoryUI().ReDraw(fr, ii, true);
                }
            }
        }

        public void UpdateItem(ISkillArgs args, ItemInventory inventory, ItemPosition ip)
        {
            ReDraw(args, inventory, true);
        }

        public void UseItem(ISkillArgs args, ItemInventory inventory, ItemPosition ip)
        {
            ReDraw(args, inventory, true);
        }

        public int GetRuleID()
        {
            return (int)ERuleIds.UnityOneInventoryUi;
        }
    }
}
