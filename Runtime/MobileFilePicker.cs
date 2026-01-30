using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace DSGarage.MobileFileIO
{
    /// <summary>
    /// モバイルファイルピッカー
    /// iOS: UIDocumentPickerViewController
    /// Android: Storage Access Framework (Intent.ACTION_OPEN_DOCUMENT)
    /// </summary>
    public static class MobileFilePicker
    {
        private static Action<PickerResult> _singleCallback;
        private static Action<MultiplePickerResult> _multipleCallback;
        private static PickerOptions _currentOptions;
        private static bool _isPickerActive;

        /// <summary>
        /// ピッカーがアクティブかどうか
        /// </summary>
        public static bool IsActive => _isPickerActive;

        /// <summary>
        /// 単一ファイルを選択
        /// </summary>
        public static void PickFile(Action<PickerResult> callback, PickerOptions options = null)
        {
            if (_isPickerActive)
            {
                callback?.Invoke(PickerResult.Failed("Picker is already active"));
                return;
            }

            _singleCallback = callback;
            _currentOptions = options ?? PickerOptions.Default;
            _isPickerActive = true;

#if UNITY_EDITOR
            PickFileEditor();
#elif UNITY_IOS
            PickFileiOS();
#elif UNITY_ANDROID
            PickFileAndroid();
#else
            _isPickerActive = false;
            callback?.Invoke(PickerResult.Failed("Platform not supported"));
#endif
        }

        /// <summary>
        /// 複数ファイルを選択
        /// </summary>
        public static void PickMultipleFiles(Action<MultiplePickerResult> callback, PickerOptions options = null)
        {
            if (_isPickerActive)
            {
                callback?.Invoke(MultiplePickerResult.Failed("Picker is already active"));
                return;
            }

            _multipleCallback = callback;
            _currentOptions = options ?? PickerOptions.Default;
            _isPickerActive = true;

#if UNITY_EDITOR
            PickMultipleFilesEditor();
#elif UNITY_IOS
            PickMultipleFilesiOS();
#elif UNITY_ANDROID
            PickMultipleFilesAndroid();
#else
            _isPickerActive = false;
            callback?.Invoke(MultiplePickerResult.Failed("Platform not supported"));
#endif
        }

        /// <summary>
        /// 非同期で単一ファイルを選択
        /// </summary>
        public static Task<PickerResult> PickFileAsync(PickerOptions options = null)
        {
            var tcs = new TaskCompletionSource<PickerResult>();
            PickFile(result => tcs.TrySetResult(result), options);
            return tcs.Task;
        }

        /// <summary>
        /// 非同期で複数ファイルを選択
        /// </summary>
        public static Task<MultiplePickerResult> PickMultipleFilesAsync(PickerOptions options = null)
        {
            var tcs = new TaskCompletionSource<MultiplePickerResult>();
            PickMultipleFiles(result => tcs.TrySetResult(result), options);
            return tcs.Task;
        }

        #region Editor Implementation

#if UNITY_EDITOR
        private static void PickFileEditor()
        {
            string extensions = GetEditorExtensions();
            string path = UnityEditor.EditorUtility.OpenFilePanel(
                _currentOptions.Title,
                "",
                extensions);

            _isPickerActive = false;

            if (string.IsNullOrEmpty(path))
            {
                _singleCallback?.Invoke(PickerResult.CancelledResult());
            }
            else
            {
                string importedPath = ImportFile(path);
                if (importedPath != null)
                {
                    _singleCallback?.Invoke(PickerResult.Succeeded(importedPath, Path.GetFileName(path)));
                }
                else
                {
                    _singleCallback?.Invoke(PickerResult.Failed("Failed to import file"));
                }
            }
            _singleCallback = null;
        }

        private static void PickMultipleFilesEditor()
        {
            // Unity Editor doesn't support multiple file selection natively
            // Fall back to single file
            string extensions = GetEditorExtensions();
            string path = UnityEditor.EditorUtility.OpenFilePanel(
                _currentOptions.Title,
                "",
                extensions);

            _isPickerActive = false;

            if (string.IsNullOrEmpty(path))
            {
                _multipleCallback?.Invoke(MultiplePickerResult.CancelledResult());
            }
            else
            {
                string importedPath = ImportFile(path);
                if (importedPath != null)
                {
                    _multipleCallback?.Invoke(MultiplePickerResult.Succeeded(
                        new[] { importedPath },
                        new[] { Path.GetFileName(path) }));
                }
                else
                {
                    _multipleCallback?.Invoke(MultiplePickerResult.Failed("Failed to import file"));
                }
            }
            _multipleCallback = null;
        }

        private static string GetEditorExtensions()
        {
            if (_currentOptions.AllowedExtensions == null || _currentOptions.AllowedExtensions.Length == 0)
                return "";

            return string.Join(",", _currentOptions.AllowedExtensions);
        }
#endif

        #endregion

        #region iOS Implementation

#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _MobileFileIO_PickFile(string[] utis, int utiCount, string callbackObject);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _MobileFileIO_PickMultipleFiles(string[] utis, int utiCount, string callbackObject);

        private static void PickFileiOS()
        {
            EnsureCallbackReceiver();
            string[] utis = GetIOSUTIs();
            _MobileFileIO_PickFile(utis, utis.Length, FilePickerCallbackReceiver.GameObjectName);
        }

        private static void PickMultipleFilesiOS()
        {
            EnsureCallbackReceiver();
            string[] utis = GetIOSUTIs();
            _MobileFileIO_PickMultipleFiles(utis, utis.Length, FilePickerCallbackReceiver.GameObjectName);
        }

        private static string[] GetIOSUTIs()
        {
            if (_currentOptions.AllowedExtensions == null || _currentOptions.AllowedExtensions.Length == 0)
                return new[] { "public.data", "public.content" };

            // 拡張子からUTIへの変換
            var utis = new System.Collections.Generic.List<string>();
            foreach (var ext in _currentOptions.AllowedExtensions)
            {
                string uti = ExtensionToUTI(ext.ToLowerInvariant());
                if (!string.IsNullOrEmpty(uti) && !utis.Contains(uti))
                    utis.Add(uti);
            }

            if (utis.Count == 0)
                return new[] { "public.data" };

            return utis.ToArray();
        }

        private static string ExtensionToUTI(string ext)
        {
            ext = ext.TrimStart('.');
            return ext switch
            {
                "stl" => "public.standard-tesselated-geometry-format",
                "obj" => "public.geometry-definition-format",
                "fbx" => "com.autodesk.fbx",
                "gltf" => "org.khronos.gltf",
                "glb" => "org.khronos.glb",
                "png" => "public.png",
                "jpg" or "jpeg" => "public.jpeg",
                "gif" => "com.compuserve.gif",
                "bmp" => "com.microsoft.bmp",
                "pdf" => "com.adobe.pdf",
                "txt" => "public.plain-text",
                "json" => "public.json",
                "xml" => "public.xml",
                "zip" => "public.zip-archive",
                _ => "public.data"
            };
        }
#endif

        #endregion

        #region Android Implementation

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void PickFileAndroid()
        {
            EnsureCallbackReceiver();
            AndroidFilePicker.PickFile(GetAndroidMimeTypes(), false);
        }

        private static void PickMultipleFilesAndroid()
        {
            EnsureCallbackReceiver();
            AndroidFilePicker.PickFile(GetAndroidMimeTypes(), true);
        }

        private static string[] GetAndroidMimeTypes()
        {
            if (_currentOptions.AllowedExtensions == null || _currentOptions.AllowedExtensions.Length == 0)
                return new[] { "*/*" };

            var mimeTypes = new System.Collections.Generic.List<string>();
            foreach (var ext in _currentOptions.AllowedExtensions)
            {
                string mime = ExtensionToMimeType(ext.ToLowerInvariant());
                if (!string.IsNullOrEmpty(mime) && !mimeTypes.Contains(mime))
                    mimeTypes.Add(mime);
            }

            if (mimeTypes.Count == 0)
                return new[] { "*/*" };

            return mimeTypes.ToArray();
        }

        private static string ExtensionToMimeType(string ext)
        {
            ext = ext.TrimStart('.');
            return ext switch
            {
                "stl" => "application/sla",
                "obj" => "text/plain",
                "fbx" => "application/octet-stream",
                "gltf" => "model/gltf+json",
                "glb" => "model/gltf-binary",
                "png" => "image/png",
                "jpg" or "jpeg" => "image/jpeg",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                "pdf" => "application/pdf",
                "txt" => "text/plain",
                "json" => "application/json",
                "xml" => "application/xml",
                "zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
#endif

        #endregion

        #region Callback Handling

        private static void EnsureCallbackReceiver()
        {
            FilePickerCallbackReceiver.EnsureExists();
        }

        /// <summary>
        /// ネイティブからのコールバック (単一ファイル)
        /// </summary>
        internal static void OnFilePickedNative(string filePath)
        {
            _isPickerActive = false;

            if (string.IsNullOrEmpty(filePath))
            {
                _singleCallback?.Invoke(PickerResult.CancelledResult());
            }
            else
            {
                string importedPath = ImportFile(filePath);
                if (importedPath != null)
                {
                    _singleCallback?.Invoke(PickerResult.Succeeded(importedPath, Path.GetFileName(filePath)));
                }
                else
                {
                    _singleCallback?.Invoke(PickerResult.Failed("Failed to import file"));
                }
            }
            _singleCallback = null;
        }

        /// <summary>
        /// ネイティブからのコールバック (複数ファイル)
        /// </summary>
        internal static void OnMultipleFilesPickedNative(string pathsJoined)
        {
            _isPickerActive = false;

            if (string.IsNullOrEmpty(pathsJoined))
            {
                _multipleCallback?.Invoke(MultiplePickerResult.CancelledResult());
            }
            else
            {
                string[] paths = pathsJoined.Split('|');
                var importedPaths = new System.Collections.Generic.List<string>();
                var originalNames = new System.Collections.Generic.List<string>();

                foreach (var path in paths)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        string imported = ImportFile(path);
                        if (imported != null)
                        {
                            importedPaths.Add(imported);
                            originalNames.Add(Path.GetFileName(path));
                        }
                    }
                }

                if (importedPaths.Count > 0)
                {
                    _multipleCallback?.Invoke(MultiplePickerResult.Succeeded(
                        importedPaths.ToArray(),
                        originalNames.ToArray()));
                }
                else
                {
                    _multipleCallback?.Invoke(MultiplePickerResult.Failed("Failed to import files"));
                }
            }
            _multipleCallback = null;
        }

        /// <summary>
        /// エラーコールバック
        /// </summary>
        internal static void OnErrorNative(string errorMessage)
        {
            _isPickerActive = false;

            if (_singleCallback != null)
            {
                _singleCallback.Invoke(PickerResult.Failed(errorMessage));
                _singleCallback = null;
            }

            if (_multipleCallback != null)
            {
                _multipleCallback.Invoke(MultiplePickerResult.Failed(errorMessage));
                _multipleCallback = null;
            }
        }

        #endregion

        #region File Import

        private static string ImportFile(string sourcePath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    Debug.LogWarning($"[MobileFilePicker] Source file not found: {sourcePath}");
                    return null;
                }

                string destDir = Path.Combine(Application.persistentDataPath, _currentOptions.DestinationFolder);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                string fileName = Path.GetFileName(sourcePath);
                string destPath = Path.Combine(destDir, fileName);

                if (!_currentOptions.OverwriteExisting && File.Exists(destPath))
                {
                    destPath = GetUniqueFilePath(destDir, fileName);
                }

                File.Copy(sourcePath, destPath, _currentOptions.OverwriteExisting);
                Debug.Log($"[MobileFilePicker] Imported: {destPath}");

                return destPath;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MobileFilePicker] Import error: {e.Message}");
                return null;
            }
        }

        private static string GetUniqueFilePath(string directory, string fileName)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            int counter = 1;

            string newPath;
            do
            {
                newPath = Path.Combine(directory, $"{nameWithoutExt}_{counter}{ext}");
                counter++;
            } while (File.Exists(newPath));

            return newPath;
        }

        #endregion
    }
}
