using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using TscMasterMente.Common;
using System.Diagnostics;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExcelItemMasterBlank : Window
    {
        #region �R���X�g���N�^

        public ExcelItemMasterBlank()
        {
            this.InitializeComponent();

            // �C�x���g�ݒ�
            this.Activated += ExcelItemMasterBlank_Activated;
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
        private void ExcelItemMasterBlank_Activated(object sender, WindowActivatedEventArgs e)
        {
            var clsSql = new SqliteParts();
            TxtMasterPath.Text = clsSql.GetAppInfo("MasterExcelDir");

            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 300);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= ExcelItemMasterBlank_Activated;
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

                //�v���O���X�E�B���h�E�̕\��                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("Excel�}�X�^�t�@�C���o�͒�...", 3);

                #region �e���v���[�g�̃R�s�[
                if (await wProgressWindow.SetProgress("�e���v���[�g�t�@�C�����R�s�[��...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                    await wDialog.ShowAsync();
                    return;
                }
                var wFilePath = Path.Combine(wDirPath, $"MstMnt_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                System.IO.File.Copy(orgFilePath, wFilePath, true);
                #endregion

                #region Excel�t�@�C���̕ۑ�
                if (await wProgressWindow.SetProgress("�ݒ蒆...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                    await wDialog.ShowAsync();
                    return;
                }
                //�t�@�C���R�s�[�����ł͍X�V�������ύX����Ȃ��̂ŁA�ۑ������{����
                using (var xlBook = new ClosedXML.Excel.XLWorkbook(wFilePath))
                {
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
    }
}
