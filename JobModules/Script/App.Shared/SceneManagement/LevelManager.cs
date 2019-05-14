﻿using System;
using System.Collections.Generic;
using App.Shared.Configuration;
using App.Shared.SceneManagement.Basic;
using App.Shared.SceneManagement.Streaming;
using Core.SceneManagement;
using Core.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using Utils.AssetManager;
using Utils.Singleton;
using Object = System.Object;

namespace App.Shared.SceneManagement
{
    internal class StreamSceneObjectAssetPostProcessor : IAssetPostProcessor
    {
        private static LoggerAdapter _logger = new LoggerAdapter(typeof(StreamSceneObjectAssetPostProcessor));
        public readonly int i = 0;

        private readonly Dictionary<int, ShadowCastingMode> originShadows;
        private readonly Dictionary<int, LightProbeUsage> originLightProbes;
        private readonly Dictionary<int, bool> originStatus;
        private static readonly List<MeshRenderer> mrs = new List<MeshRenderer>(64);
        private static readonly List<Transform> trs = new List<Transform>(64);

        public StreamSceneObjectAssetPostProcessor(UnityObject unityObject)
        {
            mrs.Clear();
            trs.Clear();
            i = unityObject.AsGameObject.GetInstanceID();
            _logger.DebugFormat("StreamSceneObjectAssetPostProcessor :{0}", i);

            // record origin state info
            GameObject go = unityObject.AsGameObject;
            go.GetComponentsInChildren<MeshRenderer>(true, mrs);

            go.GetComponentsInChildren<Transform>(true, trs);
            originShadows = new Dictionary<int, ShadowCastingMode>(mrs.Count);
            originLightProbes = new Dictionary<int, LightProbeUsage>(mrs.Count);
            originStatus = new Dictionary<int, bool>(trs.Count);
            foreach (MeshRenderer mr in mrs)
            {
                if (mr != null)
                {
                    int id = mr.GetInstanceID();
                    originShadows.Add(id, mr.shadowCastingMode);
                    originLightProbes.Add(id, mr.lightProbeUsage);
                }
            }

            foreach (Transform tr in trs)
            {
                if (tr != null)
                {
                    int id = tr.GetInstanceID();
                    originStatus.Add(id, tr.gameObject.activeSelf);
                }
            }
        }

        public void LoadFromAsset(UnityObject unityObject)
        {
            _logger.DebugFormat("LoadFromAsset :{0} {1}", i, unityObject.AsGameObject);
        }

        public void LoadFromPool(UnityObject unityObject)
        {
            _logger.DebugFormat("LoadFromPool :{0} {1}", i, unityObject.AsGameObject);
            mrs.Clear();
            trs.Clear();
            // recover origin state info
            GameObject go = unityObject.AsGameObject;
            go.GetComponentsInChildren<MeshRenderer>(true,mrs);
            foreach (MeshRenderer mr in mrs)
            {
                if (mr != null)
                {
                    int id = mr.GetInstanceID();
                    if (originShadows.ContainsKey(id)) mr.shadowCastingMode = originShadows[id];
                    if (originLightProbes.ContainsKey(id)) mr.lightProbeUsage = originLightProbes[id];

                    // recover lightmap
                    // (由于室内物件的烘焙贴图在运行时决定，室外物件不启用烘焙，在资源回收时取消烘焙信息以避免复用时出现物件贴图不正确的情况)
                    mr.lightmapIndex = -1;
                }
            }

            go.GetComponentsInChildren<Transform>(true,trs);
            foreach (Transform tr in trs)
            {
                if (tr != null)
                {
                    int id = tr.GetInstanceID();
                    if (originStatus.ContainsKey(id) && tr.gameObject.activeSelf != originStatus[id])
                    {
                        tr.gameObject.SetActive(originStatus[id]);
                    }
                }
            }
        }

