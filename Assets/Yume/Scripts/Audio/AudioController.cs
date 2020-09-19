using UnityEngine;
using UnityEngine.AddressableAssets;
using Yume.Singleton;

namespace Yume.Audio {
  public class AudioController : SingletonMono<AudioController> {
    [SerializeField]
    private AudioSource _bgmAudioSource;    
    [SerializeField]
    private AudioSource[] _soundAudioSources;
    private BGMManager m_bgmManager;
    private SoundManager m_soundManager;

    protected override void OnInit() {
      base.OnInit();
      m_bgmManager = new BGMManager(_bgmAudioSource);
      m_soundManager = new SoundManager(_soundAudioSources);
    }

    #region BGM
    public async void PlayBgm(string audioClipName) {
      AudioClip audioClip = await Addressables.LoadAssetAsync<AudioClip>(audioClipName).Task;
      m_bgmManager.PlayBgm(audioClip);
    }

    public async void PlayBgm(string audioClipName, float fadeTime) {
      AudioClip audioClip = await Addressables.LoadAssetAsync<AudioClip>(audioClipName).Task;
      m_bgmManager.PlayBgm(audioClip, fadeTime);
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
      AudioClip audioClip = await Addressables.LoadAssetAsync<AudioClip>(audioClipName).Task;
      m_soundManager.PlaySound(audioClip);
    }

    public void UpdateSoundVolume() {
      m_soundManager.UpdateVolume();
    }
    #endregion
  }
}