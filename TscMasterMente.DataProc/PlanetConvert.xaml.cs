using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TscMasterMente.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Threading.Tasks;
using TscMasterMente.Common.MasterFileEntity;
using TscMasterMente.Common.SqlMaps;
using TscMasterMente.Common.PlanetFileEntity;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlanetConvert : Window
    {
        #region コンストラクタ

        public PlanetConvert()
        {
            this.InitializeComponent();
            this.Activated += PlanetConvert_Activated;
        }

        #endregion

        #region イベント

        /// <summary>
        /// Activatedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlanetConvert_Activated(object sender, WindowActivatedEventArgs e)
        {
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 800);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            //Planet形式からデータ変換ラジオボタンを選択
            RbExport.IsChecked = true;

            this.Activated -= PlanetConvert_Activated;
        }

        /// <summary>
        /// Planet形式からデータ変換ラジオボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RbConvert_Checked(object sender, RoutedEventArgs e)
        {
            string wExplain = "プラネットのデータ形式を棚サイエンスで扱うことのできるデータ形式へ変換します。";
            wExplain += "作業の対象となるのは商品マスタと画像となります。";
            wExplain += "作業が完了後、[棚サイエンス用マスタ]-[取込]からマスタをローカルマスタに格納してください。";

            TxtExplain.Text = wExplain;
        }

        /// <summary>
        /// Planet形式へデータ変換ラジオボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RbExport_Checked(object sender, RoutedEventArgs e)
        {
            string wExplain = "ローカルマスタの内容を、プラネットのデータ形式へ出力します。";
            wExplain += "作業の対象となるのは商品マスタ・画像となります。";

            TxtExplain.Text = wExplain;
        }

        /// <summary>
        /// 閉じるボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Planet親ディレクトリボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnParentDir_Click(object sender, RoutedEventArgs e)
        {
            //フォルダ選択ダイアログ
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.XlsxFiles);
            if (wPath != null)
            {
                TxtParentPath.Text = wPath;
            }
        }

        /// <summary>
        /// Planet出力ディレクトリボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnExportDir_Click(object sender, RoutedEventArgs e)
        {
            //フォルダ選択ダイアログ
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.XlsxFiles);
            if (wPath != null)
            {
                TxtExportPath.Text = wPath;
            }
        }

        /// <summary>
        /// 実行ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnExec_Click(object sender, RoutedEventArgs e)
        {
            var wProgressWindow = new ProgressWindow();
            try
            {
                //ウィンドウの無効化
                WindowParts.SetAllChildEnabled(MainContent, false);
                

                #region チェック処理

                //出力先フォルダが設定されているかチェック
                var wExpPath = TxtExportPath.Text;
                if (string.IsNullOrEmpty(wExpPath))
                {
                    //Okボタンメッセージボックスの表示
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "出力先フォルダが設定されていません。");
                    await wDialog.ShowAsync();

                    return;
                }
                else if (Directory.Exists(wExpPath) == false)
                {
                    Directory.CreateDirectory(wExpPath);
                }
                //clsSql.UpdateAppInfo("", wExpPath);

                #endregion

                #region 実行処理

                if (RbConvert.IsChecked == true)
                {
                    //Planet形式からデータ変換
                }
                else if(RbExport.IsChecked == true)
                {
                    //Planet形式へデータ変換
                }
                else
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "処理が実行できませんでした。");
                    await tMsgDialog.ShowAsync();
                }

                #endregion

            }
            catch (Exception ex)
            {
                var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", ex.Message);
                await tMsgDialog.ShowAsync();
            }
            finally
            {
                //プログレスウィンドウのクローズ
                wProgressWindow.Close();
                wProgressWindow = null;

                //ウィンドウの有効化
                WindowParts.SetAllChildEnabled(MainContent, true);
            }

            GetIniFileData(@"C:\Tsc\TANA.ini", "TANA", "SBMP");

            string bmpFilePath = @"C:\Nisprg\Sih\TanaC\bmp\0000045\214233A.bmp";
            string jpgFilePath = @"C:\Nisprg\Sih\TanaC\bmp\0000045\214233A.jpg";
            await CnvBmp2Jpg(bmpFilePath, jpgFilePath);
        }


        #endregion

        #region メソッド

        private async void CnvTana2Planet(ProgressWindow argProgWindow)
        {
            var clsSql = new SqliteParts();
            var clsCsv = new CsvHelperParts();

            string wSql = "SELECT * FROM SyoMaster order by JanCode ";
            var dtSyoMaster = clsSql.GetSelectResult<SyoMaster>(wSql);

            string wExpDir = TxtExportPath.Text;
            await argProgWindow.SetInit("プラネットデータ変換中...", dtSyoMaster.Count);
            foreach(var iSyoMaster in dtSyoMaster)
            {
                //プログレスウィンドウの更新
                await argProgWindow.SetProgress(iSyoMaster.JanCode);

                //Planetコンバート先フォルダパスの取得
                var wExpPath = GetPlanetFolderPath(iSyoMaster.JanCode, wExpDir);

                if(Directory.Exists(wExpPath) == false)
                {
                    Directory.CreateDirectory(wExpPath);
                }

                var wCsvData = new PlanetDataEntity();
                wCsvData.Id = 1;
                wCsvData.ItemDivision=GetPlanetClass(iSyoMaster.JanCode);
                wCsvData.TanName1 = iSyoMaster.TanName;
                wCsvData.TanName2 = iSyoMaster.TanName;
                wCsvData.MkCode = iSyoMaster.MkCode;
                wCsvData.BunCode = iSyoMaster.BunCode;
                wCsvData.W = iSyoMaster.W;
                wCsvData.H = iSyoMaster.H;
                wCsvData.D = iSyoMaster.D;
                wCsvData.Price = iSyoMaster.Price;
                wCsvData.Cost = iSyoMaster.Price;
                wCsvData.JanCode = iSyoMaster.JanCode;
                clsCsv.WriteFile<PlanetDataEntity>(wExpPath, new List<PlanetDataEntity>() { wCsvData }, false, ",", true);

            }

        }

        /// <summary>
        /// BMPファイルをJPGファイルに変換する
        /// </summary>
        /// <param name="argBmpPath">BMPファイルパス</param>
        /// <param name="argJpgPath">JPGファイルパス</param>
        /// <returns></returns>
        private async Task CnvBmp2Jpg(string argBmpPath, string argJpgPath)
        {
            #region チェック処理
            if (!File.Exists(argBmpPath))
            {
                return;
            }
            else if (Path.GetExtension(argBmpPath).ToUpper() != ".BMP")
            {
                return;
            }
            StorageFile bmpFile = await StorageFile.GetFileFromPathAsync(argBmpPath);

            if (File.Exists(argJpgPath))
            {
                File.Delete(argJpgPath);
            }
            if (Directory.Exists(Path.GetDirectoryName(argJpgPath)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(argJpgPath));
            }
            StorageFolder wFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(argJpgPath));
            StorageFile jpgFile = await wFolder.CreateFileAsync(Path.GetFileName(argJpgPath), CreationCollisionOption.ReplaceExisting);

            #endregion

            #region 変換処理                      

            using (IRandomAccessStream stream = await bmpFile.OpenAsync(FileAccessMode.Read))
            {
                // BitmapDecoderを使用してBMPファイルをデコード
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.BmpDecoderId, stream);

                // デコードした画像をSoftwareBitmapとして取得
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // JPGファイル用のストリームを開く
                using (IRandomAccessStream jpgStream = await jpgFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // BitmapEncoderを使用してJPGファイルをエンコード
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, jpgStream);
                    encoder.SetSoftwareBitmap(softwareBitmap);
                    await encoder.FlushAsync();
                }
            }

            #endregion
        }

        /// <summary>
        /// INIファイルからデータを取得する
        /// </summary>
        /// <param name="argPath">Iniファイルパス</param>
        /// <param name="argSection">セクション名</param>
        /// <param name="argKey">キー名</param>
        /// <returns></returns>
        private string GetIniFileData(string argPath, string argSection, string argKey)
        {
            string wRet = "";
            if (File.Exists(argPath))
            {
                var wDicData = new Dictionary<string, Dictionary<string, string>>();

                string currentSection = "";
                foreach (var line in File.ReadAllLines(argPath))
                {
                    var trimmedLine = line.Trim();

                    // セクションの判定
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        if (!wDicData.ContainsKey(currentSection))
                        {
                            wDicData[currentSection] = new Dictionary<string, string>();
                        }
                    }
                    // キー-バリューの判定
                    else if (trimmedLine.Contains("="))
                    {
                        var keyValue = trimmedLine.Split(new[] { '=' }, 2);
                        if (keyValue.Length == 2)
                        {
                            var key = keyValue[0].Trim();
                            var value = keyValue[1].Trim();
                            if (!wDicData[currentSection].ContainsKey(key))
                            {
                                wDicData[currentSection][key] = value;
                            }
                        }
                    }
                }

                if (wDicData.Count > 0 && wDicData.ContainsKey(argSection))
                {
                    if (wDicData[argSection].ContainsKey(argKey))
                    {
                        wRet = wDicData[argSection][argKey];
                    }
                }

            }

            return wRet;
        }

        /// <summary>
        /// 13桁フルのJanコードの画像パスを取得する
        /// </summary>
        /// <param name="argTanaImgDir">棚サイエンスの画像フォルダ</param>
        /// <param name="argJanCode">Janコード</param>
        /// <param name="argSideType">画像位置(0:正面,1:側面,2:上面)</param>
        /// <returns></returns>
        private string GetFullJanImgPath(string argTanaImgDir, string argJanCode, int argSideType)
        {
            if (string.IsNullOrEmpty(argTanaImgDir) || Directory.Exists(argTanaImgDir))
            {
                return null;
            }

            if (string.IsNullOrEmpty(argJanCode))
            {
                return null;
            }

            string wFileName = null;
            var wJanCode = argJanCode.PadLeft(13, '0');
            switch (argSideType)
            {
                case 1:
                    wFileName = wJanCode + "c.jpg";
                    break;
                case 2:
                    wFileName = wJanCode + "e.jpg";
                    break;
                default:
                    wFileName = wJanCode + "a.jpg";
                    break;
            }

            if (string.IsNullOrEmpty(wFileName))
            {
                return null;
            }
            else
            {
                return Path.Combine(argTanaImgDir, wFileName);
            }
        }

        /// <summary>
        /// 7桁6桁に分割したJanコードの画像パスを取得する
        /// </summary>
        /// <param name="argTanaImgDir">棚サイエンスの画像フォルダ</param>
        /// <param name="argJanCode">Janコード</param>
        /// <param name="argSideType">画像位置(0:正面,1:側面,2:上面)</param>
        /// <returns></returns>
        private string GetDivideJanImgPath(string argTanaImgDir, string argJanCode, int argSideType)
        {
            if (string.IsNullOrEmpty(argTanaImgDir) || Directory.Exists(argTanaImgDir))
            {
                return null;
            }

            if (string.IsNullOrEmpty(argJanCode))
            {
                return null;
            }

            string wFileName = null;
            var wLeftJanCode = argJanCode.PadLeft(13, '0').Substring(0, 7);
            var wRightJanCode = argJanCode.PadRight(13, '0').Substring(7);
            switch (argSideType)
            {
                case 1:
                    wFileName = wRightJanCode + "c.jpg";
                    break;
                case 2:
                    wFileName = wRightJanCode + "e.jpg";
                    break;
                default:
                    wFileName = wRightJanCode + "a.jpg";
                    break;
            }

            if (string.IsNullOrEmpty(wFileName))
            {
                return null;
            }
            else
            {
                return Path.Combine(argTanaImgDir, wLeftJanCode, wFileName);
            }
        }

        /// <summary>
        /// Planetデータ区分を取得する
        /// </summary>
        /// <param name="argJanCode">JANコード</param>
        /// <returns>
        ///  [J],[U],[E]のいずれかを返す(nullの場合はエラー)
        /// </returns>
        /// <remarks>
        /// Janコードの桁数によって以下のように判定する
        ///  8桁: 先頭「45」、「49」ならば「J」、それ以外「U」
        /// 12桁: 「U」
        /// 13桁: 先頭「45」、「49」ならば「J」、それ以外「E」
        /// 上記以外: エラー
        /// </remarks>
        private string GetPlanetClass(string argJanCode)
        {
            string wRet = null;
            if (string.IsNullOrEmpty(argJanCode))
            {
                return wRet;
            }

            //桁数による判定
            switch (argJanCode.Length)
            {
                case 8:
                    if (argJanCode.Substring(0, 2) == "45" || argJanCode.Substring(0, 2) == "49")
                    {
                        wRet = "J";
                    }
                    else
                    {
                        wRet = "U";
                    }
                    break;
                case 12:
                    wRet = "U";
                    break;
                case 13:
                    if (argJanCode.Substring(0, 2) == "45" || argJanCode.Substring(0, 2) == "49")
                    {
                        wRet = "J";
                    }
                    else
                    {
                        wRet = "E";
                    }
                    break;
                default:
                    break;
            }

            return wRet;
        }

        /// <summary>
        /// Planetフォルダパスを取得する
        /// </summary>
        /// <param name="argJanCode">JANコード</param>
        /// <param name="argBaseDir">Planetデータ出力先親フォルダ</param>
        /// <returns></returns>
        private string GetPlanetFolderPath(string argJanCode, string argBaseDir)
        {
            string wRet = null;
            switch (argJanCode.Length)
            {
                case 8:
                case 12:
                    wRet = Path.Combine(argBaseDir, GetPlanetClass(argJanCode), argJanCode.Substring(0, 6), argJanCode.Substring(6));
                    break;
                case 13:
                    wRet = Path.Combine(argBaseDir, GetPlanetClass(argJanCode), argJanCode.Substring(0, 7), argJanCode.Substring(6));
                    break;
                default:
                    break;
            }

            return wRet;
        }

        #endregion


    }
}
