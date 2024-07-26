using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Net.Http.Headers;
using Microsoft.UI.Xaml;
using System.Collections;
using System.IO;
using System.Text.Json;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace TscMasterMente.Common
{
    public static class IoParts
    {
        #region Enum

        /// <summary>
        /// ファイルの種類
        /// </summary>
        public enum EnumFileType
        {
            /// <summary>
            /// AllFiles
            /// </summary>
            AllFiles = 0,
            /// <summary>
            /// テキストファイル
            /// </summary>
            TextFiles = 1,
            /// <summary>
            /// Excelファイル
            /// </summary>
            XlsxFiles = 2,
            /// <summary>
            /// CSVファイル
            /// </summary>
            CsvFiles = 3,
            /// <summary>
            /// 画像ファイル
            /// </summary>
            ImageFiles = 4,
        }

        #endregion

        #region メソッド

        #region Picker

        /// <summary>
        /// フォルダを開くダイアログ
        /// </summary>
        /// <param name="argWindow">Window</param>
        /// <returns></returns>
        public static async Task<string> PickFolderAsync(Window argWindow)
        {

            // FolderPickerのインスタンスを作成
            var picker = new FolderPicker();

            // ファイルの種類を指定（ここではフォルダ選択なので"*"）
            picker.FileTypeFilter.Add("*");

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(argWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            // FolderPickerを表示
            StorageFolder folder = await picker.PickSingleFolderAsync();


            // 選択されたフォルダのパスを返す
            return folder?.Path;
        }


        /// <summary>
        /// ファイルを開くダイアログ
        /// </summary>
        /// <param name="argWindow">Window</param>
        /// <param name="argFileType">ファイルの種類</param>
        /// <returns></returns>
        public static async Task<string> OpenFileAsync(Window argWindow, EnumFileType argFileType)
        {
            // FileOpenPickerのインスタンスを作成
            var picker = new FileOpenPicker();

            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // ファイルの種類を指定            
            //デフォルトで選択する直接的な方法はない模様。
            //対策方法としてファイルタイプを先頭に置くのが一般的な模様
            switch (argFileType)
            {
                case EnumFileType.TextFiles:
                    picker.FileTypeFilter.Add(".txt");
                    picker.FileTypeFilter.Add("*");
                    break;
                case EnumFileType.XlsxFiles:
                    picker.FileTypeFilter.Add(".xlsx");
                    picker.FileTypeFilter.Add("*");
                    break;
                case EnumFileType.CsvFiles:
                    picker.FileTypeFilter.Add(".csv");
                    picker.FileTypeFilter.Add("*");
                    break;
                case EnumFileType.ImageFiles:
                    picker.FileTypeFilter.Add(".png");
                    picker.FileTypeFilter.Add(".jpg");
                    picker.FileTypeFilter.Add("*");
                    break;
                default:
                    picker.FileTypeFilter.Add("*");
                    break;
            }

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(argWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            // FileOpenPickerを表示
            StorageFile file = await picker.PickSingleFileAsync();

            // 選択されたファイルのパスを返す
            return file?.Path;
        }

        /// <summary>
        /// ファイルを保存するダイアログ  
        /// </summary>
        /// <param name="argWindow">Window</param>
        /// <param name="argFileType">ファイルの種類</param>
        /// <param name="argStartLoc">開始フォルダ</param>
        /// <param name="argFileName">ファイル名</param>
        /// <returns></returns>
        public static async Task<string> SaveFileAsync(Window argWindow, EnumFileType argFileType,  string argFileName = "NewFile")
        {
            var picker = new FileSavePicker();

            // ファイルの種類を指定
            switch (argFileType)
            {
                case EnumFileType.TextFiles:
                    picker.FileTypeChoices.Add("テキストファイル", new List<string>() { ".txt" });
                    break;
                case EnumFileType.XlsxFiles:
                    picker.FileTypeChoices.Add("Excelファイル", new List<string>() { ".xlsx" });
                    break;
                case EnumFileType.CsvFiles:
                    picker.FileTypeChoices.Add("CSVファイル", new List<string>() { ".csv" });
                    break;
                case EnumFileType.ImageFiles:
                    picker.FileTypeChoices.Add("画像ファイル", new List<string>() { ".png", ".jpg" });
                    break;
                default:
                    break;
            }
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // デフォルトのファイル名
            picker.SuggestedFileName = argFileName;

            // WinRT.Interopを使用してウィンドウハンドルを取得し、FileSavePickerに関連付ける
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(argWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            // ファイルの保存ダイアログを表示し、ユーザーがファイルを保存
            StorageFile file = await picker.PickSaveFileAsync();

            return file?.Path;
        }

        /// <summary>
        /// フォルダ内のファイルを取得
        /// </summary>
        /// <param name="argDirPath">フォルダパス</param>
        /// <param name="argIsAll">フォルダ配下のファイルも取得(true:配下も含めて取得,false:カレントディレクトリのみ)</param>
        /// <param name="argExtensions">取得する拡張子</param>
        /// <returns></returns>
        public static async Task<IList<StorageFile>> GetFilesAsync(string argDirPath, bool argIsAll = false, List<string> argExtensions = null)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(argDirPath);
            List<StorageFile> files = new List<StorageFile>();

            if (argIsAll)
            {
                await GetFilesRecursiveAsync(folder, files, argExtensions);
            }
            else
            {
                IReadOnlyList<StorageFile> folderFiles = await folder.GetFilesAsync();
                if (argExtensions != null && argExtensions.Any())
                {
                    folderFiles = folderFiles.Where(file => argExtensions.Contains(file.FileType.ToLower())).ToList();
                }
                files.AddRange(folderFiles);
            }

            return files;
        }

        /// <summary>
        /// ファイルを再帰的に取得
        /// </summary>
        /// <param name="argDirectory">フォルダ</param>
        /// <param name="argFiles">取得リスト</param>
        /// <param name="argExtensions">取得する拡張子</param>
        /// <returns></returns>
        private static async Task GetFilesRecursiveAsync(StorageFolder argDirectory, List<StorageFile> argFiles, List<string> argExtensions = null)
        {
            // 現在のフォルダーのファイルを取得
            IReadOnlyList<StorageFile> folderFiles = await argDirectory.GetFilesAsync();
            if (argExtensions != null && argExtensions.Any())
            {
                folderFiles = folderFiles.Where(file => argExtensions.Contains(file.FileType.ToLower())).ToList();
            }
            argFiles.AddRange(folderFiles);

            // サブフォルダーを取得
            IReadOnlyList<StorageFolder> subfolders = await argDirectory.GetFoldersAsync();
            foreach (var subfolder in subfolders)
            {
                await GetFilesRecursiveAsync(subfolder, argFiles, argExtensions);
            }
        }

        /// <summary>
        /// 対象ディレクトリからマスタファイルの取得
        /// </summary>
        /// <param name="argDirPath">フォルダパス</param>
        /// <returns></returns>
        public static async Task<IList<StorageFile>> GetMasterFiles(string argDirPath)
        {
            var wExtensions = new List<string> { ".txt" };
            return await GetFilesAsync(argDirPath, false, wExtensions);
        }

        #endregion


        #endregion
    }
}