        public void Recyle(UnityObject unityObject)
        {
            _logger.DebugFormat("Recyle :{0} {1}", i, unityObject.AsGameObject);
        }

        public void OnDestory(UnityObject unityObject)
        {
            _logger.DebugFormat("OnDestory :{0} {1}", i, unityObject.AsGameObject);
        }
    }

    public class LevelManager : ILevelManager, ISceneResourceRequestHandler
    {
        private static LoggerAdapter _logger = new LoggerAdapter(typeof(LevelManager));

        private readonly AssetLoadOption _assetLoadOption;

        private IUnityAssetManager _assetManager;

        public IUnityAssetManager assetManager
        {
            get { return _assetManager; }
        }

        public LevelManager(IUnityAssetManager assetManager, bool isServer)
        {
            _assetManager = assetManager;
            if (isServer)
            {
                _assetLoadOption =
                    new AssetLoadOption(false, null, false);
            }
            else
            {
                _assetLoadOption =
                    new AssetLoadOption(false, null, false,
                        (unityObject) => new StreamSceneObjectAssetPostProcessor(unityObject));
            }

            SceneManager.sceneLoaded += SceneLoadedWrapper;
            SceneManager.sceneUnloaded += SceneUnloadedWrapper;
            _goLoadedProfile = SingletonManager.Get<DurationHelp>().GetCustomProfileInfo("LevelManager_GoLoaded");
            _afterGoLoadedProfile =
                SingletonManager.Get<DurationHelp>().GetCustomProfileInfo("LevelManager_AfterGoLoaded");
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= SceneLoadedWrapper;
            SceneManager.sceneUnloaded -= SceneUnloadedWrapper;
            SceneLoaded = null;
            SceneUnloaded = null;
            GoLoaded = null;
            AfterGoLoaded = null;
            BeforeGoUnloaded = null;
            GoUnloaded = null;
            DefaultGo.DisposeStreamGo();
        }

        #region ILevelManager

        public event Action<Scene, LoadSceneMode> SceneLoaded;
        public event Action<Scene> SceneUnloaded;
        public event Action<UnityObject> GoLoaded;
        public event Action<UnityObject> AfterGoLoaded;
        public event Action<UnityObject> BeforeGoUnloaded;
        public event Action<UnityObject> GoUnloaded;

        public void UpdateOrigin(Vector3 pos)
        {
            _sceneManager.UpdateOrigin(pos);
        }

        private void GoLoadedWrapper(string source, UnityObject unityObj)
        {
            if (GoLoaded != null)
            {
                try
                {
                    _goLoadedProfile.BeginProfile();
                    GoLoaded.Invoke(unityObj);
                }
                finally
                {
                    _goLoadedProfile.EndProfile();
                }
            }

            if (AfterGoLoaded != null)
            {
                try
                {
                    _afterGoLoadedProfile.BeginProfile();
                    AfterGoLoaded(unityObj);
                }
                finally
                {
                    _afterGoLoadedProfile.EndProfile();
                }
            }

            --NotFinishedRequests;
        }

        public void GetRequests(List<AssetInfo> sceneRequests, List<AssetInfo> goRequests)
        {
            if (_cachedLoadSceneRequest.Count != 0)
            {
                sceneRequests.AddRange(_cachedLoadSceneRequest);
                _cachedLoadSceneRequest.Clear();
            }

            if (_cachedLoadGoRequest.Count != 0)
            {
                goRequests.AddRange(_cachedLoadGoRequest);
                _cachedLoadGoRequest.Clear();
            }

            ProcessUnloadRequests();
        }

        public int NotFinishedRequests { get; private set; }

        public void LoadResource(string name, IUnityAssetManager assetManager, AssetInfo request)
        {
            assetManager.LoadAssetAsync(name, request, GoLoadedWrapper, _assetLoadOption);
        }

        #endregion

        private ISceneResourceManager _sceneManager;

