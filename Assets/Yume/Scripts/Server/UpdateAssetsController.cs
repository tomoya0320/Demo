using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Yume.Singleton;
using Yume.String;
using Yume.UI;

namespace Yume.Server {
  public class UpdateAssetsController : SingletonMono<UpdateAssetsController> {
    [SerializeField]
    private GameObject _downloadError;
    [SerializeField]
    private Text _downloadText;
    [SerializeField]
    private Image _downloadProcess;
    [SerializeField]
    private UIFade _startButton;

    private GameObject downloadProcessParent {
      get {
        return _downloadProcess.transform.parent.gameObject;
      }
    }

    private AsyncOperationHandle m_downloadHandle;
    private AsyncOperationHandle<long> m_checkHandle;

    protected override void OnInit() {
      dontDestroyOnLoad = false;
      base.OnInit();
    }

    public async void CheckGameAssets() {
      _downloadError.SetActive(false);
      _downloadText.gameObject.SetActive(true);

      await Addressables.InitializeAsync().Task;

      if (Application.internetReachability == NetworkReachability.NotReachable) {
        _downloadError.SetActive(true);
      } else {
        m_checkHandle = Addressables.GetDownloadSizeAsync(StringMap.PRELOAD);
        await m_checkHandle.Task;
        if (m_checkHandle.Status != AsyncOperationStatus.Succeeded) {
          _downloadError.SetActive(true);
          return;
        }
        if (m_checkHandle.Result > 0) {
          _DownloadAssets(m_checkHandle.Result / 1024.0f / 1024.0f);
        } else {
          _downloadText.gameObject.SetActive(false);
          _startButton.Open();
        }
      }
    }

    private async void _DownloadAssets(float totalSize) {
      downloadProcessParent.SetActive(true);
      m_downloadHandle = Addressables.DownloadDependenciesAsync(StringMap.PRELOAD);

      while (!m_downloadHandle.IsDone) {
        _downloadText.text = string.Format(StringMap.ASSETS_DOWNLOADING,
        (m_downloadHandle.PercentComplete * 100.0f).ToString("0.00"), totalSize.ToString("0.00"));
        _downloadProcess.fillAmount = m_downloadHandle.PercentComplete;
        await System.Threading.Tasks.Task.Delay((int)(Time.deltaTime * 1000.0f));
      }
      if (m_downloadHandle.Status != AsyncOperationStatus.Succeeded) {
        _downloadError.SetActive(true);
        return;
      }
      _downloadText.gameObject.SetActive(false);
      downloadProcessParent.SetActive(false);
      _startButton.Open();
    }

    public void QuitGame() {
      Application.Quit();
    }
  }
}