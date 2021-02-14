using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Resource;
using Yume.UI;

namespace Yume.Audio {
  public static class AudioManager {
    private class SoundManager {
      private float m_value;
      private float m_switch;
      private List<AudioSource> m_audioSourceList = new List<AudioSource>();
      public string valueKey => UISettingType.SOUND.GetDescription() + StringMap.VALUE;
      public string switchKey => UISettingType.SOUND.GetDescription() + StringMap.SWITCH;

      public SoundManager(Transform root) {
        for (int i = 1; i <= Constant.MAX_SOUND_COUNT; i++) {
          var gameObject = new GameObject($"Sound_{i}");
          gameObject.transform.parent = root;
          var audioSource = gameObject.AddComponent<AudioSource>();
          audioSource.playOnAwake = false;

          m_audioSourceList.Add(audioSource);
        }

        if (!PlayerPrefs.HasKey(valueKey)) {
          PlayerPrefs.SetInt(valueKey, 100);
        }
        if (!PlayerPrefs.HasKey(switchKey)) {
          PlayerPrefs.SetInt(switchKey, 1);
        }
        UpdateVolume();
      }

      public void UpdateVolume() {
        m_switch = PlayerPrefs.GetInt(switchKey);
        m_value = 0.01f * PlayerPrefs.GetInt(valueKey);
        foreach (AudioSource audioSource in m_audioSourceList) {
          audioSource.volume = m_switch * m_value;
        }
      }

      public void PlaySound(AudioClip audioClip) {
        AudioSource audioSource = m_audioSourceList.Find(a => !a.isPlaying);
        if (audioSource == null) {
          audioSource = m_audioSourceList.First();
          audioSource.Stop();
        }
        audioSource.clip = audioClip;
        audioSource.Play();
      }
    }

    private class BGMManager {
      private Tween m_tween;
      private float m_value;
      private float m_switch;
      private AudioSource m_audioSource;
      private readonly string m_valueKey = UISettingType.BGM.GetDescription() + StringMap.VALUE;
      private readonly string m_switchKey = UISettingType.BGM.GetDescription() + StringMap.SWITCH;

      public BGMManager(Transform root) {
        GameObject gameObject = new GameObject("BGM");
        gameObject.transform.parent = root;
        m_audioSource = gameObject.AddComponent<AudioSource>();
        m_audioSource.loop = true;

        if (!PlayerPrefs.HasKey(m_valueKey)) {
          PlayerPrefs.SetInt(m_valueKey, 100);
        }
        if (!PlayerPrefs.HasKey(m_switchKey)) {
          PlayerPrefs.SetInt(m_switchKey, 1);
        }
        UpdateVolume();
      }

      public void UpdateVolume() {
        m_switch = PlayerPrefs.GetInt(m_switchKey);
        m_value = 0.01f * PlayerPrefs.GetInt(m_valueKey);
        m_audioSource.volume = m_switch * m_value;
      }

      public void StopBgm(Action onCompleted = null, float fadeTime = 1.0f) {
        m_tween?.Complete();
        m_tween = m_audioSource
                  .DOFade(0, fadeTime)
                  .OnComplete(() => {
                    m_tween = null;
                    m_audioSource.Stop();
                    onCompleted?.Invoke();
                  });
      }

      public void PlayBgm(AudioClip audioClip, bool immediate = false, float fadeTime = 1.0f) {
        if (immediate) {
          _PlayBgmInternal(audioClip);
        } else {
          StopBgm(() => {
            _PlayBgmInternal(audioClip);
            m_tween = m_audioSource
                        .DOFade(m_switch * m_value, fadeTime)
                        .OnComplete(() => m_tween = null);
          });
        }
      }

      private void _PlayBgmInternal(AudioClip audioClip) {
        m_audioSource.clip = audioClip;
        m_audioSource.Play();
      }

      public string GetCurrentClipName() {
        return m_audioSource.clip?.name;
      }
    }

    private static BGMManager s_bgmManager;
    private static SoundManager s_soundManager;

    public static void Init() {
      var audioRoot = GameObject.Find("AudioRoot").transform;
      s_bgmManager = new BGMManager(audioRoot);
      s_soundManager = new SoundManager(audioRoot);

      // 播放默认的bgm
      PlayBgm(StringMap.DEFAULT_BGM, true);
    }

    #region BGM
    public static void PlayBgm(AudioClip audioClip) {
      s_bgmManager.PlayBgm(audioClip);
    }   
    
    public static void PlayBgm(string audioClipName) {
      var audio = ResourceManager.Load<AudioClip>(audioClipName);
      s_bgmManager.PlayBgm(audio);
    }

    public static void PlayBgm(string audioClipName, bool immediate) {
      var audio = ResourceManager.Load<AudioClip>(audioClipName);
      s_bgmManager.PlayBgm(audio, immediate);
    }

    public static void StopBgm() {
      s_bgmManager.StopBgm();
    }

    public static void StopBgm(float fadeTime) {
      s_bgmManager.StopBgm(null, fadeTime);
    }

    public static void UpdateBgmVolume() {
      s_bgmManager.UpdateVolume();
    }

    public static string GetCurrentBgmName() {
      return s_bgmManager.GetCurrentClipName();
    }
    #endregion

    #region Sound
    public static void PlaySound(string audioClipName) {
      var audio = ResourceManager.Load<AudioClip>(audioClipName);
      s_soundManager.PlaySound(audio);
    }    
    
    public static void PlaySound(AudioClip audioClip) {
      s_soundManager.PlaySound(audioClip);
    }

    public static void UpdateSoundVolume() {
      s_soundManager.UpdateVolume();
    }
    #endregion
  }
}