        private readonly List<AssetInfo> _cachedLoadSceneRequest = new List<AssetInfo>();
        private readonly List<AssetInfo> _cachedLoadGoRequest = new List<AssetInfo>();
        private readonly Queue<string> _cachedUnloadSceneRequest = new Queue<string>();
        private readonly Queue<UnityObject> _cachedUnloadGoRequest = new Queue<UnityObject>();

        private List<string> _fixedSceneNames;
        private CustomProfileInfo _goLoadedProfile;
        private CustomProfileInfo _afterGoLoadedProfile;

        public void SetToWorldCompositionLevel(WorldCompositionParam param, IStreamingGoManager streamingGo)
        {
            _fixedSceneNames = param.FixedScenes;
            _sceneManager = new StreamingManager(this, streamingGo, SingletonManager.Get<StreamingLevelStructure>().Data,               
                SingletonManager.Get<ScenesLightmapStructure>().Data, SingletonManager.Get<ScenesIndoorCullStructure>().Data, param, _fixedSceneNames.Count);

            RequestForFixedScenes(param.AssetBundleName);
        }

        public void SetToFixedScenesLevel(OnceForAllParam param)
        {
            _fixedSceneNames = param.FixedScenes;
            _sceneManager = new FixedScenesManager(this, param);

            RequestForFixedScenes(param.AssetBundleName);
        }

        public void SetAsapMode(bool value)
        {
            _sceneManager.SetAsapMode(value);
        }

        #region ISceneResourceRequestHandler

        public void AddUnloadSceneRequest(string sceneName)
        {
            _cachedUnloadSceneRequest.Enqueue(sceneName);
            ++NotFinishedRequests;
        }

        public void AddLoadSceneRequest(AssetInfo addr)
        {
            _cachedLoadSceneRequest.Add(addr);
            ++NotFinishedRequests;
        }

        public void AddLoadGoRequest(AssetInfo addr)
        {
            _cachedLoadGoRequest.Add(addr);
            ++NotFinishedRequests;
        }

        public void AddUnloadGoRequest(UnityObject unityObj)
        {
            _cachedUnloadGoRequest.Enqueue(unityObj);
            ++NotFinishedRequests;
        }

        #endregion

        private void SceneLoadedWrapper(Scene scene, LoadSceneMode mode)
        {
            if (_fixedSceneNames != null && _fixedSceneNames.Contains(scene.name))
                SceneManager.SetActiveScene(scene);

            if (SceneLoaded != null)
                SceneLoaded.Invoke(scene, mode);

            --NotFinishedRequests;

            _logger.InfoFormat("scene loaded {0}", scene.name);
        }

        private void SceneUnloadedWrapper(Scene scene)
        {
            if (SceneUnloaded != null)
                SceneUnloaded.Invoke(scene);

            --NotFinishedRequests;

            _logger.InfoFormat("scene unloaded {0}", scene.name);
        }

        private void ProcessUnloadRequests()
        {
            var goCount = _cachedUnloadGoRequest.Count;
            var sceneCount = _cachedUnloadSceneRequest.Count;

            for (int i = 0; i < goCount; i++)
            {
                var go = _cachedUnloadGoRequest.Dequeue();

                if (BeforeGoUnloaded != null)
                    BeforeGoUnloaded(go);

                if (GoUnloaded != null)
                    GoUnloaded.Invoke(go);

                _assetManager.Recycle(go);

                --NotFinishedRequests;
            }

            for (int i = 0; i < sceneCount; i++)
            {
                var scene = _cachedUnloadSceneRequest.Dequeue();
                SceneManager.UnloadSceneAsync(scene);
                _logger.InfoFormat("unload scene {0}", scene);
            }
        }

        private void RequestForFixedScenes(string bundleName)
        {
            foreach (var sceneName in _fixedSceneNames)
            {
                AddLoadSceneRequest(new AssetInfo
                {
                    BundleName = bundleName,
                    AssetName = sceneName
                });
            }
        }
    }
}