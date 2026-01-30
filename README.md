# Unity Mobile File IO

モバイルプラットフォームのネイティブAPIを直接使用したファイルピッカー。

## 対応プラットフォーム

| Platform | API |
|----------|-----|
| iOS | UIDocumentPickerViewController |
| Android | Storage Access Framework (SAF) / Intent.ACTION_OPEN_DOCUMENT |

## 使用方法

```csharp
using DSGarage.MobileFileIO;

// ファイル選択
MobileFilePicker.PickFile((result) =>
{
    if (result.Success)
    {
        Debug.Log($"Selected: {result.FilePath}");
        // result.FilePath は persistentDataPath 内のコピー済みファイル
    }
}, new PickerOptions
{
    Title = "Select 3D Model",
    AllowedExtensions = new[] { "stl", "obj", "fbx" }
});

// 複数ファイル選択
MobileFilePicker.PickMultipleFiles((result) =>
{
    if (result.Success)
    {
        foreach (var path in result.FilePaths)
        {
            Debug.Log($"Selected: {path}");
        }
    }
});
```

## 特徴

- 各プラットフォームのネイティブAPIを直接使用
- 選択ファイルは自動的に `persistentDataPath` にコピー
- Android Content URI の自動解決
- iOS/Android両対応

## 注意事項

### iOS
- Info.plist に `UIFileSharingEnabled` の設定が必要な場合あり
- iCloud Drive アクセスには追加設定が必要

### Android
- Android 10+ では Scoped Storage に対応
- Content URI からの読み取りは SAF 経由で行う
