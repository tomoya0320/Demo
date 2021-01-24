using DG.Tweening;
using System;
using UnityEngine;
using Yume.UI;

namespace Yume.Audio {
  public class BGMManager {
    private Tween m_tween;
    private float m_value;
    private float m_switch;
    private AudioSource m_audioSource;
    private readonly string m_valueKey = UISettingType.BGM.GetDescription() + StringMap.VALUE;
    private readonly string m_switchKey = UISettingType.BGM.GetDescription() + StringMap.SWITCH;

    public BGMManager(AudioSource audioSource) {
      m_audioSource = audioSource;
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
      if (m_tween != null) {
        m_tween.Complete();
      }
      m_tween = m_audioSource
                  .DOFade(0, fadeTime)
                  .OnComplete(() => {
                    m_tween = null;
                    m_audioSource.Stop();
                    onCompleted?.Invoke();
                  });
    }

    public void PlayBgm(AudioClip audioClip, float fadeTime = 1.0f) {
      StopBgm(() => {
        m_audioSource.clip = audioClip;
        m_audioSource.Play();
        m_tween = m_audioSource
                    .DOFade(m_switch * m_value, fadeTime)
                    .OnComplete(() => m_tween = null);
      });
    }
  }
}