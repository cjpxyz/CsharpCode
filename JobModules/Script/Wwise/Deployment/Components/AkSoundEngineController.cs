#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
public class AkSoundEngineController
{
    #region Public Data Members

    public static string s_DefaultBasePath {

        get
        {
            return AudioPluginManagement.GetBankAssetFolder();
        }
    }

    
	public static string s_Language = "English(US)";
	public static int s_DefaultPoolSize = 4096;
	public static int s_LowerPoolSize = 2048;
	public static int s_StreamingPoolSize = 1024;
	public static int s_PreparePoolSize = 0;
	public static float s_MemoryCutoffThreshold = 0.95f;
	public static int s_MonitorPoolSize = 128;
	public static int s_MonitorQueuePoolSize = 64;
	public static int s_CallbackManagerBufferSize = 4;
	public static bool s_EngineLogging = true;
	public static int s_SpatialAudioPoolSize = 4096;

	//public string basePath = s_DefaultBasePath;
	public string language = s_Language;
	public bool engineLogging = s_EngineLogging;

	#endregion

	private static AkSoundEngineController ms_Instance;

	public static AkSoundEngineController Instance
	{
		get
		{
			if (ms_Instance == null)
				ms_Instance = new AkSoundEngineController();

			return ms_Instance;
		}
	}

	private AkSoundEngineController()
	{
#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.pauseStateChanged += OnPauseStateChanged;
#else
		UnityEditor.EditorApplication.playmodeStateChanged += OnEditorPlaymodeStateChanged;
#endif
#endif
	}

	~AkSoundEngineController()
	{
		if (ms_Instance == this)
		{
#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
			UnityEditor.EditorApplication.pauseStateChanged -= OnPauseStateChanged;
#else
			UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlaymodeStateChanged;
#endif
			UnityEditor.EditorApplication.update -= LateUpdate;
#endif
			ms_Instance = null;
		}
	}

	public static string GetDecodedBankFolder()
	{
		return "DecodedBanks";
	}

	public static string GetDecodedBankFullPath()
	{
#if (UNITY_ANDROID || PLATFORM_LUMIN || UNITY_IOS || UNITY_SWITCH) && !UNITY_EDITOR
// This is for platforms that only have a specific file location for persistent data.
		return System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, GetDecodedBankFolder());
#else
		return System.IO.Path.Combine(AkBasePathGetter.GetPlatformBasePath(), GetDecodedBankFolder());
#endif
	}

	public void LateUpdate()
	{
#if UNITY_EDITOR
		if (!IsSoundEngineLoaded)
			return;
#endif

		//Execute callbacks that occurred in last frame (not the current update)
		AkCallbackManager.PostCallbacks();
		AkBankManager.DoUnloadBanks();
		AkSoundEngine.RenderAudio();
	}

	public void Init(AkInitializer akInitializer)
	{
		if (akInitializer == null)
		{
			UnityEngine.Debug.LogError("WwiseUnity: AkInitializer must not be null. Sound engine will not be initialized.");
			return;
		}

#if UNITY_EDITOR
		if (UnityEngine.Application.isPlaying && !IsTheSingleOwningInitializer(akInitializer))
		{
			UnityEngine.Debug.LogError("WwiseUnity: Sound engine is already initialized.");
			return;
		}

		var arguments = System.Environment.GetCommandLineArgs();
		if (System.Array.IndexOf(arguments, "-nographics") >= 0 &&
			System.Array.IndexOf(arguments, "-wwiseEnableWithNoGraphics") < 0)
			return;

		var isInitialized = false;
		try
		{
			isInitialized = AkSoundEngine.IsInitialized();
			IsSoundEngineLoaded = true;
		}
		catch (System.DllNotFoundException)
		{
			IsSoundEngineLoaded = false;
			UnityEngine.Debug.LogWarning("WwiseUnity: AkSoundEngine is not loaded.");
			return;
		}
#else
		var isInitialized = AkSoundEngine.IsInitialized();
#endif

		engineLogging = akInitializer.engineLogging;

		AkLogger.Instance.Init();

		AKRESULT result;
		uint BankID;
		if (isInitialized)
		{
#if UNITY_EDITOR
			if (UnityEngine.Application.isPlaying || UnityEditor.BuildPipeline.isBuildingPlayer)
			{
				AkSoundEngine.ClearBanks();
				AkBankManager.Reset();

				result = AkSoundEngine.LoadBank("Init.bnk", AkSoundEngine.AK_DEFAULT_POOL_ID, out BankID);
				if (result != AKRESULT.AK_Success)
					UnityEngine.Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + result);
			}

			result = AkCallbackManager.Init(akInitializer.callbackManagerBufferSize * 1024);
			if (result != AKRESULT.AK_Success)
			{
				UnityEngine.Debug.LogError("WwiseUnity: Failed to initialize Callback Manager. Terminate sound engine.");
				AkSoundEngine.Term();
				return;
			}

			OnEnableEditorListener(akInitializer.gameObject);
			UnityEditor.EditorApplication.update += LateUpdate;
#else
			UnityEngine.Debug.LogError("WwiseUnity: Sound engine is already initialized.");
#endif
			return;
		}

