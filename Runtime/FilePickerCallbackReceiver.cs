using UnityEngine;

namespace DSGarage.MobileFileIO
{
    /// <summary>
    /// ネイティブプラグインからのコールバックを受け取るMonoBehaviour
    /// </summary>
    public class FilePickerCallbackReceiver : MonoBehaviour
    {
        public const string GameObjectName = "MobileFileIOCallback";

        private static FilePickerCallbackReceiver _instance;

        public static void EnsureExists()
        {
            if (_instance == null)
            {
                var go = new GameObject(GameObjectName);
                _instance = go.AddComponent<FilePickerCallbackReceiver>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 単一ファイル選択完了 (iOSから呼び出し)
        /// </summary>
        public void OnFilePicked(string filePath)
        {
            Debug.Log($"[FilePickerCallbackReceiver] OnFilePicked: {filePath}");
            MobileFilePicker.OnFilePickedNative(filePath);
        }

        /// <summary>
        /// 複数ファイル選択完了 (iOSから呼び出し)
        /// パスは | で区切られる
        /// </summary>
        public void OnMultipleFilesPicked(string pathsJoined)
        {
            Debug.Log($"[FilePickerCallbackReceiver] OnMultipleFilesPicked: {pathsJoined}");
            MobileFilePicker.OnMultipleFilesPickedNative(pathsJoined);
        }

        /// <summary>
        /// キャンセル (iOSから呼び出し)
        /// </summary>
        public void OnPickerCancelled(string dummy)
        {
            Debug.Log("[FilePickerCallbackReceiver] OnPickerCancelled");
            MobileFilePicker.OnFilePickedNative(null);
        }

        /// <summary>
        /// エラー (iOSから呼び出し)
        /// </summary>
        public void OnPickerError(string errorMessage)
        {
            Debug.LogError($"[FilePickerCallbackReceiver] OnPickerError: {errorMessage}");
            MobileFilePicker.OnErrorNative(errorMessage);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
