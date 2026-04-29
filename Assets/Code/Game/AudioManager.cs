using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("# Audio Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    [System.Serializable]
    public class BGMPair
    {
        public string key;      // BGM 이름
        public AudioClip clip;  // 실제 클립
    }

    [Header("# BGM")]
    public List<BGMPair> BGMPairs = new List<BGMPair>();
    public float BGMVolume = 0.5f;
    private AudioSource BGMPlayer;
    private AudioHighPassFilter BGMEffet;
    private Dictionary<string, AudioClip> BGMDict;

    [Header("# SFX")]
    public float SFXVolume = 0.5f;
    public int channels = 16;
    private AudioSource[] SFXPlayers;
    private int channelIndex;

    [Header("# 3D Sound Settings")]
    [Range(0f, 1f)] public float spatialBlend = 1f; // 1이면 완전한 3D
    public float minDistance = 1f;  // 소리가 최대인 거리
    public float maxDistance = 30f; // 소리가 안 들리게 되는 거리

    [Header("# Volume UI")]
    public Scrollbar BGMScrollbar;
    public Scrollbar SFXScrollbar;

    [System.Serializable]
    public class SFXPair
    {
        public string key;
        public AudioClip clip;
    }

    public List<SFXPair> SFXPairs = new List<SFXPair>();
    private Dictionary<string, AudioClip> SFXDict;

    // DDOL로 매 씬마다 AudioManager를 재사용함.
    // TODO = LeftRoom() 마다 출력 중인 BGM과 SFX를 종료해야함.
    // TODO = JoinedRoom() 마다 BGM을 변경해야함.
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Init();

        // UI 연결
        if (BGMScrollbar != null)
        {
            BGMScrollbar.value = BGMVolume;
            BGMScrollbar.onValueChanged.AddListener(SetBGMVolume);
        }
        if (SFXScrollbar != null)
        {
            SFXScrollbar.value = SFXVolume;
            SFXScrollbar.onValueChanged.AddListener(SetSFXVolume);
        }

        // 딕셔너리 초기화
        BGMDict = new Dictionary<string, AudioClip>();
        foreach (var pair in BGMPairs)
        {
            if (!BGMDict.ContainsKey(pair.key)) BGMDict.Add(pair.key, pair.clip);
        }

        SFXDict = new Dictionary<string, AudioClip>();
        foreach (var pair in SFXPairs)
        {
            if (!SFXDict.ContainsKey(pair.key)) SFXDict.Add(pair.key, pair.clip);
        }
    }

    void Init()
    {
        // BGM Player 설정
        GameObject BGMObject = new GameObject("BGMPlayer");
        BGMObject.transform.parent = transform;
        BGMPlayer = BGMObject.AddComponent<AudioSource>();
        BGMPlayer.playOnAwake = false;
        BGMPlayer.loop = true;
        BGMPlayer.volume = BGMVolume;
        BGMPlayer.priority = 0;
        if (bgmGroup != null) BGMPlayer.outputAudioMixerGroup = bgmGroup;

        BGMEffet = BGMObject.AddComponent<AudioHighPassFilter>();
        BGMEffet.enabled = false;

        // SFX Players 채널 설정
        SFXPlayers = new AudioSource[channels];
        for (int i = 0; i < channels; i++)
        {
            GameObject SFXObject = new GameObject("SFXPlayer_" + i);
            SFXObject.transform.parent = transform;
            SFXPlayers[i] = SFXObject.AddComponent<AudioSource>();
            SFXPlayers[i].playOnAwake = false;
            SFXPlayers[i].volume = SFXVolume;
            SFXPlayers[i].bypassListenerEffects = true;

            // --- 3D 기본 설정 ---
            SFXPlayers[i].spatialBlend = 0f; // 기본은 2D (UI 등)
            SFXPlayers[i].rolloffMode = AudioRolloffMode.Logarithmic;
            SFXPlayers[i].minDistance = minDistance;
            SFXPlayers[i].maxDistance = maxDistance;
            // --------------------

            if (sfxGroup != null) SFXPlayers[i].outputAudioMixerGroup = sfxGroup;
        }
    }

    /// <summary>
    /// 2D 효과음 출력 (UI, 시스템 사운드용)
    /// </summary>
    public void PlaySFX(string key)
    {
        InternalPlaySFX(key, Vector3.zero, false);
    }

    /// <summary>
    /// 3D 효과음 출력 (월드 내 특정 위치 사운드용)
    /// </summary>
    public void PlaySFX(string key, Vector3 worldPosition)
    {
        InternalPlaySFX(key, worldPosition, true);
    }

    private void InternalPlaySFX(string key, Vector3 position, bool is3D)
    {
        if (SFXDict == null || !SFXDict.ContainsKey(key)) return;

        for (int i = 0; i < SFXPlayers.Length; i++)
        {
            int idx = (i + channelIndex) % SFXPlayers.Length;

            // 이미 재생 중이면 다음 채널 확인
            if (SFXPlayers[idx].isPlaying) continue;

            if (SFXDict.TryGetValue(key, out AudioClip clip))
            {
                // 3D 여부에 따른 설정 변경
                if (is3D)
                {
                    SFXPlayers[idx].spatialBlend = spatialBlend; // 3D 설정
                    SFXPlayers[idx].transform.position = position; // 소리 발생 위치로 이동
                    SFXPlayers[idx].minDistance = minDistance;
                    SFXPlayers[idx].maxDistance = maxDistance;
                }
                else
                {
                    SFXPlayers[idx].spatialBlend = 0f; // 2D 설정
                }

                SFXPlayers[idx].clip = clip;
                SFXPlayers[idx].Play();

                // 다음 재생을 위해 인덱스 미리 이동
                channelIndex = (idx + 1) % SFXPlayers.Length;
                break;
            }
        }
    }

    public void PlayBGM(string key)
    {
        if (BGMDict != null && BGMDict.TryGetValue(key, out AudioClip clip))
        {
            BGMPlayer.clip = clip;
            BGMPlayer.Play();
        }
    }

    public void EffectBGM(bool isPlay)
    {
        if (BGMEffet != null) BGMEffet.enabled = isPlay;
    }

    public void SetBGMVolume(float volume)
    {
        BGMVolume = volume;
        if (BGMPlayer != null) BGMPlayer.volume = BGMVolume;
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
        foreach (AudioSource sfx in SFXPlayers)
        {
            if (sfx != null) sfx.volume = SFXVolume;
        }
    }
}