using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
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
using TscMasterMente.Common.MasterFileEntity;
using TscMasterMente.Common.SqlMaps;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExcelItemMasterImport : Window
    {
        #region �R���X�g���N�^

        public ExcelItemMasterImport()
        {
            this.InitializeComponent();

            this.Activated += ExcelItemMasterImport_Activated;
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
        private async void ExcelItemMasterImport_Activated(object sender, WindowActivatedEventArgs e)
        {
            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 600);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            //�}�X�^�t�@�C���p�X�ݒ�
            var clsSql = new SqliteParts();
            var dirPath = clsSql.GetAppInfo("MasterExcelDir");
            var xlFiles = await IoParts.GetFilesAsync(dirPath, false, new List<string>() { ".xlsx" });
            if (xlFiles.Count > 0)
            {
                var lqXls = xlFiles.OrderByDescending(x => x.DateCreated).FirstOrDefault();
                dirPath = Path.Combine(dirPath, lqXls.Name);
            }
            TxtMasterPath.Text = dirPath;

            this.Activated -= ExcelItemMasterImport_Activated;
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
        /// �t�@�C���I���{�^��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            //�t�@�C�����J���_�C�A���O
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.XlsxFiles);
            if (wPath != null)
            {
                TxtMasterPath.Text = wPath;
            }
        }

        private async void BtnExec_Click(object sender, RoutedEventArgs e)
        {
            var wProgressWindow = new ProgressWindow();
            try
            {
                //�E�B���h�E�̖�����
                WindowParts.SetAllChildEnabled(MainContent, false);

                #region �`�F�b�N����

                if (string.IsNullOrEmpty(TxtMasterPath.Text))
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�t�@�C����I�����Ă��������B");
                    await tMsgDialog.ShowAsync();
                    return;
                }
                else if (!System.IO.File.Exists(TxtMasterPath.Text))
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�t�@�C�������݂��܂���B");
                    await tMsgDialog.ShowAsync();
                    return;
                }
                else if (!IsEnableExcelFile(TxtMasterPath.Text))
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "Excel�t�@�C�������s���ł��B�I����A�Ď��s�肢�܂��B");
                    await tMsgDialog.ShowAsync();
                    return;
                }

                #endregion

                #region Excel�捞����
                var wSql = new SqliteParts();
                const int BEGIN_EXCEL_ROW = 5;

                using (var wConn = wSql.GetConnection())
                using (var xlBook = new ClosedXML.Excel.XLWorkbook(TxtMasterPath.Text))
                {
                    wConn.Open();
                    using (var tran = wConn.BeginTransaction())
                    {
                        try
                        {
                            var xlSheet = xlBook.Worksheet("�ꊇ�o�^�V�[�g�i���̕ύX�s�j");
                            var wMaxRowCnt = xlSheet.RowsUsed().Count();
                            var wMaxColCnt = xlSheet.ColumnsUsed().Count();

                            //�v���O���X�E�B���h�E�̕\��                               
                            wProgressWindow.Activate();
                            await wProgressWindow.SetInit("Excel�}�X�^�t�@�C���o�͒�...", wMaxRowCnt - BEGIN_EXCEL_ROW);

                            if (RbDelInsert.IsChecked == true)
                            {
                                //�폜���ǉ�
                                if(ChkSyoZokImpFlg.IsChecked == true)
                                {
                                    wSql.ExecuteSql(wConn, "DELETE FROM SyoZokSei", null);
                                }
                                wSql.ExecuteSql(wConn, "DELETE FROM SyoZokSei", null);
                            }

                            for (int iRow = BEGIN_EXCEL_ROW; iRow < wMaxRowCnt; iRow++)
                            {
                                //�i����
                                if (await wProgressWindow.SetProgress($"{iRow}�s�ڂ�������...") == false)
                                {
                                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                                    await wDialog.ShowAsync();
                                    return;
                                }

                                if (!ImportSyoMaser(wSql, wConn, xlSheet, iRow))
                                {
                                    var errMsg = $"���i�}�X�^�̎捞�Ɏ��s���܂����B�s�ԍ��F{iRow}";
                                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", errMsg);
                                    await tMsgDialog.ShowAsync();
                                }

                                if (ChkSyoZokImpFlg.IsChecked == true && !ImportSyoZokSei(wSql, wConn, xlSheet, iRow))
                                {
                                    var errMsg = $"���i�����}�X�^�̎捞�Ɏ��s���܂����B�s�ԍ��F{iRow}";
                                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", errMsg);
                                    await tMsgDialog.ShowAsync();
                                }
                            }
                            tran.Commit();
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

        /// <summary>
        /// Excel���g�p�\���`�F�b�N
        /// </summary>
        /// <param name="argPath">Excel�t�@�C���p�X</param>
        /// <returns></returns>
        private bool IsEnableExcelFile(string argPath)
        {
            try
            {
                using (FileStream stream = File.Open(argPath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                // IOException�����������ꍇ�A�t�@�C���͊J���Ă��邩�A�A�N�Z�X�ł��Ȃ���Ԃł��B
                return false;
            }

            // ��O���������Ȃ������ꍇ�A�t�@�C���͊J���Ă��Ȃ��ƌ��Ȃ��܂��B
            return true;
        }

        /// <summary>
        /// ���i�}�X�^�捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argSheet">�捞��V�[�g</param>
        /// <param name="argRow">�����s</param>
        /// <returns></returns>
        private bool ImportSyoMaser(SqliteParts argSqlCls, SqliteConnection argConn, IXLWorksheet argSheet, int argRow)
        {
            const int MAX_WDH = 9999;
            const int MIN_WDH = 0;

            #region �`�F�b�N�y�уf�[�^�擾

            //JanCode
            var wJanCode = argSheet.Cell(argRow, 1).GetString();
            if (string.IsNullOrEmpty(wJanCode) || wJanCode.Length > 16)
            {
                return false;
            }

            //���[�J�R�[�h
            var wMkCode = argSheet.Cell(argRow, 2).GetString();
            if (!string.IsNullOrEmpty(wMkCode) && wMkCode.Length > 20)
            {
                return false;
            }

            //���i�R�[�h
            var wStoCode = argSheet.Cell(argRow, 4).GetString();
            if (!string.IsNullOrEmpty(wStoCode) && wStoCode.Length > 20)
            {
                return false;
            }

            //���i���J�i
            var wTanName = argSheet.Cell(argRow, 5).GetString();
            if (!string.IsNullOrEmpty(wTanName) && wTanName.Length > 255)
            {
                return false;
            }

            //���i��
            var wItemName = argSheet.Cell(argRow, 6).GetString();
            if (!string.IsNullOrEmpty(wItemName) && wItemName.Length > 255)
            {
                return false;
            }

            //��
            var chkW = argSheet.Cell(argRow, 7).GetString();
            int wW = 0;
            if (string.IsNullOrEmpty(chkW))
            {
                wW = 100;
            }
            else if (!int.TryParse(chkW, out wW))
            {
                return false;
            }
            else if (wW < MIN_WDH || wW > MAX_WDH)
            {
                return false;
            }

            //���s
            var chkD = argSheet.Cell(argRow, 8).GetString();
            int wD = 0;
            if (string.IsNullOrEmpty(chkD))
            {
                wD = 100;
            }
            else if (!int.TryParse(chkD, out wD))
            {
                return false;
            }
            else if (wW < MIN_WDH || wW > MAX_WDH)
            {
                return false;
            }

            //����
            var chkH = argSheet.Cell(argRow, 9).GetString();
            int wH = 0;
            if (string.IsNullOrEmpty(chkH))
            {
                wH = 100;
            }
            else if (!int.TryParse(chkH, out wH))
            {
                return false;
            }
            else if (wW < MIN_WDH || wW > MAX_WDH)
            {
                return false;
            }

            //����
            var chkPCase = argSheet.Cell(argRow, 10).GetString();
            int wPCase = 0;
            if (string.IsNullOrEmpty(chkPCase))
            {
                wPCase = 0;
            }
            else if (!int.TryParse(chkPCase, out wPCase))
            {
                return false;
            }
            else if (wPCase < 0 || wPCase > 99999)
            {
                return false;
            }

            //���ރR�[�h
            var wBunCode = argSheet.Cell(argRow, 11).GetString();
            if (!string.IsNullOrEmpty(wBunCode) && wBunCode.Length > 20)
            {
                return false;
            }

            //����
            var chkPrice = argSheet.Cell(argRow, 13).GetString();
            double wPrice = 0;
            if (string.IsNullOrEmpty(chkPrice))
            {
                wPrice = 0;
            }
            else if (!double.TryParse(chkPrice, out wPrice))
            {
                return false;
            }
            else if (wPrice < 0 || wPrice > 9999999)
            {
                return false;
            }

            //����
            var chkCost = argSheet.Cell(argRow, 14).GetString();
            double wCost = 0;
            if (string.IsNullOrEmpty(chkCost))
            {
                wCost = 0;
            }
            else if (!double.TryParse(chkCost, out wCost))
            {
                return false;
            }
            else if (wPrice < 0 || wPrice > 9999999)
            {
                return false;
            }

            //�o�^��
            var wAttr3 = argSheet.Cell(argRow, 15).GetString();
            if (!string.IsNullOrEmpty(wAttr3) && wAttr3.Length > 8)
            {
                return false;
            }

            //�p�~��
            var wAttr4 = argSheet.Cell(argRow, 16).GetString();
            if (!string.IsNullOrEmpty(wAttr4) && wAttr4.Length > 8)
            {
                return false;
            }

            //�X�V��
            var wAttr5 = DateTime.Now.ToString("yyyyMMdd");

            #endregion

            #region �p�����^�ݒ�

            var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@JanCode", wJanCode),
                        new SqliteParameter("@MkCode", wMkCode),
                        new SqliteParameter("@StoCode", wStoCode),
                        new SqliteParameter("@TanName", wTanName),
                        new SqliteParameter("@ItemName", wItemName),
                        new SqliteParameter("@W", wW),
                        new SqliteParameter("@D", wD),
                        new SqliteParameter("@H", wH),
                        new SqliteParameter("@PCase", wPCase),
                        new SqliteParameter("@BunCode", wBunCode),
                        new SqliteParameter("@Price", wPrice),
                        new SqliteParameter("@Cost", wCost),
                        new SqliteParameter("@Attr3", wAttr3),
                        new SqliteParameter("@Attr4", wAttr4),
                        new SqliteParameter("@Attr5", wAttr5)
                    };
            foreach (var iParam in wParams)
            {
                switch (iParam.ParameterName)
                {
                    case "@Attr3":
                    case "@Attr4":
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DateTime.Now.ToString("yyyyMMdd");
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
            else
            {
                //�폜���ǉ������̏ꍇ�͊����f�[�^�폜�ς�
                //�ǉ��X�V�A�폜���ǉ������͓��ꏈ��
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

            return true;
        }

        /// <summary>
        /// ���i�����}�X�^�捞����
        /// </summary>
        /// <param name="argSqlCls">SqliteParts�N���X</param>
        /// <param name="argConn">�R�l�N�V����</param>
        /// <param name="argSheet">�捞��V�[�g</param>
        /// <param name="argRow">�����s</param>
        /// <returns></returns>
        private bool ImportSyoZokSei(SqliteParts argSqlCls, SqliteConnection argConn, IXLWorksheet argSheet, int argRow)
        {
            //JanCode�`�F�b�N
            var wJanCode = argSheet.Cell(argRow, 1).GetString();
            if (string.IsNullOrEmpty(wJanCode) || wJanCode.Length > 16)
            {
                return false;
            }

            var dtSyoZk = new List<SyoZokSei>();
            for (int iCol = 17; iCol < argSheet.ColumnsUsed().Count(); iCol++)
            {
                var tmpSyoZk = new SyoZokSei();

                //JanCode�ݒ�
                tmpSyoZk.JanCode = wJanCode;

                //�����R�[�h�`�F�b�N
                var wZkCode = argSheet.Cell(3, iCol).GetString();
                if (string.IsNullOrEmpty(wZkCode) || wZkCode.Length > 20)
                {
                    return false;
                }

                //�����R�[�h�`�F�b�N
                var wSuiCode = argSheet.Cell(argRow, iCol).GetString();
                if (string.IsNullOrEmpty(wSuiCode) || wSuiCode.Length > 20)
                {
                    return false;
                }

                tmpSyoZk.JanCode = wJanCode;
                tmpSyoZk.ZkCode = wZkCode;
                tmpSyoZk.SuiCode = wSuiCode;
                dtSyoZk.Add(tmpSyoZk);
            }

            #region �o�^����            
            foreach (var iData in dtSyoZk)
            {
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

                //�폜���ǉ������̏ꍇ�͊����f�[�^�폜�ς�
                //�S�Ă̍X�V�����œ��ꏈ��
                var tSql = $"INSERT INTO SyoZokSei (JanCode, ZkCode, SuiCode) VALUES (@JanCode , @ZkCode, @SuiCode) ";
                tSql += " ON CONFLICT(JanCode, ZkCode, SuiCode) DO NOTHING";
                argSqlCls.ExecuteSql(argConn, tSql, wParams);

                #endregion
            }

            #endregion

            return true;
        }



        #endregion
    }
}
