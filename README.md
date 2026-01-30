# Unity Mobile File IO

Unity用モバイルファイルピッカー。各プラットフォームのネイティブAPIを直接使用してファイル選択を行います。

[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)

## 特徴

- **ネイティブAPI直接使用** - サードパーティライブラリに依存しない
- **自動ファイルコピー** - 選択ファイルを `persistentDataPath` に自動コピー
- **Android Scoped Storage対応** - Android 10以降のストレージ制限に対応
- **iOS Security Scoped Resource対応** - iCloud等の外部ストレージに対応
- **async/await対応** - 非同期プログラミングをサポート

## 対応プラットフォーム

| Platform | API | 最小バージョン |
|----------|-----|---------------|
| iOS | `UIDocumentPickerViewController` | iOS 11.0+ |
| Android | Storage Access Framework (SAF) | API 19+ |
| Editor | `EditorUtility.OpenFilePanel` | - |

## インストール

### Unity Package Manager (推奨)

1. Window > Package Manager を開く
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力:

```
https://github.com/dsgarage/UnityMobileFileIO.git
```

### manifest.json

`Packages/manifest.json` に直接追加:

```json
{
  "dependencies": {
    "com.dsgarage.unitymobilefileio": "https://github.com/dsgarage/UnityMobileFileIO.git"
  }
}
```

### 特定バージョンを指定

```json
"com.dsgarage.unitymobilefileio": "https://github.com/dsgarage/UnityMobileFileIO.git#v1.0.0"
```

## 使用方法

### 基本的な使い方

```csharp
using DSGarage.MobileFileIO;

// ファイル選択（コールバック）
MobileFilePicker.PickFile((result) =>
{
    if (result.Success)
    {
        Debug.Log($"ファイルパス: {result.FilePath}");
        Debug.Log($"元のファイル名: {result.OriginalFileName}");

        // result.FilePath は persistentDataPath 内のコピー済みファイル
        // そのまま File.ReadAllBytes() 等で読み込み可能
    }
    else if (result.Cancelled)
    {
        Debug.Log("キャンセルされました");
    }
    else
    {
        Debug.LogError($"エラー: {result.ErrorMessage}");
    }
});
```

### 非同期 (async/await)

```csharp
using DSGarage.MobileFileIO;

public async void LoadFileAsync()
{
    try
    {
        var result = await MobileFilePicker.PickFileAsync();

        if (result.Success)
        {
            byte[] data = File.ReadAllBytes(result.FilePath);
            // ファイルを処理...
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"エラー: {e.Message}");
    }
}
```

### オプション指定

```csharp
using DSGarage.MobileFileIO;

// カスタムオプション
var options = new PickerOptions
{
    Title = "3Dモデルを選択",
    AllowedExtensions = new[] { "stl", "obj", "fbx" },
    DestinationFolder = "Models",      // persistentDataPath内のサブフォルダ
    OverwriteExisting = false          // 既存ファイルを上書きしない
};

MobileFilePicker.PickFile((result) =>
{
    if (result.Success)
    {
        // result.FilePath = persistentDataPath/Models/filename.stl
    }
}, options);
```

### プリセットオプション

```csharp
// 3Dモデル用
MobileFilePicker.PickFile(callback, PickerOptions.For3DModels);

// 画像用
MobileFilePicker.PickFile(callback, PickerOptions.ForImages);

// デフォルト（全ファイル）
MobileFilePicker.PickFile(callback, PickerOptions.Default);
```

### 複数ファイル選択

```csharp
MobileFilePicker.PickMultipleFiles((result) =>
{
    if (result.Success)
    {
        foreach (var path in result.FilePaths)
        {
            Debug.Log($"選択: {path}");
        }
    }
}, PickerOptions.ForImages);

// 非同期版
var result = await MobileFilePicker.PickMultipleFilesAsync();
```

## API リファレンス

### MobileFilePicker

| メソッド | 説明 |
|---------|------|
| `PickFile(callback, options)` | 単一ファイルを選択 |
| `PickMultipleFiles(callback, options)` | 複数ファイルを選択 |
| `PickFileAsync(options)` | 単一ファイルを選択（非同期） |
| `PickMultipleFilesAsync(options)` | 複数ファイルを選択（非同期） |
| `IsActive` | ピッカーがアクティブかどうか |

### PickerOptions

| プロパティ | 型 | デフォルト | 説明 |
|-----------|-----|-----------|------|
| `Title` | string | "Select File" | ピッカーのタイトル |
| `AllowedExtensions` | string[] | null | 許可する拡張子（nullで全ファイル） |
| `DestinationFolder` | string | "ImportedFiles" | コピー先サブフォルダ |
| `OverwriteExisting` | bool | false | 既存ファイルを上書きするか |

### PickerResult

