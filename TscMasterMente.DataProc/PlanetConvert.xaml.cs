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
using Serilog.Core;
using Serilog;
using DocumentFormat.OpenXml.Vml;
using System.Text;
using ClosedXML;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{   

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlanetConvert : Window
    {
        #region private 変数

        /// <summary>
        /// ロード済みフラグ
        /// </summary>
        private bool _IsLoaded = false;
        #endregion

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
            //アプリアイコンを設定
            WindowParts.SetAppIcon(this);
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 800);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            //Planet形式からデータ変換ラジオボタンを選択
            RbExport.IsChecked = true;
            SetCntrEnabled();

            //マスタファイルパス設定
            var clsSql = new SqliteParts();
            var dirPath = clsSql.GetAppInfo("PlanetWriteDir");
            if (!string.IsNullOrEmpty(dirPath))
            {
                var wParentDir = Directory.GetParent(dirPath);                
                TxtParentPath.Text = wParentDir.FullName;
                TxtExportPath.Text = dirPath;
            }
            else
            {
                TxtParentPath.Text = null;
                TxtParentPath.Text = null;
            }

            _IsLoaded = true;
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

            if (_IsLoaded) SetCntrEnabled();
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

            if (_IsLoaded) SetCntrEnabled();
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
            var wSucceed = false;
            var clsLog=new LogParts();
            using (var wLog = clsLog.GetInstance())
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
                        wLog.Information("プラネットデータからローカルマスタに変換開始");
                        wLog.Information("プラネットデータからローカルマスタに変換終了");
                    }
                    else if (RbExport.IsChecked == true)
                    {
                        //Planet形式へデータ変換
                        wLog.Information("ローカルマスタからプラネットデータに変換開始");
                        wSucceed = await CnvTana2Planet(wProgressWindow, wLog);
                        wLog.Information("ローカルマスタからプラネットデータに変換終了");
                    }
                    else
                    {
                        var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "致命的な問題が発生しています。");
                        await tMsgDialog.ShowAsync();
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", ex.Message);
                    await tMsgDialog.ShowAsync();
                    wSucceed = false;
                }
                finally
                {
                    //プログレスウィンドウのクローズ
                    wProgressWindow.Close();
                    wProgressWindow = null;

                    //ウィンドウの有効化
                    WindowParts.SetAllChildEnabled(MainContent, true);

                    wLog.Information("End");

                    if (wSucceed)
                    {
                        var wDialog = MessageParts.ShowMessageOkOnly(this, "完了", "プラネットデータの出力が完了しました。");
                        await wDialog.ShowAsync();
                    }
                }
            }
        }


        #endregion

        #region メソッド

        /// <summary>
        /// 棚サイエンスデータをPlanetデータに変換する
        /// </summary>
        /// <param name="argProgWindow">プログレスウィンドウ</param>
        /// <returns></returns>
        private async Task<bool> CnvTana2Planet(ProgressWindow argProgWindow,Logger argLog)
        {
            var clsSql = new SqliteParts();
            var clsCsv = new CsvHelperParts();

            var wTanaIniDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var wTanaIniPath = System.IO.Path.Combine(wTanaIniDir, "TANA.ini");
            var wMaserImgPath = GetIniFileData(wTanaIniPath, "TANA", "SBMP");
            if (string.IsNullOrEmpty(wMaserImgPath))
            {
                argLog.Error("TANA.iniファイルが存在しないか、SBMPの設定がされていません。");
                return false;
            }

            argLog.Information("商品マスタの取得開始");
            string wSql = "SELECT * FROM SyoMaster order by JanCode ";
            var dtSyoMaster = clsSql.GetSelectResult<SyoMaster>(wSql);
            argLog.Information("商品マスタの取得終了");

            string wExpDir = TxtExportPath.Text;

            argProgWindow.Activate();
            await argProgWindow.SetInit("プラネットデータ変換中...", dtSyoMaster.Count);

            foreach (var iSyoMaster in dtSyoMaster)
            {
                //プログレスウィンドウの更新
                if (await argProgWindow.SetProgress(iSyoMaster.JanCode) == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                    await wDialog.ShowAsync();
                    argLog.Error($"キャンセルを実行しました。");
                    return false;
                }

                //Planetコンバート先フォルダパスの取得
                var wExpPath = GetPlanetFolderPath(iSyoMaster.JanCode, wExpDir);
                if (string.IsNullOrEmpty(wExpPath))
                {
                    argLog.Error($"JANコード='{iSyoMaster.JanCode}'は処理出来ません。(8桁、12桁、13桁以外)");
                    continue;
                }

                #region Data.csvの出力

                if (Directory.Exists(wExpPath) == false)
                {
                    Directory.CreateDirectory(wExpPath);
                    argLog.Information($"フォルダを作成しました。({wExpPath})");
                }

                var wCsvData = new PlanetDataEntity();
                wCsvData.Id = 1;
                wCsvData.ItemDivision = GetPlanetClass(iSyoMaster.JanCode);
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

                var wCsvPath = System.IO.Path.Combine(wExpPath, "Data.csv");
                clsCsv.WriteFile<PlanetDataEntity>(wCsvPath, new List<PlanetDataEntity>() { wCsvData }, false, ",", false);
                argLog.Information($"Csvファイルを出力しました。({wCsvPath})");

                #endregion

                #region 画像の出力

                //A面 画像
                const string IMAGEFILE1 = "Image1.jpg";
                //'C面 画像
                const string IMAGEFILE2 = "Image2.jpg";
                //'E面 画像
                const string IMAGEFILE3 = "Image3.jpg";
                foreach (var iExtention in new List<string> { "bmp", "jpg" })
                {
                    //Jan13桁フル
                    foreach (var iSide in new List<int> { 0, 1, 2 })
                    {
                        string wOrgImgPath = GetFullJanImgPath(wMaserImgPath, iSyoMaster.JanCode, iSide, iExtention);
                        if (!string.IsNullOrEmpty(wOrgImgPath) && File.Exists(wOrgImgPath) == true)
                        {
                            string wExpImgPath = null;
                            switch (iSide)
                            {
                                case 1:
                                    wExpImgPath = System.IO.Path.Combine(wExpPath, IMAGEFILE2);
                                    break;
                                case 2:
                                    wExpImgPath = System.IO.Path.Combine(wExpPath, IMAGEFILE3);
                                    break;
                                default:
                                    wExpImgPath = System.IO.Path.Combine(wExpPath, IMAGEFILE1);
                                    break;
                            }
                            if (iExtention.ToUpper() == "BMP")
                            {
                                await CnvBmp2Jpg(wOrgImgPath, wExpImgPath);
                            }
                            else
                            {
                                System.IO.File.Copy(wOrgImgPath, wExpImgPath, true);
                            }
                            argLog.Information($"画像ファイルを出力しました。({wExpImgPath})");
                        }
                    }

                    //Jan7桁6桁
                    foreach (var iSide in new List<int> { 0, 1, 2 })
                    {
                        string wOrgImgPath = GetDivideJanImgPath(wMaserImgPath, iSyoMaster.JanCode, iSide, iExtention);
                        if (!string.IsNullOrEmpty(wOrgImgPath) && File.Exists(wOrgImgPath) == true)
                        {
                            string wExpImgPath = null;
                            switch (iSide)
                            {
                                case 1:
                                    wExpImgPath = System.IO.Path.Combine(wExpPath, IMAGEFILE2);
                                    break;
                                case 2:
                                    wExpImgPath = System.IO.Path.Combine(wExpPath, IMAGEFILE3);
                                    break;
                                default:
                                    wExpImgPath = System.IO.Path.Combine(wExpPath, IMAGEFILE1);
                                    break;
                            }

                            if (iExtention.ToUpper() == "BMP")
                            {
                                await CnvBmp2Jpg(wOrgImgPath, wExpImgPath);
                            }
                            else
                            {
                                System.IO.File.Copy(wOrgImgPath, wExpImgPath, true);
                            }
                            argLog.Information($"画像ファイルを出力しました。({wExpImgPath})");
                        }
                    }
                }
                #endregion

            }

            return true;
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
            else if (System.IO.Path.GetExtension(argBmpPath).ToUpper() != ".BMP")
            {
                return;
            }
            StorageFile bmpFile = await StorageFile.GetFileFromPathAsync(argBmpPath);

            if (File.Exists(argJpgPath))
            {
                File.Delete(argJpgPath);
            }
            if (Directory.Exists(System.IO.Path.GetDirectoryName(argJpgPath)) == false)
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(argJpgPath));
            }
            StorageFolder wFolder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(argJpgPath));
            StorageFile jpgFile = await wFolder.CreateFileAsync(System.IO.Path.GetFileName(argJpgPath), CreationCollisionOption.ReplaceExisting);

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
                foreach (var line in File.ReadAllLines(argPath, Encoding.GetEncoding("Shift_JIS")))
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
        /// <param name="argExtension">拡張子</param>
        /// <returns></returns>
        private string GetFullJanImgPath(string argTanaImgDir, string argJanCode, int argSideType, string argExtension)
        {
            if (string.IsNullOrEmpty(argTanaImgDir) || !Directory.Exists(argTanaImgDir))
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
                    wFileName = wJanCode + $"c.{argExtension}";
                    break;
                case 2:
                    wFileName = wJanCode + $"e.{argExtension}";
                    break;
                default:
                    wFileName = wJanCode + $"a.{argExtension}";
                    break;
            }

            if (string.IsNullOrEmpty(wFileName))
            {
                return null;
            }
            else
            {
                return System.IO.Path.Combine(argTanaImgDir, wFileName);
            }
        }

        /// <summary>
        /// 7桁6桁に分割したJanコードの画像パスを取得する
        /// </summary>
        /// <param name="argTanaImgDir">棚サイエンスの画像フォルダ</param>
        /// <param name="argJanCode">Janコード</param>
        /// <param name="argSideType">画像位置(0:正面,1:側面,2:上面)</param>
        /// <param name="argExtension">拡張子</param>
        /// <returns></returns>
        private string GetDivideJanImgPath(string argTanaImgDir, string argJanCode, int argSideType, string argExtension)
        {
            if (string.IsNullOrEmpty(argTanaImgDir) || !Directory.Exists(argTanaImgDir))
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
                    wFileName = wRightJanCode + $"c.{argExtension}";
                    break;
                case 2:
                    wFileName = wRightJanCode + $"e.{argExtension}";
                    break;
                default:
                    wFileName = wRightJanCode + $"a.{argExtension}";
                    break;
            }

            if (string.IsNullOrEmpty(wFileName))
            {
                return null;
            }
            else
            {
                return System.IO.Path.Combine(argTanaImgDir, wLeftJanCode, wFileName);
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

            var wPlanetClass = GetPlanetClass(argJanCode);
            if (string.IsNullOrEmpty(wPlanetClass))
            {
                //Planetデータ区分が取得できない場合はエラー
                return null;
            }

            switch (argJanCode.Length)
            {
                case 8:
                case 12:
                    wRet = System.IO.Path.Combine(argBaseDir, wPlanetClass, argJanCode.Substring(0, 6), argJanCode.Substring(6));
                    break;
                case 13:
                    wRet = System.IO.Path.Combine(argBaseDir, wPlanetClass, argJanCode.Substring(0, 7), argJanCode.Substring(6));
                    break;
                default:
                    break;
            }

            return wRet;
        }

        /// <summary>
        /// 設定によるコントロール制御
        /// </summary>
        private void SetCntrEnabled()
        {
            var wEnabled = (RbConvert.IsChecked == true);
            TxtParentPath.IsEnabled = wEnabled;
            BtnParentDir.IsEnabled = wEnabled;
            ChkaBmpCnv.IsEnabled = wEnabled;
            ChkAddUnit.IsEnabled = wEnabled;
            ChkMaker.IsEnabled = wEnabled;
            ChkJICFS.IsEnabled = wEnabled;
        }

        #endregion


    }
}
