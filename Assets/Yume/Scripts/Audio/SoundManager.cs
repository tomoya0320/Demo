using System.Collections.Generic;
using UnityEngine;
using Yume.String;
using Yume.UI;

namespace Yume.Audio {
  public class SoundManager {
    private float m_value;
    private float m_switch;
    private Transform m_soundFolder;
    private List<AudioSource> m_audioSourceList = new List<AudioSource>();
    private readonly string valueKey = UISettingType.SOUND.GetDescription() + StringMap.VALUE;
    private readonly string switchKey = UISettingType.SOUND.GetDescription() + StringMap.SWITCH;

    public SoundManager(Transform soundFolder) {
      m_soundFolder = soundFolder;
      for (int i = 0; i < m_soundFolder.childCount; i++) {
        m_audioSourceList.Add(m_soundFolder.GetChild(i).GetComponent<AudioSource>());
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
      for (int i = 0; i < m_audioSourceList.Count; i++) {
        m_audioSourceList[i].volume = m_switch * m_value;
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
}