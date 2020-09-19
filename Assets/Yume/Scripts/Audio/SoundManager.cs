using System.Collections.Generic;
using UnityEngine;
using Yume.String;
using Yume.UI;

namespace Yume.Audio {
  public class SoundManager {
    private List<AudioSource> m_audioSources;
    private float m_value;
    private float m_switch;
    private readonly string m_valueKey = SettingType.SOUND.ToString() + StringMap.VALUE;
    private readonly string m_switchKey = SettingType.SOUND.ToString() + StringMap.SWITCH;

    public SoundManager(AudioSource[] audioSources) {
      m_audioSources = new List<AudioSource>(audioSources);
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
      for (int i = 0; i < m_audioSources.Count; i++) {
        m_audioSources[i].volume = m_switch * m_value;
      }
    }

    public void PlaySound(AudioClip audioClip) {
      AudioSource audioSource = m_audioSources.Find(a => !a.isPlaying);
      if(audioSource == null) {
        audioSource = m_audioSources[0];
      }
      audioSource.clip = audioClip;
      audioSource.Play();
    }
  }
}