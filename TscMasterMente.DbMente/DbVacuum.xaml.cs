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
using Windows.Foundation;
using Windows.Foundation.Collections;
using TscMasterMente.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DbMente
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DbVacuum : Window
    {
        #region �R���X�g���N�^

        public DbVacuum()
        {
            this.InitializeComponent();

            // �C�x���g�ݒ�
            this.Activated += DbVacuum_Activated;
        }

        #endregion

        #region �C�x���g

        /// <summary>
        /// Activated�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbVacuum_Activated(object sender, WindowActivatedEventArgs e)
        {
            //�A�v���A�C�R����ݒ�
            WindowParts.SetAppIcon(this);
            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 300);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            var clsSql = new SqliteParts();
            TxtDbPath.Text = clsSql.GetDbFilePath();

            this.Activated -= DbVacuum_Activated;
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
        /// DB�t�@�C���p�X�{�^��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnDbPath_Click(object sender, RoutedEventArgs e)
        {
            var wPath = await IoParts.SaveFileAsync(this, IoParts.EnumFileType.SqliteFiles);
            if (wPath != null)
            {
                TxtDbPath.Text = wPath;
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

            try
            {
                //�E�B���h�E�̖�����
                WindowParts.SetAllChildEnabled(MainContent, false);

                var clsSql = new SqliteParts();

                #region �`�F�b�N����

                if (string.IsNullOrEmpty(TxtDbPath.Text))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "DB�t�@�C���p�X���ݒ肳��Ă��܂���B");
                    await wDialog.ShowAsync();
                    return;
                }
                else if (File.Exists(TxtDbPath.Text) == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "DB�����݂��܂���B");
                    await wDialog.ShowAsync();
                    return;
                }

                #endregion

                //�v���O���X�E�B���h�E�̕\��                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("DB���œK����...", 1);

                #region DB�œK������
                if (await wProgressWindow.SetProgress("�œK����...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                    await wDialog.ShowAsync();
                    return;
                }
                clsSql.ExecuteVacuum(TxtDbPath.Text);

                #endregion


                wSucceed = true;
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

                if (wSucceed)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "����", "DB�̍œK�����������܂����B");
                    await wDialog.ShowAsync();
                }
            }
        }

        #endregion


    }
}
