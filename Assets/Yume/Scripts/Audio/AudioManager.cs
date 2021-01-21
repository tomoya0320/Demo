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
    public async void PlayBgm(string audioClipName) {
      var audio = await ResourceManager.LoadAssetAsync<AudioClip>(audioClipName);
      m_bgmManager.PlayBgm(audio);
    }

    public async void PlayBgm(string audioClipName, float fadeTime) {
      var audio = await ResourceManager.LoadAssetAsync<AudioClip>(audioClipName);
      m_bgmManager.PlayBgm(audio, fadeTime);
    }

    public void PlayBgmBySceneName(string sceneName) {
      switch (sceneName) {
        case StringMap.LOGIN: {
            PlayBgm(StringMap.LOGIN_BGM);
            break;
          }
        case StringMap.HOME: {
            PlayBgm(StringMap.HOME_BGM);
            break;
          }
      }   
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
    #endregion

    #region Sound
    public async void PlaySound(string audioClipName) {
      var audio = await ResourceManager.LoadAssetAsync<AudioClip>(audioClipName);
      m_soundManager.PlaySound(audio);
    }

    public void UpdateSoundVolume() {
      m_soundManager.UpdateVolume();
    }
    #endregion
  }
}