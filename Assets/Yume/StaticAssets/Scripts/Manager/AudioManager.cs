using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.UI;

namespace Yume {
  public static class AudioManager {
    private class SoundManager {
      private int m_switch;
      private float m_value;
      private List<AudioSource> m_audioSourceList = new List<AudioSource>();
      public readonly string valueKey = UISettingType.SOUND.GetDescription() + StringMap.VALUE;
      public readonly string switchKey = UISettingType.SOUND.GetDescription() + StringMap.SWITCH;

      public SoundManager(Transform root) {
        for (int i = 1; i <= Constant.MAX_SOUND_COUNT; i++) {
          var gameObject = new GameObject($"Sound_{i}");
          gameObject.transform.parent = root;
          var audioSource = gameObject.AddComponent<AudioSource>();
          audioSource.playOnAwake = false;

          m_audioSourceList.Add(audioSource);
        }

        if (!PlayerPrefs.HasKey(valueKey)) {
          PlayerPrefs.SetFloat(valueKey, 1.0f);
        }
        if (!PlayerPrefs.HasKey(switchKey)) {
          PlayerPrefs.SetInt(switchKey, 1);
        }
        UpdateVolume();
      }

      public void UpdateVolume() {
        m_switch = PlayerPrefs.GetInt(switchKey);
        m_value = PlayerPrefs.GetFloat(valueKey);
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
      private int m_switch;
      private float m_value;
      private Tween m_tween;
      private AudioSource m_audioSource;
      private readonly string m_valueKey = UISettingType.BGM.GetDescription() + StringMap.VALUE;
      private readonly string m_switchKey = UISettingType.BGM.GetDescription() + StringMap.SWITCH;

      public BGMManager(Transform root) {
        GameObject gameObject = new GameObject("BGM");
        gameObject.transform.parent = root;
        m_audioSource = gameObject.AddComponent<AudioSource>();
        m_audioSource.loop = true;

        if (!PlayerPrefs.HasKey(m_valueKey)) {
          PlayerPrefs.SetFloat(m_valueKey, 1.0f);
        }
        if (!PlayerPrefs.HasKey(m_switchKey)) {
          PlayerPrefs.SetInt(m_switchKey, 1);
        }
        UpdateVolume();
      }

      public void UpdateVolume() {
        m_switch = PlayerPrefs.GetInt(m_switchKey);
        m_value = PlayerPrefs.GetFloat(m_valueKey);
        m_audioSource.volume = m_switch * m_value;
      }

      public void StopBgm(Action onCompleted = null, float fadeTime = 1.0f) {
        // 因为OnComplete里可能又会创建一个Tween
        while (m_tween != null) {
          m_tween.Complete();
        }
        
        if (fadeTime <= 0) {
          _StopBgmInternal();
          onCompleted?.Invoke();
        } else {
          m_tween = m_audioSource
                    .DOFade(0, fadeTime)
                    .OnComplete(
                      () => {
                        m_tween = null;
                        _StopBgmInternal();
                        onCompleted?.Invoke();
                      }
                    );  
        }
      }

      public void PlayBgm(AudioClip audioClip, float fadeTime = 1.0f) {
        // 因为OnComplete里可能又会创建一个Tween
        while (m_tween != null) {
          m_tween.Complete();
        }
        
        if (fadeTime <= 0) {
          _PlayBgmInternal(audioClip);
        } else {
          StopBgm(
            () => {
              _PlayBgmInternal(audioClip);
              m_tween = m_audioSource
                        .DOFade(m_switch * m_value, fadeTime)
                        .OnComplete(() => m_tween = null);
            }
          );
        }
      }

      private void _PlayBgmInternal(AudioClip audioClip) {
        m_audioSource.clip = audioClip;
        m_audioSource.Play();
      }

      private void _StopBgmInternal() {
        m_audioSource.Stop();
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
      PlayBgm(StringMap.DEFAULT_BGM, 0);
    }

    #region BGM

    public static void PlayBgm(AudioClip audioClip, float fadeTime = 1.0f) {
      s_bgmManager.PlayBgm(audioClip, fadeTime);
    }

    public static void PlayBgm(string audioClipName, float fadeTime = 1.0f) {
      var audio = ResourceManager.Load<AudioClip>(audioClipName);
      s_bgmManager.PlayBgm(audio, fadeTime);
    }

    public static void StopBgm(Action onCompleted = null, float fadeTime = 1.0f) {
      s_bgmManager.StopBgm(onCompleted, fadeTime);
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