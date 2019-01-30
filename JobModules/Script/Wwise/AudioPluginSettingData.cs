using System.Collections;
using System;
using UnityEngine;

[System.Serializable]
public class AudioPluginSettingData
{

    //  public const string WwiseSettingsFilename = "WwiseSettings.xml";

    //  private static WwiseSettings instance;

    public bool CopySoundBanksAsPreBuildStep = false;

    public static bool D_CreatedPicker = false;
    //default is true
    public bool CreateWwiseGlobal = true;
    public const bool D_CreateWwiseGlobal = true;
    //default is false
    public bool CreateWwiseListener = false;
    public static bool D_CreateWwiseListener = false;

    public bool GenerateSoundBanksAsPreBuildStep = false;
    public bool ShowMissingRigidBodyWarning = false;
    public static string BankEditorAssetRelativePath = "Assets/Sound/WiseBank";

    /// <summary>
    ///�� WAV ���ݵĴ洢λ��(KB)
    /// </summary>
    public uint defaultPoolSizeKB = 8 * 1024;

    /// <summary>
    /// �ײ㲥�Ż����ڴ��.
    ///This contains the audio processing buffers and DSP data.  
    /// </summary>
    public uint lowerPoolSizeKB = 8 * 1024;

    /// <summary>
    /// uLEngineDefaultPoolSize �ڴ�ط�ֵ,���LowerPool
    /// </summary>
    public float memoryCutoffThreshold = 0.95f;

    /// <summary>
    /// �ռ������Ƶ�ߴ�
    /// </summary>
    public uint spatialAudioPoolSizeKB = 4 * 1024;


    /// <summary>
    /// streaming pool
    /// </summary>
    public uint streamingPoolSizeKB = 4 * 1024;
    /// <summary>
    /// preparePool
    /// </summary>
    public uint preparePoolSizeKB = 0;
    /// <summary>
    /// mem max Pool length
    /// </summary>
    public uint maxPoolNum = 20;


    /// <summary>
    /// �ص��ڴ����
    /// </summary>
    public int callbackManagerBufferSize = 4 * 1024;

    //optional 
    //  public string language = AkSoundEngineController.s_Language;


    public string BankFolder_UnityEditor;
    // public string WwiseInstallationPathMac = @"E:\Wwise 2017.2.8.6698\";
    public AudioPluginSettingData()
    {
        Init();
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void Init()
    {
        BankFolder_UnityEditor = System.IO.Path.Combine(Application.dataPath, BankEditorAssetRelativePath);
        AkBasePathGetter.FixSlashes(ref BankFolder_UnityEditor);
    }
}