#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer)
			return;
#endif

		UnityEngine.Debug.Log("WwiseUnity: Initialize sound engine ...");
		var basePath = s_DefaultBasePath;
		language = akInitializer.language;

        if (!AudioPluginManagement.SettingMgr.Initialize()) return;
#if !UNITY_SWITCH
		// Calling Application.persistentDataPath crashes Switch
		var decodedBankFullPath = GetDecodedBankFullPath();
		// AkSoundEngine.SetDecodedBankPath creates the folders for writing to (if they don't exist)
        ///�趨����·����
		AkSoundEngine.SetDecodedBankPath(decodedBankFullPath);
#endif

		AkSoundEngine.SetCurrentLanguage(language);

#if !UNITY_SWITCH
		// Calling Application.persistentDataPath crashes Switch
		///�趨����·������windows����Ч��AkSoundEngine.AddBasePath is currently only implemented for iOS and Android; No-op for all other platforms.
		AkSoundEngine.AddBasePath(UnityEngine.Application.persistentDataPath + System.IO.Path.DirectorySeparatorChar);
		// Adding decoded bank path last to ensure that it is the first one used when writing decoded banks.
		AkSoundEngine.AddBasePath(decodedBankFullPath);
#endif

		result = AkCallbackManager.Init(AudioPluginManagement.SettingData.callbackManagerBufferSize * 1024);
		if (result != AKRESULT.AK_Success)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Failed to initialize Callback Manager. Terminate sound engine.");
			AkSoundEngine.Term();
			return;
		}

		AkBankManager.Reset();

		UnityEngine.Debug.Log("WwiseUnity: Sound engine initialized.");

		//Load the init bank right away.  Errors will be logged automatically.
		result = AkSoundEngine.LoadBank("Init.bnk", AkSoundEngine.AK_DEFAULT_POOL_ID, out BankID);
   
        if (result != AKRESULT.AK_Success)
			UnityEngine.Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + result);

#if UNITY_EDITOR
		OnEnableEditorListener(akInitializer.gameObject);
		UnityEditor.EditorApplication.update += LateUpdate;
#endif
       
	}

	public void OnDisable()
	{
#if UNITY_EDITOR
		if (!IsSoundEngineLoaded)
			return;

		OnDisableEditorListener();
#endif
	}

	public void Terminate()
	{
#if UNITY_EDITOR
		ClearInitializeState();

		if (!IsSoundEngineLoaded)
			return;
#endif

		if (!AkSoundEngine.IsInitialized())
			return;

		// Stop everything, and make sure the callback buffer is empty. We try emptying as much as possible, and wait 10 ms before retrying.
		// Callbacks can take a long time to be posted after the call to RenderAudio().
		AkSoundEngine.StopAll();
		AkSoundEngine.ClearBanks();
		AkSoundEngine.RenderAudio();
		var retry = 5;
		do
		{
			var numCB = 0;
			do
			{
				numCB = AkCallbackManager.PostCallbacks();

				// This is a WSA-friendly sleep
				using (System.Threading.EventWaitHandle tmpEvent = new System.Threading.ManualResetEvent(false))
				{
					tmpEvent.WaitOne(System.TimeSpan.FromMilliseconds(1));
				}
			}
			while (numCB > 0);

			// This is a WSA-friendly sleep
			using (System.Threading.EventWaitHandle tmpEvent = new System.Threading.ManualResetEvent(false))
			{
				tmpEvent.WaitOne(System.TimeSpan.FromMilliseconds(10));
			}

			retry--;
		}
		while (retry > 0);

		AkSoundEngine.Term();

		// Make sure we have no callbacks left after Term. Some might be posted during termination.
		AkCallbackManager.PostCallbacks();

		AkCallbackManager.Term();
		AkBankManager.Reset();
	}

	// In the Editor, the sound needs to keep playing when switching windows (remote debugging in Wwise, for example).
	// On iOS, application interruptions are handled in the sound engine already.
