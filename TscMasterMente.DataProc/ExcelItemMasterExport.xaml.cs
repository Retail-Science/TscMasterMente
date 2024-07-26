using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TscMasterMente.Common.SqlMaps;
using TscMasterMente.Common;
using System.Diagnostics;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Reflection.Emit;
using TscMasterMente.Common.MasterFileEntity;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExcelItemMasterExport : Window
    {
        #region ListViews�f�[�^�\�[�X
        
        /// <summary>
        /// ���[�J�[��`���X�g
        /// </summary>
        public ObservableCollection<MkMaster> MakerDef { get; } = new ObservableCollection<MkMaster>();
        /// <summary>
        /// ���[�J�[�I�����X�g
        /// </summary>
        public ObservableCollection<MkMaster> MakerSel { get; } = new ObservableCollection<MkMaster>();
        /// <summary>
        /// ���ޒ�`���X�g
        /// </summary>
        public ObservableCollection<BunMaster> BunDef { get; } = new ObservableCollection<BunMaster>();
        /// <summary>
        /// ���ޑI�����X�g
        /// </summary>
        public ObservableCollection<BunMaster> BunSel { get; } = new ObservableCollection<BunMaster>();

        #endregion

        #region �R���X�g���N�^

        public ExcelItemMasterExport()
        {
            this.InitializeComponent();

            var clsSql = new SqliteParts();

            //���[�J�[�}�X�^�擾
            var wMkMasters = clsSql.GetSelectResult<MkMaster>("SELECT * FROM MkMaster ORDER BY MkCode");
            foreach (var item in wMkMasters)
            {
                MakerDef.Add(item);
            }

            //���ރ}�X�^�擾
            var wBunMasters = clsSql.GetSelectResult<BunMaster>("SELECT * FROM BunMaster ORDER BY BunCode");
            foreach (var item in wBunMasters)
            {
                BunDef.Add(item);
            }   

            this.Activated += ExcelItemMasterExport_Activated;
        }

        #endregion

        #region �C�x���g

        /// <summary>
        /// Activated�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// �y���l�z
        /// WinUI3��Load�C�x���g���Ȃ����߁AActivated�C�x���g���g�p���ď������������s���B
        /// </remarks>
        private void ExcelItemMasterExport_Activated(object sender, WindowActivatedEventArgs e)
        {
            var clsSql = new SqliteParts();
            TxtMasterPath.Text = clsSql.GetAppInfo("MasterExcelDir");

            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 850);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= ExcelItemMasterExport_Activated;
        }

        /// <summary>
        /// ���[�J�[�I���{�^���N���b�N(�E��)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMkSet_Click(object sender, RoutedEventArgs e)
        {
            var wMkDefined = LvMkDefined.SelectedItems;
            var wMkTarget = new ObservableCollection<MkMaster>();
            foreach (MkMaster iMaker in wMkDefined)
            {
                wMkTarget.Add(iMaker);
            }

            MakerMoveItems(wMkTarget, true);
        }

        /// <summary>
        /// ���[�J�[�I���{�^���N���b�N(����)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMkUnset_Click(object sender, RoutedEventArgs e)
        {
            var wMkSelected = LvMkSelected.SelectedItems;
            var wMkTarget = new ObservableCollection<MkMaster>();
            foreach (MkMaster iMaker in wMkSelected)
            {
                wMkTarget.Add(iMaker);
            }

            MakerMoveItems(wMkTarget, false);

        }

        /// <summary>
        /// ���[�J�[�S�I���{�^���N���b�N(�E��)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMkAllSet_Click(object sender, RoutedEventArgs e)
        {
            var wMkAllDifined = LvMkDefined.Items;
            var wMkTarget = new ObservableCollection<MkMaster>();
            foreach (MkMaster iMaker in wMkAllDifined)
            {
                wMkTarget.Add(iMaker);
            }

            MakerMoveItems(wMkTarget, true);

        }

        /// <summary>
        /// ���[�J�[�S�I���{�^���N���b�N(����)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMkAllUnset_Click(object sender, RoutedEventArgs e)
        {
            var wMkAllSelected = LvMkSelected.Items;
            var wMkTarget = new ObservableCollection<MkMaster>();
            foreach (MkMaster iMaker in wMkAllSelected)
            {
                wMkTarget.Add(iMaker);
            }

            MakerMoveItems(wMkTarget, false);
        }

        /// <summary>
        /// ���ޑI���{�^���N���b�N(�E��)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBunSet_Click(object sender, RoutedEventArgs e)
        {
            var wBunDefined = LvBunDefined.SelectedItems;
            var wBunTarget = new ObservableCollection<BunMaster>();
            foreach (BunMaster iBun in wBunDefined)
            {
                wBunTarget.Add(iBun);
            }

            BunMoveItems(wBunTarget, true);
        }

        /// <summary>
        /// ���ޑI���{�^���N���b�N(����)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBunUnset_Click(object sender, RoutedEventArgs e)
        {
            var wBunSelected = LvBunSelected.SelectedItems;
            var wBunTarget = new ObservableCollection<BunMaster>();
            foreach (BunMaster iBun in wBunSelected)
            {
                wBunTarget.Add(iBun);
            }

            BunMoveItems(wBunTarget, false);
        }

        /// <summary>
        /// ���ޑS�I���{�^���N���b�N(�E��)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBunAllSet_Click(object sender, RoutedEventArgs e)
        {
            var wBunAllDifined = LvBunDefined.Items;
            var wBunTarget = new ObservableCollection<BunMaster>();
            foreach (BunMaster iBun in wBunAllDifined)
            {
                wBunTarget.Add(iBun);
            }

            BunMoveItems(wBunTarget, true);
        }

        /// <summary>
        /// ���ޑS�I���{�^���N���b�N(����)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBunAllUnset_Click(object sender, RoutedEventArgs e)
        {
            var wBunAllSelected = LvBunSelected.Items;
            var wBunTarget = new ObservableCollection<BunMaster>();
            foreach (BunMaster iBun in wBunAllSelected)
            {
                wBunTarget.Add(iBun);
            }

            BunMoveItems(wBunTarget, false);
        }
        
        /// <summary>
        /// ����{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// �t�H���_�I���{�^���N���b�N
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
        /// ���s�{�^���N���b�N
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

                var clsSql = new SqliteParts();

                #region �`�F�b�N����

                //�e���v���[�g�t�@�C�������݂��邩�`�F�b�N
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string dirPath = System.IO.Path.GetDirectoryName(exePath);
                var orgFilePath = Path.Combine(dirPath, "Assets", "MstMnt.xlsx");
                if (!File.Exists(orgFilePath))
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�e���v���[�g�t�@�C�������݂��܂���B");
                    await tMsgDialog.ShowAsync();
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
                clsSql.UpdateAppInfo("MasterExcelDir", wDirPath);

                #endregion

                #region Excel�o��
                const int EXCEL_MAX_ROW = 1048576;

                var dtItems = GetOutputData();
                if (dtItems.Count == 0)
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", $"�����ɍ��v����f�[�^������܂���B");
                    await tMsgDialog.ShowAsync();
                    return;
                }
                if (dtItems.Count > EXCEL_MAX_ROW)
                {
                    string tMsg = $"�o�̓f�[�^��{EXCEL_MAX_ROW}���𒴂��Ă��܂��B";
                    tMsg += Environment.NewLine;
                    tMsg += "�o�͏�����ύX���Ď��s���Ă��������B";
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", tMsg);
                    await tMsgDialog.ShowAsync();
                    return;
                }
                var dtZokMaster = GetZokMaster();

                //�v���O���X�E�B���h�E�̕\��                               
                wProgressWindow.Activate();
                //�t�@�C���R�s�[��Excel�N�����܂߂�+2
                await wProgressWindow.SetInit("Excel�}�X�^�t�@�C���o�͒�...", dtItems.Count + 2);

                //�e���v���[�g�̃R�s�[
                if (await wProgressWindow.SetProgress("�e���v���[�g�t�@�C�����R�s�[��...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                    await wDialog.ShowAsync();
                    return;
                }
                var wFilePath = Path.Combine(wDirPath, $"MstMnt_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                System.IO.File.Copy(orgFilePath, wFilePath, true);

                //closedxml���[�N�V�[�g
                using (var xlBook = new ClosedXML.Excel.XLWorkbook(wFilePath))
                {
                    var xlSheet = xlBook.Worksheet(1);

                    if(ChkZok.IsChecked == true)
                    {
                        //�����}�X�^�̏o��
                        const int ZOK_CODE_ROW = 3;
                        const int ZOK_NAME_ROW = 4;
                        var actZokCol = 16;
                        foreach (var iZok in dtZokMaster)
                        {
                            actZokCol++;

                            //�����R�[�h
                            xlSheet.Cell(ZOK_CODE_ROW, actZokCol).Value = iZok.ZkCode;
                            //������
                            xlSheet.Cell(ZOK_NAME_ROW, actZokCol).Value = iZok.ZkName;
                        }
                    }

                    var actRow = 4;
                    foreach(var iItem in dtItems)
                    {
                        actRow++;

                        string wProgressMsg = $"{actRow}�s�ڂ�������...";
                        if (await wProgressWindow.SetProgress(wProgressMsg) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                            await wDialog.ShowAsync();
                            return;
                        }

                        //JAN�R�[�h
                        xlSheet.Cell(actRow, 1).Value = iItem.JanCode;
                        //���[�J�[�R�[�h
                        xlSheet.Cell(actRow, 2).Value = iItem.MkCode;
                        //���[�J�[��
                        xlSheet.Cell(actRow, 3).Value = iItem.MkName;
                        //���i�R�[�h
                        xlSheet.Cell(actRow, 4).Value = iItem.StoCode;
                        //���i���J�i
                        xlSheet.Cell(actRow, 5).Value = iItem.TanName;
                        //���i��
                        xlSheet.Cell(actRow, 6).Value = iItem.ItemName;
                        //��
                        xlSheet.Cell(actRow, 7).Value = iItem.W;
                        //����
                        xlSheet.Cell(actRow, 8).Value = iItem.H;
                        //���s
                        xlSheet.Cell(actRow, 9).Value = iItem.D;
                        //����
                        xlSheet.Cell(actRow, 10).Value = iItem.PCase;
                        //���ރR�[�h
                        xlSheet.Cell(actRow, 11).Value = iItem.BunCode;
                        //���ޖ�
                        xlSheet.Cell(actRow, 12).Value = iItem.BunName;
                        //����
                        xlSheet.Cell(actRow, 13).Value = iItem.Price;
                        //����
                        xlSheet.Cell(actRow, 14).Value = iItem.Cost;
                        //�o�^��
                        xlSheet.Cell(actRow, 15).Value = iItem.Attr3;
                        //�p�~��
                        xlSheet.Cell(actRow, 16).Value = iItem.Attr4;

                        if (ChkZok.IsChecked == true)
                        {
                            var actZokCol = 16;
                            var dtSyoZokSei = GetSyoZokSei(iItem.JanCode);
                            foreach (var iZk in dtZokMaster)
                            {
                                actZokCol++;
                                var lqZok = dtSyoZokSei.Where(x => x.ZkCode == iZk.ZkCode).FirstOrDefault();
                                if (lqZok != null)
                                {
                                    xlSheet.Cell(actRow, actZokCol).Value = lqZok.SuiCode;
                                }
                            }
                        }
                    }

                    xlBook.SaveAs(wFilePath);
                }


                #endregion

                #region Excel�N��

                if (await wProgressWindow.SetProgress("Excel�}�X�^�t�@�C�����N����...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                    await wDialog.ShowAsync();
                    return;
                }
                var wInf = new ProcessStartInfo
                {
                    FileName = "EXCEL.EXE",
                    Arguments = wFilePath,
                    UseShellExecute = true
                };
                Process.Start(wInf);

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
        }


        #endregion

        #region ���\�b�h

        #region �I�������֘A

        /// <summary>
        /// ���[�J�[�I������
        /// </summary>
        /// <param name="argItems">�ړ��Ώۃ��[�J�[</param>
        /// <param name="argIsSet">�ړ��ݒ�(true:��`���I��,false:�I������`)</param>
        private void MakerMoveItems(ObservableCollection<MkMaster> argItems, bool argIsSet)
        {

            foreach (MkMaster iMaker in argItems)
            {
                if (argIsSet)
                {
                    MakerSel.Add(iMaker);
                    MakerDef.Remove(iMaker);
                }
                else
                {
                    MakerDef.Add(iMaker);
                    MakerSel.Remove(iMaker);
                }
            }
        }

        /// <summary>
        /// ���ޑI������
        /// </summary>
        /// <param name="argItems">�ړ��Ώە���</param>
        /// <param name="argIsSet">�ړ��ݒ�(true:��`���I��,false:�I������`)</param>
        private void BunMoveItems(ObservableCollection<BunMaster> argItems, bool argIsSet)
        {

            foreach (BunMaster iBun in argItems)
            {
                if (argIsSet)
                {
                    BunSel.Add(iBun);
                    BunDef.Remove(iBun);
                }
                else
                {
                    BunDef.Add(iBun);
                    BunSel.Remove(iBun);
                }
            }
        }

        #endregion

        #region Excel�o�͊֘A

        /// <summary>
        /// Excel�o�̓f�[�^�擾
        /// </summary>
        /// <returns></returns>
        private IList<SyoMasterExcel> GetOutputData()
        {
            var clsSql = new SqliteParts();
            string wSql = "";
            wSql += "SELECT s.JanCode, s.MkCode, m.MkName, s.StoCode, s.TanName, s.ItemName, ";
            wSql += " s.W, s.H, s.D, s.PCase, s.BunCode, b.BunName, s.Price, s.Cost, s.Attr3, s.Attr4";
            wSql += " FROM(BunMaster b RIGHT JOIN SyoMaster s ON b.BunCode = s.BunCode)";
            wSql += " LEFT JOIN MkMaster m ON s.MkCode = m.MkCode";
            wSql +=" WHERE 1 = 1";

            #region ���[�J�[�i��

            string wMkCond = "";
            if (LvMkSelected.SelectedItems.Count > 0)
            {
                foreach(MkMaster iMaker in LvMkSelected.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(wMkCond)) wMkCond += " , ";
                    wMkCond += $" '{iMaker.MkCode}'";
                }
            }
            if (!string.IsNullOrEmpty(wMkCond))
            {
                wSql += $" AND s.MkCode IN ({wMkCond}) ";
            }

            #endregion

            #region ���ލi��

            string wBunCond = "";
            if (LvBunSelected.SelectedItems.Count > 0)
            {
                foreach (BunMaster iBun in LvBunSelected.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(wBunCond)) wBunCond += " , ";
                    wBunCond += $" '{iBun.BunCode}'";
                }
            }
            if (!string.IsNullOrEmpty(wBunCond))
            {
                wSql += $" AND s.BunCode IN ({wBunCond}) ";
            }
            #endregion

            wSql += " ORDER BY s.JanCode";

            var tData = clsSql.GetSelectResult<SyoMasterExcel>(wSql);

            return tData;
        }

        /// <summary>
        /// �����}�X�^�̎擾
        /// </summary>
        /// <returns></returns>
        private IList<ZokMaster> GetZokMaster()
        {

           var clsSql = new SqliteParts();
            string wSql = "";
            wSql += "SELECT * FROM ZokMaster";
            wSql += " ORDER BY ZkCode";

            var tData = clsSql.GetSelectResult<ZokMaster>(wSql);

            return tData;
        }

        /// <summary>
        /// ���i�����̎擾
        /// </summary>
        /// <param name="argJanCode"></param>
        /// <returns></returns>
        private IList<SyoZokSei> GetSyoZokSei(string argJanCode)
        {

            var clsSql = new SqliteParts();
            string wSql = "";
            wSql += "SELECT * FROM SyoZokSei";
            wSql += $" WHERE JanCode = '{argJanCode}'";

            var tData = clsSql.GetSelectResult<SyoZokSei>(wSql);

            return tData;
        }

        #endregion

        #endregion


    }
}

