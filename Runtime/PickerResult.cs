using System;

namespace DSGarage.MobileFileIO
{
    /// <summary>
    /// ファイル選択結果
    /// </summary>
    public class PickerResult
    {
        /// <summary>
        /// 選択成功かどうか
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 選択されたファイルパス (persistentDataPath内のコピー)
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 元のファイル名
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// エラーメッセージ (失敗時)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// キャンセルされたかどうか
        /// </summary>
        public bool Cancelled { get; set; }

        public static PickerResult Succeeded(string filePath, string originalFileName)
        {
            return new PickerResult
            {
                Success = true,
                FilePath = filePath,
                OriginalFileName = originalFileName
            };
        }

        public static PickerResult Failed(string errorMessage)
        {
            return new PickerResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public static PickerResult CancelledResult()
        {
            return new PickerResult
            {
                Success = false,
                Cancelled = true
            };
        }
    }

    /// <summary>
    /// 複数ファイル選択結果
    /// </summary>
    public class MultiplePickerResult
    {
        /// <summary>
        /// 選択成功かどうか
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 選択されたファイルパス配列
        /// </summary>
        public string[] FilePaths { get; set; }

        /// <summary>
        /// 元のファイル名配列
        /// </summary>
        public string[] OriginalFileNames { get; set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// キャンセルされたかどうか
        /// </summary>
        public bool Cancelled { get; set; }

        public static MultiplePickerResult Succeeded(string[] filePaths, string[] originalFileNames)
        {
            return new MultiplePickerResult
            {
                Success = true,
                FilePaths = filePaths,
                OriginalFileNames = originalFileNames
            };
        }

        public static MultiplePickerResult Failed(string errorMessage)
        {
            return new MultiplePickerResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public static MultiplePickerResult CancelledResult()
        {
            return new MultiplePickerResult
            {
                Success = false,
                Cancelled = true
            };
        }
    }
}
