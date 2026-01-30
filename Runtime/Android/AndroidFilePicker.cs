#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;

namespace DSGarage.MobileFileIO
{
    /// <summary>
    /// Android Storage Access Framework を使用したファイルピッカー
    /// MobileFileIOActivity を起動してファイル選択を行う
    /// </summary>
    public static class AndroidFilePicker
    {
        private static AndroidJavaObject _currentActivity;

        private static AndroidJavaObject CurrentActivity
        {
            get
            {
                if (_currentActivity == null)
                {
                    using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                }
                return _currentActivity;
            }
        }

        /// <summary>
        /// ファイルピッカーを開く
        /// </summary>
        public static void PickFile(string[] mimeTypes, bool allowMultiple)
        {
            try
            {
                // MobileFileIOActivity を起動
                using var activityClass = new AndroidJavaClass("com.dsgarage.mobilefileio.MobileFileIOActivity");
                using var intent = new AndroidJavaObject("android.content.Intent", CurrentActivity, activityClass);

                // パラメータを設定
                intent.Call<AndroidJavaObject>("putExtra", "callbackObject", FilePickerCallbackReceiver.GameObjectName);
                intent.Call<AndroidJavaObject>("putExtra", "allowMultiple", allowMultiple);
                intent.Call<AndroidJavaObject>("putExtra", "mimeTypes", mimeTypes);

                // Activity を起動
                CurrentActivity.Call("startActivity", intent);

                Debug.Log("[AndroidFilePicker] Started MobileFileIOActivity");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AndroidFilePicker] Error: {e.Message}");
                MobileFilePicker.OnErrorNative(e.Message);
            }
        }
    }
}
#endif
