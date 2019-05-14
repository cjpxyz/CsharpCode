using Assets.Utils.Configuration;
using Core.Components;
using Core.Prediction.UserPrediction;
using Core.SnapshotReplication.Serialization.NetworkProperty;
using Core.Utils;
using Entitas.CodeGeneration.Attributes;
using System.Text;
using Utils.Singleton;

namespace App.Shared.Components.Weapon
{
    [Weapon]
    public class WeaponBasicDataComponent : IUserPredictionComponent
    {
        [DontInitilize, NetworkProperty] public int ConfigId;
        [DontInitilize, NetworkProperty] public int WeaponAvatarId;
        [DontInitilize, NetworkProperty] public int UpperRail;
        [DontInitilize, NetworkProperty] public int LowerRail;
        [DontInitilize, NetworkProperty] public int SideRail;
        [DontInitilize, NetworkProperty] public int Stock;
        [DontInitilize, NetworkProperty] public int Muzzle;
        [DontInitilize, NetworkProperty] public int Magazine;
        [DontInitilize, NetworkProperty] public int Bullet;
        [DontInitilize, NetworkProperty] public bool PullBolt;
        [DontInitilize, NetworkProperty] public int FireModel;
        [DontInitilize, NetworkProperty] public int ReservedBullet;
        [DontInitilize] private WeaponAllConfigs configCache;
        [DontInitilize, NetworkProperty] public int Bore;
        [DontInitilize, NetworkProperty] public int Feed;
        [DontInitilize, NetworkProperty] public int Trigger;
        [DontInitilize, NetworkProperty] public int Interlock;
        [DontInitilize, NetworkProperty] public int Brake;

        public int RealFireModel
        {
            get
            {
                if (FireModel == 0)
                {
                    if (configCache == null || configCache.S_Id != ConfigId)
                        configCache = SingletonManager.Get<WeaponConfigManagement>().FindConfigById(ConfigId);
                    return (int) configCache.GetDefaultFireModel();
                }
                return FireModel;
            }
        }

        public void CopyFrom(object rightComponent)
        {
            var remote = rightComponent as WeaponBasicDataComponent;
            ConfigId = remote.ConfigId;
            WeaponAvatarId = remote.WeaponAvatarId;
            UpperRail = remote.UpperRail;
            LowerRail = remote.LowerRail;
            SideRail = remote.SideRail;
            Stock = remote.Stock;
            Muzzle = remote.Muzzle;
            Magazine = remote.Magazine;
            Bullet = remote.Bullet;
            ReservedBullet = remote.ReservedBullet;
            PullBolt = remote.PullBolt;
            FireModel = remote.FireModel;
            Bore = remote.Bore;
            Feed = remote.Feed;
            Trigger = remote.Trigger;
            Interlock = remote.Interlock;
            Brake = remote.Brake;
        }

        public int GetComponentId()
        {
            return (int) EComponentIds.WeaponBasicInfo;
        }

        private static LoggerAdapter Logger = new LoggerAdapter(typeof(WeaponBasicDataComponent));

        StringBuilder builder = new StringBuilder();

        public bool IsApproximatelyEqual(object right)
        {
            var remote = right as WeaponBasicDataComponent;
            var result = ConfigId == remote.ConfigId && WeaponAvatarId == remote.WeaponAvatarId && UpperRail == remote.UpperRail &&
                LowerRail == remote.LowerRail && SideRail == remote.SideRail && Stock == remote.Stock &&
                Muzzle == remote.Muzzle && Magazine == remote.Magazine && Bullet == remote.Bullet &&
                ReservedBullet == remote.ReservedBullet && PullBolt == remote.PullBolt && FireModel == remote.FireModel &&
                Bore == remote.Bore && Feed == remote.Feed && Trigger == remote.Trigger &&
                Interlock == remote.Interlock && Brake == remote.Brake;
            return result;
        }

        public void RewindTo(object rightComponent)
        {
            CopyFrom(rightComponent);
        }

        public void Reset()
        {
            CopyFrom(Empty);
        }

        public static readonly WeaponBasicDataComponent Empty = new WeaponBasicDataComponent();
    }
}