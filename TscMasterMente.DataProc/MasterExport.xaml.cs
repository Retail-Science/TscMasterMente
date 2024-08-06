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
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using TscMasterMente.Common;
using TscMasterMente.Common.MasterFileEntity;
using TscMasterMente.Common.SqlMaps;
using Microsoft.VisualBasic;
using static TscMasterMente.Common.SqliteParts;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MasterExport : Window
    {
        public MasterExport()
        {
            this.InitializeComponent();
            this.Activated += MasterExport_Activated;
        }

        #region �C�x���g

        /// <summary>
        /// Activated�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// �y���l�z
        /// WinUI3��Load�C�x���g���Ȃ����߁AActivated�C�x���g���g�p���ď������������s���B
        /// </remarks>
        private void MasterExport_Activated(object sender, WindowActivatedEventArgs args)
        {
            var clsSql = new SqliteParts();
            TxtMasterPath.Text = clsSql.GetAppInfo("MasterTextDir");

            //�A�v���A�C�R����ݒ�
            WindowParts.SetAppIcon(this);
            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 700);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= MasterExport_Activated;
            
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
        /// �t�H���_�{�^��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnFolder_Click(object sender, RoutedEventArgs e)
        {
            //�t�H���_�I���_�C�A���O�̕\��
            var wPath = await IoParts.PickFolderAsync(this);
            if (wPath != null)
            {
                TxtMasterPath.Text = wPath;
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
            var wProgressWindow = new ProgressWindow();
            string wExecMaster = "";
            try
            {
                //�E�B���h�E�̖�����
                WindowParts.SetAllChildEnabled(MainContent, false);

                //csv�t�@�C���̏o��
                var clsCsv = new CsvHelperParts();

                //sqltscmaster�C���X�^���X
                var clsSql = new SqliteParts();

                //�}�X�^�����擾
                var wMasters = clsSql.GetSelectResult<Masters>("SELECT * FROM Masters");

                #region ���s�O�`�F�b�N

                //�}�X�^�o�͑Ώۃ`�F�b�N���ݒ肳��Ă��邩�`�F�b�N
                if (IsValidCheckedMaster() == false)
                {
                    //Ok�{�^�����b�Z�[�W�{�b�N�X�̕\��
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�}�X�^�o�͑Ώۂ��ݒ肳��Ă��܂���B");
                    await wDialog.ShowAsync();

                    return;
                }

                //�o�͐�t�H���_���ݒ肳��Ă��邩�`�F�b�N
                var wDirPath = TxtMasterPath.Text;
                if (string.IsNullOrEmpty(wDirPath))
                {
                    //Ok�{�^�����b�Z�[�W�{�b�N�X�̕\��
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�o�͐�t�H���_���ݒ肳��Ă��܂���B");
                    await wDialog.ShowAsync();

                    return;
                }
                else if (Directory.Exists(wDirPath) == false)
                {
                    Directory.CreateDirectory(wDirPath);
                }
                clsSql.UpdateAppInfo("MasterTextDir", wDirPath);
                #endregion

                #region �o�͏���

                //�v���O���X�E�B���h�E�̕\��                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("�}�X�^�o�͒�...", GetMasterCount());

                //���[�J�[�}�X�^
                if (ChkMakerMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.MakerMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "���[�J�[�}�X�^";
                        if(await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT * FROM MkMaster ORDER BY MkCode ";
                        var tData = clsSql.GetSelectResult<MkMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<MkMasterEntity>(tPath, tData);
                    }
                }

                //���i�}�X�^
                if (ChkItemMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.SyoMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {

                        wExecMaster = "���i�}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT * FROM SyoMaster ";
                        if (ChkDisposal.IsChecked == true)
                        {
                            tSql += " WHERE (Attr4 IS NULL ";
                            tSql += " OR Attr4 = '' ";
                            tSql += $" OR Attr4 > '{DateTime.Now.ToString("yyyyMMdd")}') ";
                            //tSql += " and mkCode='JTI' ";
                        }
                        tSql += " ORDER BY JanCode ";

                        var tData = clsSql.GetSelectResult<SyoMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<SyoMasterEntity>(tPath, tData);
                    }
                }

                //�ו��ރ}�X�^
                if (ChkSubClassMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.BunMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "�ו��ރ}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT BunCode, BunName ";
                        tSql += " FROM BunMaster ";
                        tSql += " ORDER BY BunCode ";
                        var tData = clsSql.GetSelectResult<BunMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<BunMasterEntity>(tPath, tData);
                    }
                }

                //�����}�X�^
                if (ChkAttributeMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.ZokMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "�����}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT BunCode, ZkCode, ZkName ";
                        tSql += " FROM ZokMaster ";
                        tSql += " ORDER BY ZkCode ";
                        var tData = clsSql.GetSelectResult<ZokMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<ZokMasterEntity>(tPath, tData);
                    }
                }

                //�����}�X�^
                if (ChkLevelMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.SuiMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "�����}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT BunCode, ZkCode, SuiCode, SuiName ";
                        tSql += " FROM SuiMaster ";
                        tSql += " ORDER BY ZkCode, SuiCode ";
                        var tData = clsSql.GetSelectResult<SuiMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<SuiMasterEntity>(tPath, tData);
                    }
                }

                //���i�����}�X�^
                if (ChkItemAttributeMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.SyoZokSei).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "���i�����}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "";
                        if (ChkDisposal.IsChecked == true)
                        {
                            tSql = " SELECT SyoZoksei.JanCode, SyoZoksei.ZkCode, SyoZoksei.SuiCode ";
                            tSql += " FROM SyoZoksei";
                            tSql += " INNER JOIN SyoMaster ON SyoZoksei.JanCode = SyoMaster.JanCode ";
                            tSql += $" AND (SyoMaster.Attr4 IS NULL OR SyoMaster.Attr4 = '' OR SyoMaster.Attr4 > '{DateTime.Now.ToString("yyyyMMdd")}') ";
                            tSql += " ORDER BY SyoZoksei.JanCode, SyoZoksei.ZkCode, SyoZoksei.SuiCode ";
                        }
                        else
                        {
                            tSql = "SELECT JanCode, ZkCode, SuiCode ";
                            tSql += " FROM SyoZokSei ";
                            tSql += " ORDER BY JanCode, ZkCode, SuiCode ";
                        }
                        var tData = clsSql.GetSelectResult<SyoZokSeiEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<SyoZokSeiEntity>(tPath, tData);
                    }
                }

                //POP�}�X�^
                if (ChkPopMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.PopMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "POP�}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT PopCode, PopName, W, H, PBunCode ";
                        tSql += " FROM PopMaster ";
                        tSql += " ORDER BY PopCode";
                        var tData = clsSql.GetSelectResult<PopMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<PopMasterEntity>(tPath, tData);
                    }
                }

                //POP���ރ}�X�^
                if (ChkPopClassMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.PBunMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "POP���ރ}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT PBunCode, PBunName ";
                        tSql += " FROM PBunMaster ";
                        tSql += " ORDER BY PBunCode ";
                        var tData = clsSql.GetSelectResult<PBunMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<PBunMasterEntity>(tPath, tData);
                    }
                }

                //�X�܃}�X�^
                if (ChkStoreMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.TenpoMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "�X�܃}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT ";
                        tSql += " TenpoMaster.TenpoNumber, ";
                        tSql += " TenpoMaster.TenpoCode, ";
                        tSql += " TenpoMaster.TenpoTanName, ";
                        tSql += " TenpoMaster.TenpoName, ";
                        tSql += " CompMaster.CompCode, ";
                        tSql += " CompMaster.CompName, ";
                        tSql += " AreaMaster.AreaCode, ";
                        tSql += " AreaMaster.AreaName ";
                        tSql += " FROM ";
                        tSql += " TenpoMaster ";
                        tSql += " LEFT JOIN CompMaster ON TenpoMaster.CompCode = CompMaster.CompCode ";
                        tSql += " LEFT JOIN AreaMaster ON TenpoMaster.AreaCode = AreaMaster.AreaCode ";
                        tSql += " ORDER BY ";
                        tSql += " TenpoMaster.TenpoNumber ";
                        var tData = clsSql.GetSelectResult<TenpoMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<TenpoMasterEntity>(tPath, tData);
                    }
                }

                //�S���h���Y��}�X�^
                if (ChkGondolaMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.GonJyu).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "�S���h���Y��}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT GonJyuName ";
                        tSql += " FROM GonJyu ";
                        tSql += " ORDER BY GonJyuName ";
                        var tData = clsSql.GetSelectResult<GonJyuEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<GonJyuEntity>(tPath, tData);
                    }
                }

                //�I�i�Y��}�X�^
                if (ChkShelfMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.SlfJyu).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "�I�i�Y��}�X�^";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT SlfJyuName ";
                        tSql += " FROM SlfJyu ";
                        tSql += " ORDER BY SlfJyuName ";
                        var tData = clsSql.GetSelectResult<SlfJyuEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<SlfJyuEntity>(tPath, tData);
                    }
                }

                #endregion

                wSucceed = true;

            }
            catch (Exception ex)
            {
                //�G���[���b�Z�[�W�̕\��
                var errMsg = $"{wExecMaster }�}�X�^�o�͒��ɃG���[���������܂����B";
                errMsg +="\r\n";
                errMsg += "�y�ڍׁz";
                errMsg += "\r\n";
                errMsg += ex.Message;

                var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", errMsg);
                await wDialog.ShowAsync();

                wSucceed = false;
            }
            finally
            {
                //�v���O���X�E�B���h�E�̃N���[�Y
                wProgressWindow.Close();
                wProgressWindow = null;

                //�E�B���h�E�̗L����
                WindowParts.SetAllChildEnabled(MainContent, true);

                if (wSucceed)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "����", "�}�X�^�o�͂��������܂����B");
                    await wDialog.ShowAsync();
                }
            }
        }

        #endregion

        #region ���\�b�h

        #region �X�e�[�^�X�o�[

        /// <summary>
        /// �o�̓}�X�^�����擾
        /// </summary>
        /// <returns></returns>
        private int GetMasterCount()
        {
            int wMasterCount = 0;
            foreach (var child in GrdMasterCheckBoxes.Children)
            {
                if (child is CheckBox chk && chk.IsChecked == true)
                {
                    wMasterCount++;
                }
            }
            if (wMasterCount == 0) wMasterCount = 100;

            return wMasterCount;
        }

        #endregion

        #region ���s


        /// <summary>
        /// �}�X�^�o�͑Ώۃ`�F�b�N���ݒ肳��Ă��邩�`�F�b�N
        /// </summary>
        /// <returns></returns>
        private bool IsValidCheckedMaster()
        {
            foreach (var child in GrdMasterCheckBoxes.Children)
            {
                switch (child)
                {
                    case CheckBox checkBox:
                        // CheckBox�ɑ΂��鏈��
                        if (checkBox.IsChecked == true)
                        {
                            return true;
                        }
                        break;
                    case ComboBox comboBox:
                        break;
                }
            }

            return false;
        }

        #endregion

        #endregion
    }


}
