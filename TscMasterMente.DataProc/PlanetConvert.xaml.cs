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
        #region private �ϐ�

        /// <summary>
        /// ���[�h�ς݃t���O
        /// </summary>
        private bool _IsLoaded = false;
        #endregion

        #region �R���X�g���N�^

        public PlanetConvert()
        {
            this.InitializeComponent();            
            this.Activated += PlanetConvert_Activated;            
        }

        #endregion

        #region �C�x���g

        /// <summary>
        /// Activated�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlanetConvert_Activated(object sender, WindowActivatedEventArgs e)
        {
            //�A�v���A�C�R����ݒ�
            WindowParts.SetAppIcon(this);
            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 800);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            //Planet�`������f�[�^�ϊ����W�I�{�^����I��
            RbExport.IsChecked = true;
            SetCntrEnabled();

            //�}�X�^�t�@�C���p�X�ݒ�
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
        /// Planet�`������f�[�^�ϊ����W�I�{�^��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RbConvert_Checked(object sender, RoutedEventArgs e)
        {
            string wExplain = "�v���l�b�g�̃f�[�^�`����I�T�C�G���X�ň������Ƃ̂ł���f�[�^�`���֕ϊ����܂��B";
            wExplain += "��Ƃ̑ΏۂƂȂ�̂͏��i�}�X�^�Ɖ摜�ƂȂ�܂��B";
            wExplain += "��Ƃ�������A[�I�T�C�G���X�p�}�X�^]-[�捞]����}�X�^�����[�J���}�X�^�Ɋi�[���Ă��������B";

            TxtExplain.Text = wExplain;

            if (_IsLoaded) SetCntrEnabled();
        }

        /// <summary>
        /// Planet�`���փf�[�^�ϊ����W�I�{�^��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RbExport_Checked(object sender, RoutedEventArgs e)
        {
            string wExplain = "���[�J���}�X�^�̓��e���A�v���l�b�g�̃f�[�^�`���֏o�͂��܂��B";
            wExplain += "��Ƃ̑ΏۂƂȂ�̂͏��i�}�X�^�E�摜�ƂȂ�܂��B";

            TxtExplain.Text = wExplain;

            if (_IsLoaded) SetCntrEnabled();
        }

        /// <summary>
        /// ����{�^��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();            
        }

        /// <summary>
        /// Planet�e�f�B���N�g���{�^��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnParentDir_Click(object sender, RoutedEventArgs e)
        {
            //�t�H���_�I���_�C�A���O
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.XlsxFiles);
            if (wPath != null)
            {
                TxtParentPath.Text = wPath;
            }
        }

        /// <summary>
        /// Planet�o�̓f�B���N�g���{�^��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnExportDir_Click(object sender, RoutedEventArgs e)
        {
            //�t�H���_�I���_�C�A���O
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.XlsxFiles);
            if (wPath != null)
            {
                TxtExportPath.Text = wPath;
            }
        }

        /// <summary>
        /// ���s�{�^��
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
                    
                    //�E�B���h�E�̖�����
                    WindowParts.SetAllChildEnabled(MainContent, false);


                    #region �`�F�b�N����

                    //�o�͐�t�H���_���ݒ肳��Ă��邩�`�F�b�N
                    var wExpPath = TxtExportPath.Text;
                    if (string.IsNullOrEmpty(wExpPath))
                    {
                        //Ok�{�^�����b�Z�[�W�{�b�N�X�̕\��
                        var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�o�͐�t�H���_���ݒ肳��Ă��܂���B");
                        await wDialog.ShowAsync();

                        return;
                    }
                    else if (Directory.Exists(wExpPath) == false)
                    {
                        Directory.CreateDirectory(wExpPath);
                    }
                    //clsSql.UpdateAppInfo("", wExpPath);

                    #endregion

                    #region ���s����

                    if (RbConvert.IsChecked == true)
                    {
                        //Planet�`������f�[�^�ϊ�
                        wLog.Information("�v���l�b�g�f�[�^���烍�[�J���}�X�^�ɕϊ��J�n");
                        wLog.Information("�v���l�b�g�f�[�^���烍�[�J���}�X�^�ɕϊ��I��");
                    }
                    else if (RbExport.IsChecked == true)
                    {
                        //Planet�`���փf�[�^�ϊ�
                        wLog.Information("���[�J���}�X�^����v���l�b�g�f�[�^�ɕϊ��J�n");
                        wSucceed = await CnvTana2Planet(wProgressWindow, wLog);
                        wLog.Information("���[�J���}�X�^����v���l�b�g�f�[�^�ɕϊ��I��");
                    }
                    else
                    {
                        var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�v���I�Ȗ�肪�������Ă��܂��B");
                        await tMsgDialog.ShowAsync();
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", ex.Message);
                    await tMsgDialog.ShowAsync();
                    wSucceed = false;
                }
                finally
                {
                    //�v���O���X�E�B���h�E�̃N���[�Y
                    wProgressWindow.Close();
                    wProgressWindow = null;

                    //�E�B���h�E�̗L����
                    WindowParts.SetAllChildEnabled(MainContent, true);

                    wLog.Information("End");

                    if (wSucceed)
                    {
                        var wDialog = MessageParts.ShowMessageOkOnly(this, "����", "�v���l�b�g�f�[�^�̏o�͂��������܂����B");
                        await wDialog.ShowAsync();
                    }
                }
            }
        }


        #endregion

        #region ���\�b�h

        /// <summary>
        /// �I�T�C�G���X�f�[�^��Planet�f�[�^�ɕϊ�����
        /// </summary>
        /// <param name="argProgWindow">�v���O���X�E�B���h�E</param>
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
                argLog.Error("TANA.ini�t�@�C�������݂��Ȃ����ASBMP�̐ݒ肪����Ă��܂���B");
                return false;
            }

            argLog.Information("���i�}�X�^�̎擾�J�n");
            string wSql = "SELECT * FROM SyoMaster order by JanCode ";
            var dtSyoMaster = clsSql.GetSelectResult<SyoMaster>(wSql);
            argLog.Information("���i�}�X�^�̎擾�I��");

            string wExpDir = TxtExportPath.Text;

            argProgWindow.Activate();
            await argProgWindow.SetInit("�v���l�b�g�f�[�^�ϊ���...", dtSyoMaster.Count);

            foreach (var iSyoMaster in dtSyoMaster)
            {
                //�v���O���X�E�B���h�E�̍X�V
                if (await argProgWindow.SetProgress(iSyoMaster.JanCode) == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                    await wDialog.ShowAsync();
                    argLog.Error($"�L�����Z�������s���܂����B");
                    return false;
                }

                //Planet�R���o�[�g��t�H���_�p�X�̎擾
                var wExpPath = GetPlanetFolderPath(iSyoMaster.JanCode, wExpDir);
                if (string.IsNullOrEmpty(wExpPath))
                {
                    argLog.Error($"JAN�R�[�h='{iSyoMaster.JanCode}'�͏����o���܂���B(8���A12���A13���ȊO)");
                    continue;
                }

                #region Data.csv�̏o��

                if (Directory.Exists(wExpPath) == false)
                {
                    Directory.CreateDirectory(wExpPath);
                    argLog.Information($"�t�H���_���쐬���܂����B({wExpPath})");
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
                argLog.Information($"Csv�t�@�C�����o�͂��܂����B({wCsvPath})");

                #endregion

                #region �摜�̏o��

                //A�� �摜
                const string IMAGEFILE1 = "Image1.jpg";
                //'C�� �摜
                const string IMAGEFILE2 = "Image2.jpg";
                //'E�� �摜
                const string IMAGEFILE3 = "Image3.jpg";
                foreach (var iExtention in new List<string> { "bmp", "jpg" })
                {
                    //Jan13���t��
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
                            argLog.Information($"�摜�t�@�C�����o�͂��܂����B({wExpImgPath})");
                        }
                    }

                    //Jan7��6��
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
                            argLog.Information($"�摜�t�@�C�����o�͂��܂����B({wExpImgPath})");
                        }
                    }
                }
                #endregion

            }

            return true;
        }

        /// <summary>
        /// BMP�t�@�C����JPG�t�@�C���ɕϊ�����
        /// </summary>
        /// <param name="argBmpPath">BMP�t�@�C���p�X</param>
        /// <param name="argJpgPath">JPG�t�@�C���p�X</param>
        /// <returns></returns>
        private async Task CnvBmp2Jpg(string argBmpPath, string argJpgPath)
        {
            #region �`�F�b�N����
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

            #region �ϊ�����                      

            using (IRandomAccessStream stream = await bmpFile.OpenAsync(FileAccessMode.Read))
            {
                // BitmapDecoder���g�p����BMP�t�@�C�����f�R�[�h
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.BmpDecoderId, stream);

                // �f�R�[�h�����摜��SoftwareBitmap�Ƃ��Ď擾
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // JPG�t�@�C���p�̃X�g���[�����J��
                using (IRandomAccessStream jpgStream = await jpgFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // BitmapEncoder���g�p����JPG�t�@�C�����G���R�[�h
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, jpgStream);
                    encoder.SetSoftwareBitmap(softwareBitmap);
                    await encoder.FlushAsync();
                }
            }

            #endregion
        }

        /// <summary>
        /// INI�t�@�C������f�[�^���擾����
        /// </summary>
        /// <param name="argPath">Ini�t�@�C���p�X</param>
        /// <param name="argSection">�Z�N�V������</param>
        /// <param name="argKey">�L�[��</param>
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

                    // �Z�N�V�����̔���
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        if (!wDicData.ContainsKey(currentSection))
                        {
                            wDicData[currentSection] = new Dictionary<string, string>();
                        }
                    }
                    // �L�[-�o�����[�̔���
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
        /// 13���t����Jan�R�[�h�̉摜�p�X���擾����
        /// </summary>
        /// <param name="argTanaImgDir">�I�T�C�G���X�̉摜�t�H���_</param>
        /// <param name="argJanCode">Jan�R�[�h</param>
        /// <param name="argSideType">�摜�ʒu(0:����,1:����,2:���)</param>
        /// <param name="argExtension">�g���q</param>
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
        /// 7��6���ɕ�������Jan�R�[�h�̉摜�p�X���擾����
        /// </summary>
        /// <param name="argTanaImgDir">�I�T�C�G���X�̉摜�t�H���_</param>
        /// <param name="argJanCode">Jan�R�[�h</param>
        /// <param name="argSideType">�摜�ʒu(0:����,1:����,2:���)</param>
        /// <param name="argExtension">�g���q</param>
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
        /// Planet�f�[�^�敪���擾����
        /// </summary>
        /// <param name="argJanCode">JAN�R�[�h</param>
        /// <returns>
        ///  [J],[U],[E]�̂����ꂩ��Ԃ�(null�̏ꍇ�̓G���[)
        /// </returns>
        /// <remarks>
        /// Jan�R�[�h�̌����ɂ���Ĉȉ��̂悤�ɔ��肷��
        ///  8��: �擪�u45�v�A�u49�v�Ȃ�΁uJ�v�A����ȊO�uU�v
        /// 12��: �uU�v
        /// 13��: �擪�u45�v�A�u49�v�Ȃ�΁uJ�v�A����ȊO�uE�v
        /// ��L�ȊO: �G���[
        /// </remarks>
        private string GetPlanetClass(string argJanCode)
        {
            string wRet = null;
            if (string.IsNullOrEmpty(argJanCode))
            {
                return wRet;
            }

            //�����ɂ�锻��
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
        /// Planet�t�H���_�p�X���擾����
        /// </summary>
        /// <param name="argJanCode">JAN�R�[�h</param>
        /// <param name="argBaseDir">Planet�f�[�^�o�͐�e�t�H���_</param>
        /// <returns></returns>
        private string GetPlanetFolderPath(string argJanCode, string argBaseDir)
        {
            string wRet = null;

            var wPlanetClass = GetPlanetClass(argJanCode);
            if (string.IsNullOrEmpty(wPlanetClass))
            {
                //Planet�f�[�^�敪���擾�ł��Ȃ��ꍇ�̓G���[
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
        /// �ݒ�ɂ��R���g���[������
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
