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
using TscMasterMente.Common.MasterFileEntity;
using TscMasterMente.Common.SqlMaps;
using static System.Net.WebRequestMethods;
using ABI.Windows.Media.Protection.PlayReady;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Shapes;
using ABI.Windows.Web.Syndication;
using System.Diagnostics;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MasterImport : Window
    {
        public MasterImport()
        {
            this.InitializeComponent();
            this.Activated += MasterImport_Activated;
        }

        #region �C�x���g

        /// <summary>
        /// Activeted�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// �y���l�z
        /// WinUI3��Load�C�x���g���Ȃ����߁AActivated�C�x���g���g�p���ď������������s���B
        /// </remarks>
        private void MasterImport_Activated(object sender, WindowActivatedEventArgs args)
        {
            var clsSql = new SqliteParts();
            TxtMasterPath.Text = clsSql.GetAppInfo("MasterTextDir");

            //�A�v���A�C�R����ݒ�
            WindowParts.SetAppIcon(this);
            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 560);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= MasterImport_Activated;
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
        /// �t�H���_�I���{�^��
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

                var clsSql = new SqliteParts();

                #region �`�F�b�N����

                //txtMasterPath�̃`�F�b�N
                var wDirPath = TxtMasterPath.Text;
                if (string.IsNullOrEmpty(wDirPath))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�捞��t�H���_���ݒ肳��Ă��܂���B");
                    await wDialog.ShowAsync();

                    return;
                }
                else if (!Directory.Exists(wDirPath))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�捞��t�H���_�����݂��܂���B");
                    await wDialog.ShowAsync();
                    return;
                }                

                //�}�X�^�t�@�C���̎擾
                var wMasters = clsSql.GetSelectResult<Masters>("SELECT * FROM Masters");
                var wFiles = await IoParts.GetMasterFiles(wDirPath);

                if (wFiles.Count > 0)
                {

                    //�}�X�^�t�@�C�����̎擾
                    var lqFiles = from x in wFiles
                                  join y in wMasters on x.DisplayName + x.FileType.ToLower() equals y.MasterFileName
                                  where new FileInfo(x.Path).Length > 0
                                  select x;

                    wFiles = lqFiles.ToList();

                }

                if (wFiles.Count == 0)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�}�X�^�t�@�C�������݂��܂���B");
                    await wDialog.ShowAsync();
                    return;
                }
                #endregion

                #region �捞�m�F

                int wMasterCount = 0;
                var wMsg = "�ȉ��̃}�X�^�t�@�C����������܂����B�����̃}�X�^���捞�݂܂����H";
                wMsg += Environment.NewLine;
                wMsg += "�y�Q�Ɛ�p�X�z";
                wMsg += Environment.NewLine;
                wMsg += wDirPath;
                wMsg += Environment.NewLine;
                wMsg += "�y�}�X�^�t�@�C���z";
                foreach (var iFile in wFiles)
                {
                    //Masters����t�@�C��������
                    var lqMaster = wMasters.Where(x => x.MasterFileName == iFile.DisplayName + iFile.FileType.ToLower()).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wMsg += Environment.NewLine;
                        wMsg += $"�E{lqMaster.MasterName} ({lqMaster.MasterFileName})";
                        wMasterCount++;
                    }
                }

                var wDialogMsg = MessageParts.ShowMessageYesNo(this, "�m�F", wMsg);
                var wDialogResult = await wDialogMsg.ShowAsync();
                if (wDialogResult == ContentDialogResult.Secondary)
                {
                    return;
                }

                #endregion

                #region �}�X�^�t�@�C���̎捞

                //�v���O���X�E�B���h�E�̕\��                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("�}�X�^�捞��...", wMasterCount);

                clsSql.UpdateAppInfo("MasterTextDir", wDirPath);
                using (var conn = clsSql.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var wAlertList = new List<string>();
                            foreach (var iFile in wFiles)
                            {
                                #region �t�@�C��������}�X�^�����擾

                                //Masters����t�@�C��������
                                int wMasterCode = 0;
                                string wErrFileName = "";
                                var lqMaster = wMasters.Where(x => x.MasterFileName == iFile.DisplayName + iFile.FileType.ToLower()).FirstOrDefault();
                                if (lqMaster != null)
                                {
                                    wMasterCode = lqMaster.MasterCode;
                                    wExecMaster = lqMaster.MasterName;
                                    wErrFileName = lqMaster.ErrFileName;

                                    if (await wProgressWindow.SetProgress(wExecMaster) == false)
                                    {
                                        var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                                        await wDialog.ShowAsync();
                                        return;
                                    }
                                }

                                #endregion

                                #region �}�X�^�捞����

                                switch (wMasterCode)
                                {
                                    case (int)SqliteParts.EnumMasterInfo.MakerMaster:
                                        //���[�J�[�}�X�^�̎捞
                                        if (!ImportMkMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.SyoMaster:
                                        //���i�}�X�^�̎捞
                                        if (!ImportSyoMaser(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.BunMaster:
                                        //�ו��ރ}�X�^�̎捞
                                        if (!ImportBunMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.ZokMaster:
                                        //�����}�X�^�̎捞
                                        if (!ImportZokMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.SuiMaster:
                                        //�����}�X�^�̎捞
                                        if (!ImportSuiMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.SyoZokSei:
                                        //���i�����}�X�^�̎捞
                                        if (!ImportSyoZokSei(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.PopMaster:
                                        //POP�}�X�^�̎捞
                                        if (!ImportPopMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.PBunMaster:
                                        //POP���ރ}�X�^�̎捞
                                        if (!ImportPBunMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.TenpoMaster:
                                        //�X�܃}�X�^�̎捞
                                        if (!ImportTenpoMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.GonJyu:
                                        //�S���h���Y��̎捞
                                        if (!ImportGonJyu(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.SlfJyu:
                                        //�I�i�Y��̎捞
                                        if (!ImportSlfJyu(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //�G���[�f�[�^����
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                #endregion
                            }
                            tran.Commit();

                            #region ���b�Z�[�W����

                            //���b�Z�[�W
                            if (wAlertList.Count == 0)
                            {
                                wSucceed = true;
                            }
                            else
                            {
                                string alertMsg = "�f�[�^�̎捞�͎��s���܂������A�捞�Ɏ��s�����f�[�^������܂����B";
                                alertMsg += Environment.NewLine;
                                alertMsg += "�G���[���X�g�Ƃ��ďo�͂��܂����̂ŁA���e�����m�F���������B";
                                alertMsg += Environment.NewLine;
                                alertMsg += "�y�o�͐�p�X�z";
                                alertMsg += Environment.NewLine;
                                alertMsg += wDirPath;
                                alertMsg += Environment.NewLine;
                                alertMsg += "�y�G���[�t�@�C�����X�g�z";
                                foreach (var iAlert in wAlertList)
                                {
                                    alertMsg += Environment.NewLine;
                                    alertMsg += $"�E{iAlert}";
                                }
                                var wMsgDialog = MessageParts.ShowMessageOkOnly(this, "����", alertMsg);
                                await wMsgDialog.ShowAsync();

                            }
                            #endregion

                        }
                        catch (Exception)
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                //�G���[���b�Z�[�W�̕\��
                var errMsg = $"{wExecMaster}�捞���ɃG���[���������܂����B";
                errMsg += "\r\n";
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
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "����", "�}�X�^�̎捞�������������܂����B");
                    await wDialog.ShowAsync();
                }
            }
        }


        #endregion

        #region ���\�b�h

        #region �}�X�^�捞���̋��ʃ��\�b�h

        /// <summary>
        /// �Ǎ��}�X�^�t�@�C������}�X�^�G���[�t�@�C���̃p�X���擾
        /// </summary>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private string GetMasterErrFilePath(string argErrFileName)
        {
            return System.IO.Path.Combine(TxtMasterPath.Text, argErrFileName);
        }

        /// <summary>
        /// CsvHelper�œǍ����ɃG���[�ɂȂ����f�[�^���G���[�t�@�C���ɏo��
        /// </summary>
        /// <param name="argPath">�o�͐�G���[�t�@�C���p�X</param>
        /// <param name="argData">�G���[�f�[�^</param>
        private void WriteCsvHelperConvertErrData(string argPath, List<string> argData)
        {
            using (StreamWriter wWrite = new StreamWriter(argPath, true, Encoding.GetEncoding("shift_jis")))
            {
                foreach (var iData in argData)
                {
                    wWrite.WriteLine(iData);
                }
            }
        }

        #endregion

        #region �}�X�^�捞���̌ʃ��\�b�h

        /// <summary>
        /// ���[�J�[�}�X�^�̎捞
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportMkMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<MkMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<MkMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N


                    //���[�J�[�R�[�h�`�F�b�N
                    if (string.IsNullOrEmpty(iData.MkCode) || iData.MkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���[�J�[���`�F�b�N
                    if (!string.IsNullOrEmpty(iData.MkName) && iData.MkName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���[�J�[���J�i�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.MkName_Kana) && iData.MkName_Kana.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@MkCode", iData.MkCode),
                        new SqliteParameter("@MkName", iData.MkName),
                        new SqliteParameter("@MkName_Kana", iData.MkName_Kana)
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO MkMaster (MkCode, MkName, MkName_Kana) VALUES (@MkCode , @MkName, @MkName_Kana)";
                        tSql += " ON CONFLICT(MkCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //�ǉ��X�V
                        var tSql = $"INSERT INTO MkMaster (MkCode, MkName, MkName_Kana) VALUES (@MkCode , @MkName, @MkName_Kana)";
                        tSql += $" ON CONFLICT(MkCode) DO UPDATE SET MkName = @MkName , MkName_Kana = @MkName_Kana ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM MkMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO MkMaster (MkCode, MkName, MkName_Kana) VALUES (@MkCode , @MkName, @MkName_Kana)";
                        tSql += $" ON CONFLICT(MkCode) DO UPDATE SET MkName = @MkName , MkName_Kana = @MkName_Kana ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<MkMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// ���i�}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportSyoMaser(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;
            var clsCsv = new CsvHelperParts();
            const int MAX_WDH = 9999;
            const int MIN_WDH = 0;

            var dtFileData = clsCsv.ReadMasterFile<SyoMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<SyoMasterEntity>();
            if (dtMaster.Count() > 0)
            {
                string wExecDate = DateTime.Now.ToString("yyyyMMdd");
                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N

                    //Jan�R�[�h�`�F�b�N
                    if (string.IsNullOrEmpty(iData.JanCode) || iData.JanCode.Length > 16)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���[�J�R�[�h�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.MkCode) && iData.MkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���i�R�[�h�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.StoCode) && iData.StoCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���i���J�i�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.TanName) && iData.TanName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���i���`�F�b�N
                    if (!string.IsNullOrEmpty(iData.ItemName) && iData.ItemName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���`�F�b�N
                    if (iData.W == null)
                    {
                        iData.W = 100;
                    }
                    else if (iData.W < MIN_WDH || iData.W > MAX_WDH)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���s�`�F�b�N
                    if (iData.D == null)
                    {
                        iData.D = 100;
                    }
                    else if (iData.D < MIN_WDH || iData.D > MAX_WDH)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����`�F�b�N
                    if (iData.H == null)
                    {
                        iData.H = 100;
                    }
                    else if (iData.H < MIN_WDH || iData.H > MAX_WDH)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����`�F�b�N
                    if (iData.PCase == null)
                    {
                        iData.PCase = 0;
                    }
                    else if (iData.PCase < 0 || iData.PCase > 99999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //���ރR�[�h�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.BunCode) && iData.BunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����`�F�b�N
                    if (iData.Price == null)
                    {
                        iData.Price = 0;
                    }
                    else if (iData.Price < 0 || iData.Price > 9999999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����`�F�b�N
                    if (iData.Cost == null)
                    {
                        iData.Cost = 0;
                    }
                    else if (iData.Cost < 0 || iData.Cost > 9999999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�o�^���`�F�b�N
                    if (!string.IsNullOrEmpty(iData.Attr3) && iData.Attr3.Length > 8)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�X�V���`�F�b�N
                    if (!string.IsNullOrEmpty(iData.Attr5) && iData.Attr5.Length > 8)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@JanCode", iData.JanCode),
                        new SqliteParameter("@MkCode", iData.MkCode),
                        new SqliteParameter("@StoCode", iData.StoCode),
                        new SqliteParameter("@TanName", iData.TanName),
                        new SqliteParameter("@ItemName", iData.ItemName),
                        new SqliteParameter("@W", iData.W),
                        new SqliteParameter("@D", iData.D),
                        new SqliteParameter("@H", iData.H),
                        new SqliteParameter("@PCase", iData.PCase),
                        new SqliteParameter("@BunCode", iData.BunCode),
                        new SqliteParameter("@Price", iData.Price),
                        new SqliteParameter("@Cost", iData.Cost),
                        new SqliteParameter("@Attr3", iData.Attr3),
                        new SqliteParameter("@Attr5", iData.Attr5)
                    };
                    foreach (var iParam in wParams)
                    {
                        switch (iParam.ParameterName)
                        {
                            case "@Attr3":
                            case "@Attr5":
                                if (argSqlCls.IsNullData(iParam.Value))
                                {
                                    iParam.Value = wExecDate;
                                }
                                break;
                            default:
                                if (argSqlCls.IsNullData(iParam.Value))
                                {
                                    iParam.Value = DBNull.Value;
                                }
                                break;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO SyoMaster ";
                        tSql += $"(JanCode, MkCode, StoCode, TanName, ItemName, W, H, D, PCase, BunCode, Price, Cost, Attr3,  Attr5) ";
                        tSql += $" VALUES ";
                        tSql += $"(@JanCode, @MkCode, @StoCode, @TanName, @ItemName, @W, @H, @D, @PCase, @BunCode, @Price, @Cost, @Attr3, @Attr5) ";
                        tSql += $" ON CONFLICT(JanCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //�ǉ��X�V
                        var tSql = $"INSERT INTO SyoMaster ";
                        tSql += $" (JanCode, MkCode, StoCode, TanName, ItemName, W, H, D, PCase, BunCode, Price, Cost, Attr3,  Attr5) ";
                        tSql += $" VALUES ";
                        tSql += $" (@JanCode, @MkCode, @StoCode, @TanName, @ItemName, @W, @H, @D, @PCase, @BunCode, @Price, @Cost, @Attr3, @Attr5) ";
                        tSql += $" ON CONFLICT(JanCode) DO UPDATE ";
                        tSql += $" SET MkCode = @MkCode, StoCode = @StoCode, TanName = @TanName, ItemName = @ItemName, ";
                        tSql += $" W = @W , H = @H , D = @D , PCase = @PCase , BunCode = @BunCode , Price = @Price , Cost = @Cost , Attr3 = @Attr3 , Attr5 = @Attr5 ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM SyoMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO SyoMaster ";
                        tSql += $" (JanCode, MkCode, StoCode, TanName, ItemName, W, H, D, PCase, BunCode, Price, Cost, Attr3,  Attr5) ";
                        tSql += $" VALUES ";
                        tSql += $" (@JanCode, @MkCode, @StoCode, @TanName, @ItemName, @W, @H, @D, @PCase, @BunCode, @Price, @Cost, @Attr3, @Attr5) ";
                        tSql += $" ON CONFLICT(JanCode) DO UPDATE ";
                        tSql += $" SET MkCode = @MkCode, StoCode = @StoCode, TanName = @TanName, ItemName = @ItemName, ";
                        tSql += $" W = @W , H = @H , D = @D , PCase = @PCase , BunCode = @BunCode , Price = @Price , Cost = @Cost , Attr3 = @Attr3 , Attr5 = @Attr5 ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region �G���[����
            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<SyoMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;

        }

        /// <summary>
        /// �ו��ރ}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportBunMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<BunMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<BunMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N


                    //�ו��ރR�[�h�`�F�b�N
                    if (string.IsNullOrEmpty(iData.BunCode) || iData.BunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�ו��ޖ��`�F�b�N
                    if (!string.IsNullOrEmpty(iData.BunName) && iData.BunName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@BunCode", iData.BunCode),
                        new SqliteParameter("@BunName", iData.BunName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO BunMaster (BunCode, BunName) VALUES (@BunCode , @BunName) ";
                        tSql += " ON CONFLICT(BunCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //�ǉ��X�V
                        var tSql = $"INSERT INTO BunMaster (BunCode, BunName) VALUES (@BunCode , @BunName) ";
                        tSql += $" ON CONFLICT(BunCode) DO UPDATE SET BunName = @BunName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM BunMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO BunMaster (BunCode, BunName) VALUES (@BunCode , @BunName) ";
                        tSql += $" ON CONFLICT(BunCode) DO UPDATE SET BunName = @BunName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<BunMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// �����}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportZokMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<ZokMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<ZokMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N


                    //�ו��ރR�[�h�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.BunCode) && iData.BunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����R�[�h�`�F�b�N
                    if (string.IsNullOrEmpty(iData.ZkCode) || iData.ZkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�������`�F�b�N
                    if (!string.IsNullOrEmpty(iData.ZkName) && iData.ZkName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@BunCode", iData.BunCode),
                        new SqliteParameter("@ZkCode", iData.ZkCode),
                        new SqliteParameter("@ZkName", iData.ZkName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO ZokMaster (BunCode, ZkCode, ZkName) VALUES (@BunCode , @ZkCode, @ZkName) ";
                        tSql += " ON CONFLICT(ZkCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //�ǉ��X�V
                        var tSql = $"INSERT INTO ZokMaster (BunCode, ZkCode, ZkName) VALUES (@BunCode , @ZkCode, @ZkName) ";
                        tSql += $" ON CONFLICT(ZkCode) DO UPDATE SET ZkName = @ZkName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM ZokMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO ZokMaster (BunCode, ZkCode, ZkName) VALUES (@BunCode , @ZkCode, @ZkName) ";
                        tSql += $" ON CONFLICT(ZkCode) DO UPDATE SET ZkName = @ZkName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<ZokMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// �����}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportSuiMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<SuiMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<SuiMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N


                    //�ו��ރR�[�h�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.BunCode) && iData.BunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����R�[�h�`�F�b�N
                    if (string.IsNullOrEmpty(iData.ZkCode) || iData.ZkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����R�[�h�`�F�b�N
                    if (string.IsNullOrEmpty(iData.SuiCode) || iData.SuiCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�������`�F�b�N
                    if (!string.IsNullOrEmpty(iData.SuiName) && iData.SuiName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@BunCode", iData.BunCode),
                        new SqliteParameter("@ZkCode", iData.ZkCode),
                        new SqliteParameter("@SuiCode", iData.SuiCode),
                        new SqliteParameter("@SuiName", iData.SuiName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO SuiMaster (BunCode, ZkCode, SuiCode, SuiName) VALUES (@BunCode , @ZkCode, @SuiCode, @SuiName) ";
                        tSql += " ON CONFLICT(ZkCode, SuiCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //�ǉ��X�V
                        var tSql = $"INSERT INTO SuiMaster (BunCode, ZkCode, SuiCode, SuiName) VALUES (@BunCode , @ZkCode, @SuiCode, @SuiName) ";
                        tSql += $" ON CONFLICT(ZkCode, SuiCode) DO UPDATE SET SuiName = @SuiName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM SuiMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO SuiMaster (BunCode, ZkCode, SuiCode, SuiName) VALUES (@BunCode , @ZkCode, @SuiCode, @SuiName) ";
                        tSql += $" ON CONFLICT(ZkCode, SuiCode) DO UPDATE SET SuiName = @SuiName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<SuiMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// ���i�����}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportSyoZokSei(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<SyoZokSeiEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<SyoZokSeiEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N

                    //JanCode�`�F�b�N
                    if (string.IsNullOrEmpty(iData.JanCode) || iData.JanCode.Length > 16)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����R�[�h�`�F�b�N
                    if (string.IsNullOrEmpty(iData.ZkCode) || iData.ZkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //�����R�[�h�`�F�b�N
                    if (string.IsNullOrEmpty(iData.SuiCode) || iData.SuiCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@JanCode", iData.JanCode),
                        new SqliteParameter("@ZkCode", iData.ZkCode),
                        new SqliteParameter("@SuiCode", iData.SuiCode),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true || RbMerge.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO SyoZokSei (JanCode, ZkCode, SuiCode) VALUES (@JanCode , @ZkCode, @SuiCode) ";
                        tSql += " ON CONFLICT(JanCode, ZkCode, SuiCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM SyoZokSei", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO SyoZokSei (JanCode, ZkCode, SuiCode) VALUES (@JanCode , @ZkCode, @SuiCode) ";
                        tSql += " ON CONFLICT(JanCode, ZkCode, SuiCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<SyoZokSeiEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// POP�}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportPopMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<PopMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<PopMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N

                    //PopCode�`�F�b�N
                    if (string.IsNullOrEmpty(iData.PopCode) || iData.PopCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //PopName�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.PopName) && iData.PopName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //W�`�F�b�N
                    if (iData.W == null)
                    {
                        iData.W = 100;
                    }
                    else if (iData.W < 0 || iData.W > 9999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //H�`�F�b�N
                    if (iData.H == null)
                    {
                        iData.H = 100;
                    }
                    else if (iData.H < 0 || iData.H > 9999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //PBunCode�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.PBunCode) && iData.PBunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@PopCode", iData.PopCode),
                        new SqliteParameter("@PopName", iData.PopName),
                        new SqliteParameter("@W", iData.W),
                        new SqliteParameter("@H", iData.H),
                        new SqliteParameter("@PBunCode", iData.PBunCode),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO PopMaster (PopCode, PopName, W, H, PBunCode) VALUES (@PopCode , @PopName, @W, @H, @PBunCode) ";
                        tSql += " ON CONFLICT(PopCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //�ǉ��X�V
                        var tSql = $"INSERT INTO PopMaster (PopCode, PopName, W, H, PBunCode) VALUES (@PopCode , @PopName, @W, @H, @PBunCode) ";
                        tSql += $" ON CONFLICT(PopCode) DO UPDATE SET PopName = @PopName, W = @W, H = @H, PBunCode = @PBunCode";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM PopMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO PopMaster (PopCode, PopName, W, H, PBunCode) VALUES (@PopCode , @PopName, @W, @H, @PBunCode) ";
                        tSql += $" ON CONFLICT(PopCode) DO UPDATE SET PopName = @PopName, W = @W, H = @H, PBunCode = @PBunCode";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<PopMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// Pop���ރ}�X�^
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportPBunMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<PBunMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<PBunMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N


                    //PBunCode�`�F�b�N
                    if (string.IsNullOrEmpty(iData.PBunCode) || iData.PBunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //PBunName�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.PBunName) && iData.PBunName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@PBunCode", iData.PBunCode),
                        new SqliteParameter("@PBunName", iData.PBunName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO PBunMaster (PBunCode, PBunName) VALUES (@PBunCode , @PBunName) ";
                        tSql += " ON CONFLICT(PBunCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //�ǉ��X�V
                        var tSql = $"INSERT INTO PBunMaster (PBunCode, PBunName) VALUES (@PBunCode , @PBunName) ";
                        tSql += $" ON CONFLICT(PBunCode) DO UPDATE SET PBunName = @PBunName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM PBunMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO PBunMaster (PBunCode, PBunName) VALUES (@PBunCode , @PBunName) ";
                        tSql += $" ON CONFLICT(PBunCode) DO UPDATE SET PBunName = @PBunName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<PBunMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// �X�܃}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportTenpoMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<TenpoMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<TenpoMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N

                    //TenpoNumber�`�F�b�N
                    if (string.IsNullOrEmpty(iData.TenpoNumber) || iData.TenpoNumber.Length > 10)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //TenpoCode�`�F�b�N
                    if (string.IsNullOrEmpty(iData.TenpoCode) || iData.TenpoCode.Length > 10)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //TenpoTanName�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.TenpoTanName) && iData.TenpoTanName.Length > 50)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //TenpoName�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.TenpoName) && iData.TenpoName.Length > 50)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //CompCode�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.CompCode) && iData.CompCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //CompName�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.CompName) && iData.CompName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //AreaCode�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.AreaCode) && iData.AreaCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //AreaName�`�F�b�N
                    if (!string.IsNullOrEmpty(iData.AreaName) && iData.AreaName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }



                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@TenpoNumber", iData.TenpoNumber),
                        new SqliteParameter("@TenpoCode", iData.TenpoCode),
                        new SqliteParameter("@TenpoTanName", iData.TenpoTanName),
                        new SqliteParameter("@TenpoName", iData.TenpoName),
                        new SqliteParameter("@CompCode", iData.CompCode),
                        new SqliteParameter("@CompName", iData.CompName),
                        new SqliteParameter("@AreaCode", iData.AreaCode),
                        new SqliteParameter("@AreaName", iData.AreaName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�                        

                        //�X�܃}�X�^
                        var tSql = $"INSERT INTO TenpoMaster (TenpoNumber, TenpoCode, TenpoTanName, TenpoName, CompCode, CompName, AreaCode, AreaName) ";
                        tSql += " VALUES (@TenpoNumber, @TenpoCode, @TenpoTanName, @TenpoName, @CompCode, @CompName, @AreaCode, @AreaName) ";
                        tSql += " ON CONFLICT(TenpoNumber) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //�G���A�}�X�^
                        tSql = $"INSERT INTO AreaMaster (AreaCode, AreaName) ";
                        tSql += " VALUES (@AreaCode, @AreaName) ";
                        tSql += " ON CONFLICT(AreaCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //�@�l�}�X�^
                        tSql = $"INSERT INTO CompMaster (CompCode, CompName) ";
                        tSql += " VALUES (@CompCode, @CompName) ";
                        tSql += " ON CONFLICT(CompCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //�ǉ��X�V

                        //�X�܃}�X�^
                        var tSql = $"INSERT INTO TenpoMaster (TenpoNumber, TenpoCode, TenpoTanName, TenpoName, CompCode, CompName, AreaCode, AreaName) ";
                        tSql += " VALUES (@TenpoNumber, @TenpoCode, @TenpoTanName, @TenpoName, @CompCode, @CompName, @AreaCode, @AreaName) ";
                        tSql += $" ON CONFLICT(TenpoNumber) DO UPDATE SET ";
                        tSql += " TenpoCode = @TenpoCode, TenpoTanName = @TenpoTanName, TenpoName = @TenpoName ";
                        tSql += " ,CompCode = @CompCode, CompName = @CompName, AreaCode = @AreaCode, AreaName = @AreaName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //�G���A�}�X�^
                        tSql = $"INSERT INTO AreaMaster (AreaCode, AreaName) ";
                        tSql += " VALUES (@AreaCode, @AreaName) ";
                        tSql += $" ON CONFLICT(AreaCode) DO UPDATE SET AreaName = @AreaName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //�@�l�}�X�^
                        tSql = $"INSERT INTO CompMaster (CompCode, CompName) ";
                        tSql += " VALUES (@CompCode, @CompName) ";
                        tSql += $" ON CONFLICT(CompCode) DO UPDATE SET CompName = @CompName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM TenpoMaster", null);
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM AreaMaster", null);
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM CompMaster", null);
                            wDeleted = true;
                        }

                        //�X�܃}�X�^
                        var tSql = $"INSERT INTO TenpoMaster (TenpoNumber, TenpoCode, TenpoTanName, TenpoName, CompCode, CompName, AreaCode, AreaName) ";
                        tSql += " VALUES (@TenpoNumber, @TenpoCode, @TenpoTanName, @TenpoName, @CompCode, @CompName, @AreaCode, @AreaName) ";
                        tSql += $" ON CONFLICT(TenpoNumber) DO UPDATE SET ";
                        tSql += " TenpoCode = @TenpoCode, TenpoTanName = @TenpoTanName, TenpoName = @TenpoName ";
                        tSql += " ,CompCode = @CompCode, CompName = @CompName, AreaCode = @AreaCode, AreaName = @AreaName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //�G���A�}�X�^
                        tSql = $"INSERT INTO AreaMaster (AreaCode, AreaName) ";
                        tSql += " VALUES (@AreaCode, @AreaName) ";
                        tSql += $" ON CONFLICT(AreaCode) DO UPDATE SET AreaName = @AreaName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //�@�l�}�X�^
                        tSql = $"INSERT INTO CompMaster (CompCode, CompName) ";
                        tSql += " VALUES (@CompCode, @CompName) ";
                        tSql += $" ON CONFLICT(CompCode) DO UPDATE SET CompName = @CompName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<TenpoMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            #region �X�ܔԍ��̐U�蒼��
            var dtTenpo = argSqlCls.GetSelectResult<TenpoMaster>(argConn, "SELECT * FROM TenpoMaster ORDER BY TenpoCode");
            var wTenpoId = 0;
            foreach (var iData in dtTenpo)
            {
                wTenpoId++;
                var tSql = $"UPDATE TenpoMaster SET TenpoNumber = '{wTenpoId.ToString("000")}' WHERE TenpoId = '{iData.TenpoCode}'";
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// �S���h���Y��}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportGonJyu(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<GonJyuEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<GonJyuEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N

                    //GonJyuName�`�F�b�N
                    if (string.IsNullOrEmpty(iData.GonJyuName) || iData.GonJyuName.Length > 50)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@GonJyuName", iData.GonJyuName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true || RbMerge.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO GonJyu (GonJyuName) VALUES (@GonJyuName) ";
                        tSql += " ON CONFLICT(GonJyuName) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM GonJyu", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO GonJyu (GonJyuName) VALUES (@GonJyuName) ";
                        tSql += " ON CONFLICT(GonJyuName) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<GonJyuEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// �I�i�Y��}�X�^�̎捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argPath">�}�X�^�t�@�C���p�X</param>
        /// <param name="argErrFileName">�G���[�t�@�C����</param>
        /// <returns></returns>
        private bool ImportSlfJyu(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<SlfJyuEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<SlfJyuEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region �`�F�b�N

                    //SlfJyuName�`�F�b�N
                    if (string.IsNullOrEmpty(iData.SlfJyuName) || iData.SlfJyuName.Length > 50)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region �p�����^�ݒ�

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@SlfJyuName", iData.SlfJyuName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL���s

                    if (RbInsertOnly.IsChecked == true || RbMerge.IsChecked == true)
                    {
                        //���o�^�̂ݒǉ�
                        var tSql = $"INSERT INTO SlfJyu (SlfJyuName) VALUES (@SlfJyuName) ";
                        tSql += " ON CONFLICT(SlfJyuName) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //�폜���ǉ�����
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM SlfJyu", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO SlfJyu (SlfJyuName) VALUES (@SlfJyuName) ";
                        tSql += " ON CONFLICT(SlfJyuName) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region �G���[����

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<SlfJyuEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        #endregion

        #endregion

    }
}
