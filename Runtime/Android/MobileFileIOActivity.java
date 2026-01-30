package com.dsgarage.mobilefileio;

import android.app.Activity;
import android.content.ClipData;
import android.content.ContentResolver;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.provider.OpenableColumns;
import android.util.Log;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;

/**
 * ファイルピッカー用のActivity
 * Storage Access Framework (SAF) を使用
 */
public class MobileFileIOActivity extends Activity {

    private static final String TAG = "MobileFileIO";
    private static final int REQUEST_CODE_PICK_FILE = 1001;

    private static String unityCallbackObject = null;
    private static boolean allowMultiple = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Intent intent = getIntent();
        if (intent != null) {
            unityCallbackObject = intent.getStringExtra("callbackObject");
            allowMultiple = intent.getBooleanExtra("allowMultiple", false);
            String[] mimeTypes = intent.getStringArrayExtra("mimeTypes");

            openFilePicker(mimeTypes, allowMultiple);
        } else {
            finish();
        }
    }

    private void openFilePicker(String[] mimeTypes, boolean allowMultiple) {
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        intent.addCategory(Intent.CATEGORY_OPENABLE);

        if (mimeTypes != null && mimeTypes.length == 1) {
            intent.setType(mimeTypes[0]);
        } else {
            intent.setType("*/*");
            if (mimeTypes != null && mimeTypes.length > 1) {
                intent.putExtra(Intent.EXTRA_MIME_TYPES, mimeTypes);
            }
        }

        if (allowMultiple) {
            intent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true);
        }

        try {
            startActivityForResult(intent, REQUEST_CODE_PICK_FILE);
        } catch (Exception e) {
            Log.e(TAG, "Failed to open file picker: " + e.getMessage());
            sendErrorToUnity("Failed to open file picker: " + e.getMessage());
            finish();
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == REQUEST_CODE_PICK_FILE) {
            if (resultCode == RESULT_OK && data != null) {
                ArrayList<String> copiedPaths = new ArrayList<>();

                // 複数選択
                ClipData clipData = data.getClipData();
                if (clipData != null) {
                    for (int i = 0; i < clipData.getItemCount(); i++) {
                        Uri uri = clipData.getItemAt(i).getUri();
                        String path = copyFileFromUri(uri);
                        if (path != null) {
                            copiedPaths.add(path);
                        }
                    }
                } else {
                    // 単一選択
                    Uri uri = data.getData();
                    if (uri != null) {
                        String path = copyFileFromUri(uri);
                        if (path != null) {
                            copiedPaths.add(path);
                        }
                    }
                }

                if (!copiedPaths.isEmpty()) {
                    if (allowMultiple) {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < copiedPaths.size(); i++) {
                            sb.append(copiedPaths.get(i));
                            if (i < copiedPaths.size() - 1) {
                                sb.append("|");
                            }
                        }
                        sendMultipleFilesToUnity(sb.toString());
                    } else {
                        sendFileToUnity(copiedPaths.get(0));
                    }
                } else {
                    sendErrorToUnity("Failed to copy selected files");
                }
            } else {
                sendCancelToUnity();
            }
        }

        finish();
    }

    private String copyFileFromUri(Uri uri) {
        try {
            ContentResolver resolver = getContentResolver();

            // ファイル名を取得
            String fileName = getFileName(resolver, uri);
            if (fileName == null) {
                fileName = "file_" + System.currentTimeMillis();
            }

            // キャッシュディレクトリにコピー
            File cacheDir = getCacheDir();
            File destFile = new File(cacheDir, fileName);

            InputStream inputStream = resolver.openInputStream(uri);
            if (inputStream == null) {
                Log.e(TAG, "Failed to open input stream for: " + uri);
                return null;
            }

            OutputStream outputStream = new FileOutputStream(destFile);
            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = inputStream.read(buffer)) != -1) {
                outputStream.write(buffer, 0, bytesRead);
            }

            inputStream.close();
            outputStream.close();

            Log.d(TAG, "Copied file to: " + destFile.getAbsolutePath());
            return destFile.getAbsolutePath();

        } catch (Exception e) {
            Log.e(TAG, "Failed to copy file: " + e.getMessage());
            return null;
        }
    }

    private String getFileName(ContentResolver resolver, Uri uri) {
        String fileName = null;

        if ("content".equals(uri.getScheme())) {
            try (Cursor cursor = resolver.query(uri, null, null, null, null)) {
                if (cursor != null && cursor.moveToFirst()) {
                    int index = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME);
                    if (index >= 0) {
                        fileName = cursor.getString(index);
                    }
                }
            } catch (Exception e) {
                Log.w(TAG, "Failed to get file name: " + e.getMessage());
            }
        }

        if (fileName == null) {
            fileName = uri.getLastPathSegment();
        }

        return fileName;
    }

    private void sendFileToUnity(String path) {
        if (unityCallbackObject != null) {
            com.unity3d.player.UnityPlayer.UnitySendMessage(
                unityCallbackObject,
                "OnFilePicked",
                path != null ? path : ""
            );
        }
    }

    private void sendMultipleFilesToUnity(String paths) {
        if (unityCallbackObject != null) {
            com.unity3d.player.UnityPlayer.UnitySendMessage(
                unityCallbackObject,
                "OnMultipleFilesPicked",
                paths != null ? paths : ""
            );
        }
    }

    private void sendCancelToUnity() {
        if (unityCallbackObject != null) {
            com.unity3d.player.UnityPlayer.UnitySendMessage(
                unityCallbackObject,
                "OnPickerCancelled",
                ""
            );
        }
    }

    private void sendErrorToUnity(String error) {
        if (unityCallbackObject != null) {
            com.unity3d.player.UnityPlayer.UnitySendMessage(
                unityCallbackObject,
                "OnPickerError",
                error
            );
        }
    }
}
