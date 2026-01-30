namespace DSGarage.MobileFileIO
{
    /// <summary>
    /// ファイルピッカーのオプション
    /// </summary>
    public class PickerOptions
    {
        /// <summary>
        /// ピッカーのタイトル
        /// </summary>
        public string Title { get; set; } = "Select File";

        /// <summary>
        /// 許可する拡張子 (例: "stl", "obj", "fbx")
        /// nullまたは空の場合は全ファイル
        /// </summary>
        public string[] AllowedExtensions { get; set; }

        /// <summary>
        /// コピー先のサブフォルダ名 (persistentDataPath内)
        /// </summary>
        public string DestinationFolder { get; set; } = "ImportedFiles";

        /// <summary>
        /// 既存ファイルを上書きするか
        /// </summary>
        public bool OverwriteExisting { get; set; } = false;

        /// <summary>
        /// デフォルトオプション
        /// </summary>
        public static PickerOptions Default => new PickerOptions();

        /// <summary>
        /// 3Dモデル用オプション
        /// </summary>
        public static PickerOptions For3DModels => new PickerOptions
        {
            Title = "Select 3D Model",
            AllowedExtensions = new[] { "stl", "obj", "fbx", "gltf", "glb" },
            DestinationFolder = "Models"
        };

        /// <summary>
        /// 画像用オプション
        /// </summary>
        public static PickerOptions ForImages => new PickerOptions
        {
            Title = "Select Image",
            AllowedExtensions = new[] { "png", "jpg", "jpeg", "gif", "bmp" },
            DestinationFolder = "Images"
        };
    }
}