| プロパティ | 型 | 説明 |
|-----------|-----|------|
| `Success` | bool | 選択成功かどうか |
| `FilePath` | string | コピー後のファイルパス |
| `OriginalFileName` | string | 元のファイル名 |
| `ErrorMessage` | string | エラーメッセージ（失敗時） |
| `Cancelled` | bool | キャンセルされたかどうか |

### MultiplePickerResult

| プロパティ | 型 | 説明 |
|-----------|-----|------|
| `Success` | bool | 選択成功かどうか |
| `FilePaths` | string[] | コピー後のファイルパス配列 |
| `OriginalFileNames` | string[] | 元のファイル名配列 |
| `ErrorMessage` | string | エラーメッセージ（失敗時） |
| `Cancelled` | bool | キャンセルされたかどうか |

## ファイルタイプ対応表

### 3Dモデル

| 拡張子 | iOS UTI | Android MIME |
|--------|---------|--------------|
| .stl | public.standard-tesselated-geometry-format | application/sla |
| .obj | public.geometry-definition-format | text/plain |
| .fbx | com.autodesk.fbx | application/octet-stream |
| .gltf | org.khronos.gltf | model/gltf+json |
| .glb | org.khronos.glb | model/gltf-binary |

### 画像

| 拡張子 | iOS UTI | Android MIME |
|--------|---------|--------------|
| .png | public.png | image/png |
| .jpg/.jpeg | public.jpeg | image/jpeg |
| .gif | com.compuserve.gif | image/gif |
| .bmp | com.microsoft.bmp | image/bmp |

## プラットフォーム別実装詳細

### iOS

```
MobileFileIOBridge.mm
├── UIDocumentPickerViewController を使用
├── iOS 14+: UTType API
├── iOS 11-13: 従来のdocumentTypes API
└── Security Scoped Resource に対応
```

**処理フロー:**
1. `UIDocumentPickerViewController` を表示
2. ユーザーがファイルを選択
3. Security Scoped Resource としてアクセス権を取得
4. 一時ディレクトリにファイルをコピー
5. Unity にパスをコールバック

### Android

```
MobileFileIOActivity.java
├── Intent.ACTION_OPEN_DOCUMENT を使用
├── Storage Access Framework (SAF)
├── Content URI からファイル名を取得
└── キャッシュディレクトリにコピー
```

**処理フロー:**
1. `MobileFileIOActivity` を起動
2. SAF のファイル選択UIを表示
3. Content URI を取得
4. `ContentResolver` でファイルを読み取り
5. キャッシュディレクトリにコピー
6. Unity にパスをコールバック

## 注意事項

### 全般

- 選択されたファイルは `Application.persistentDataPath` 内にコピーされます
- 元のファイルは変更されません
- コピー後のファイルパスが `result.FilePath` として返されます

### iOS

- iOS 14以降: `UTType` API を使用
- iOS 11-13: 従来の `UIDocumentPickerViewController` API を使用
- iCloud Driveからのファイル選択に対応

### Android

- Storage Access Framework (SAF) を使用
- Content URI からファイルを読み取り、キャッシュディレクトリにコピー
- Android 10 (API 29) 以降の Scoped Storage に完全対応

## トラブルシューティング

### iOS: ファイルが選択できない

Info.plist に以下が必要な場合があります：

```xml
<key>UIFileSharingEnabled</key>
<true/>
<key>LSSupportsOpeningDocumentsInPlace</key>
<true/>
```

### Android: 特定のファイルタイプが表示されない

一部のファイルマネージャーはMIMEタイプのフィルタリングを正しく処理しません。
`AllowedExtensions = null` で全ファイル表示を試してください。

### Android: ビルドエラー

`AndroidManifest.xml` のマージ問題が発生した場合：

1. `Player Settings > Publishing Settings > Build` を確認
2. `Custom Main Manifest` を使用している場合、Activity定義が重複していないか確認

## サンプルプロジェクト

```csharp
using UnityEngine;
using DSGarage.MobileFileIO;
using System.IO;

public class FilePickerExample : MonoBehaviour
{
    public void OnPickFileButtonClicked()
    {
        var options = new PickerOptions
        {
            Title = "ファイルを選択",
            AllowedExtensions = new[] { "txt", "json" },
            DestinationFolder = "Downloads"
        };

        MobileFilePicker.PickFile(OnFilePicked, options);
    }

    private void OnFilePicked(PickerResult result)
    {
        if (result.Success)
        {
            string content = File.ReadAllText(result.FilePath);
            Debug.Log($"ファイル内容:\n{content}");
        }
        else if (result.Cancelled)
        {
            Debug.Log("キャンセルされました");
        }
        else
        {
            Debug.LogError($"エラー: {result.ErrorMessage}");
        }
    }
}
```

## ライセンス

MIT License - 詳細は [LICENSE.md](LICENSE.md) を参照

## 作者

dsgarage (dsgarage@gmail.com)

## 関連リンク

- [GitHub Issues](https://github.com/dsgarage/UnityMobileFileIO/issues) - バグ報告・機能要望