#if UNITY_EDITOR || UNITY_IOS
	public void OnApplicationPause(bool pauseStatus)
	{
	}

	public void OnApplicationFocus(bool focus)
	{
	}
#else
	public void OnApplicationPause(bool pauseStatus) 
	{
		ActivateAudio(!pauseStatus);
	}

	public void OnApplicationFocus(bool focus)
	{
		ActivateAudio(focus);
	}
#endif

#if UNITY_EDITOR
	public bool IsSoundEngineLoaded { get; set; }

	// Enable/Disable the audio when pressing play/pause in the editor.
#if UNITY_2017_2_OR_NEWER
	private void OnPauseStateChanged(UnityEditor.PauseState pauseState)
	{
		ActivateAudio(pauseState != UnityEditor.PauseState.Paused);
	}
#else
	private void OnEditorPlaymodeStateChanged()
	{
		ActivateAudio(!UnityEditor.EditorApplication.isPaused);
	}
#endif
#endif

#if UNITY_EDITOR || !UNITY_IOS
	private void ActivateAudio(bool activate)
	{
		if (AkSoundEngine.IsInitialized())
		{
			if (activate)
				AkSoundEngine.WakeupFromSuspend();
			else
				AkSoundEngine.Suspend();

			AkSoundEngine.RenderAudio();
		}
	}
#endif

#if UNITY_EDITOR
	#region Editor Listener
	private UnityEngine.GameObject editorListenerGameObject;

	private bool IsPlayingOrIsNotInitialized
	{
		get { return UnityEngine.Application.isPlaying || !AkSoundEngine.IsInitialized(); }
	}

	private void OnEnableEditorListener(UnityEngine.GameObject gameObject)
	{
		if (IsPlayingOrIsNotInitialized || editorListenerGameObject != null)
			return;

		editorListenerGameObject = gameObject;
		AkSoundEngine.RegisterGameObj(editorListenerGameObject, editorListenerGameObject.name);

		// Do not create AkGameObj component when adding this listener
		var id = AkSoundEngine.GetAkGameObjectID(editorListenerGameObject);
		AkSoundEnginePINVOKE.CSharp_AddDefaultListener(id);

		UnityEditor.EditorApplication.update += UpdateEditorListenerPosition;
	}

	private void OnDisableEditorListener()
	{
		if (IsPlayingOrIsNotInitialized || editorListenerGameObject == null)
			return;

		UnityEditor.EditorApplication.update -= UpdateEditorListenerPosition;

		var id = AkSoundEngine.GetAkGameObjectID(editorListenerGameObject);
		AkSoundEnginePINVOKE.CSharp_RemoveDefaultListener(id);

		AkSoundEngine.UnregisterGameObj(editorListenerGameObject);
		editorListenerGameObject = null;
	}

	private UnityEngine.Vector3 editorListenerPosition = UnityEngine.Vector3.zero;
	private UnityEngine.Vector3 editorListenerForward = UnityEngine.Vector3.zero;
	private UnityEngine.Vector3 editorListenerUp = UnityEngine.Vector3.zero;

	private void UpdateEditorListenerPosition()
	{
		if (IsPlayingOrIsNotInitialized || editorListenerGameObject == null)
			return;

		if (UnityEditor.SceneView.lastActiveSceneView == null)
			return;

		var sceneViewCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
		if (sceneViewCamera == null)
			return;

		var sceneViewTransform = sceneViewCamera.transform;
		if (sceneViewTransform == null)
			return;

		if (editorListenerPosition == sceneViewTransform.position &&
			editorListenerForward == sceneViewTransform.forward &&
			editorListenerUp == sceneViewTransform.up)
			return;

		AkSoundEngine.SetObjectPosition(editorListenerGameObject, sceneViewTransform);

		editorListenerPosition = sceneViewTransform.position;
		editorListenerForward = sceneViewTransform.forward;
		editorListenerUp = sceneViewTransform.up;
	}
	#endregion

	#region Initialize only once
	private AkInitializer TheAkInitializer = null;

	/// <summary>
	/// Determines whether this AkInitializer is the single one responsible for initializing the sound engine.
	/// </summary>
	/// <param name="akInitializer"></param>
	/// <returns>Returns true when called on the first AkInitializer and false otherwise.</returns>
	private bool IsTheSingleOwningInitializer(AkInitializer akInitializer)
	{
		if (TheAkInitializer == null && akInitializer != null)
		{
			TheAkInitializer = akInitializer;
			return true;
		}

		return false;
	}

	private void ClearInitializeState()
	{
		TheAkInitializer = null;
	}
	#endregion
#endif // UNITY_EDITOR
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.