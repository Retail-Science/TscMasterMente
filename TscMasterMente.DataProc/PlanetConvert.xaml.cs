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
            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 800);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            //Planet�`������f�[�^�ϊ����W�I�{�^����I��
            RbExport.IsChecked = true;

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
                }
                else if(RbExport.IsChecked == true)
                {
                    //Planet�`���փf�[�^�ϊ�
                }
                else
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "���������s�ł��܂���ł����B");
                    await tMsgDialog.ShowAsync();
                }

                #endregion

            }
            catch (Exception ex)
            {
                var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", ex.Message);
                await tMsgDialog.ShowAsync();
            }
            finally
            {
                //�v���O���X�E�B���h�E�̃N���[�Y
                wProgressWindow.Close();
                wProgressWindow = null;

                //�E�B���h�E�̗L����
                WindowParts.SetAllChildEnabled(MainContent, true);
            }

            GetIniFileData(@"C:\Tsc\TANA.ini", "TANA", "SBMP");

            string bmpFilePath = @"C:\Nisprg\Sih\TanaC\bmp\0000045\214233A.bmp";
            string jpgFilePath = @"C:\Nisprg\Sih\TanaC\bmp\0000045\214233A.jpg";
            await CnvBmp2Jpg(bmpFilePath, jpgFilePath);
        }


        #endregion

        #region ���\�b�h

        private async void CnvTana2Planet(ProgressWindow argProgWindow)
        {
            var clsSql = new SqliteParts();
            var clsCsv = new CsvHelperParts();

            string wSql = "SELECT * FROM SyoMaster order by JanCode ";
            var dtSyoMaster = clsSql.GetSelectResult<SyoMaster>(wSql);

            string wExpDir = TxtExportPath.Text;
            await argProgWindow.SetInit("�v���l�b�g�f�[�^�ϊ���...", dtSyoMaster.Count);
            foreach(var iSyoMaster in dtSyoMaster)
            {
                //�v���O���X�E�B���h�E�̍X�V
                await argProgWindow.SetProgress(iSyoMaster.JanCode);

                //Planet�R���o�[�g��t�H���_�p�X�̎擾
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
                foreach (var line in File.ReadAllLines(argPath))
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
        /// 7��6���ɕ�������Jan�R�[�h�̉摜�p�X���擾����
        /// </summary>
        /// <param name="argTanaImgDir">�I�T�C�G���X�̉摜�t�H���_</param>
        /// <param name="argJanCode">Jan�R�[�h</param>
        /// <param name="argSideType">�摜�ʒu(0:����,1:����,2:���)</param>
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
