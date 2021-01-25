using UnityEngine;
using Yume.Resource;

namespace Yume.Audio {
  public class AudioManager : SingletonMono<AudioManager> {
    [SerializeField]
    private AudioSource _bgmAudioSource;
    [SerializeField]
    private Transform _soundFolder;
    private BGMManager m_bgmManager;
    private SoundManager m_soundManager;

    protected override void OnInit() {
      m_bgmManager = new BGMManager(_bgmAudioSource);
      m_soundManager = new SoundManager(_soundFolder);
    }

    #region BGM
    public void PlayBgm(string audioClipName) {
      var audio = ResourceManager.Load<AudioClip>(audioClipName);
      m_bgmManager.PlayBgm(audio);
    }

    public void PlayBgm(string audioClipName, bool force, float fadeTime) {
      var audio = ResourceManager.Load<AudioClip>(audioClipName);
      m_bgmManager.PlayBgm(audio, force, fadeTime);
    }

    public void StopBgm() {
      m_bgmManager.StopBgm();
    }

    public void StopBgm(float fadeTime) {
      m_bgmManager.StopBgm(null, fadeTime);
    }

    public void UpdateBgmVolume() {
      m_bgmManager.UpdateVolume();
    }

    public string GetCurrentBgmName() {
      return m_bgmManager.GetCurrentClipName();
    }
    #endregion

    #region Sound
    public void PlaySound(string audioClipName) {
      var audio = ResourceManager.Load<AudioClip>(audioClipName);
      m_soundManager.PlaySound(audio);
    }

    public void UpdateSoundVolume() {
      m_soundManager.UpdateVolume();
    }
    #endregion
  }
}