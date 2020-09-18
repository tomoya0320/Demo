using UnityEngine;
using UnityEngine.AddressableAssets;
using Yume.Singleton;

namespace Yume.Audio {
  public class AudioController : SingletonMono<AudioController> {
    [SerializeField]
    private AudioSource _bgmAudioSource;
    private BGMManager m_bgmManager;

    protected override void OnInit() {
      base.OnInit();
      m_bgmManager = new BGMManager(_bgmAudioSource);
    }

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
  }
}