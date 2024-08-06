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
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DbMente
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DbBackup : Window
    {
        #region �R���X�g���N�^

        public DbBackup()
        {
            this.InitializeComponent();

            // �C�x���g�ݒ�
            this.Activated += DbBackup_Activated;
        }

        #endregion

        #region �C�x���g

        /// <summary>
        /// Activated�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbBackup_Activated(object sender, WindowActivatedEventArgs e)
        {
            var clsSql = new SqliteParts();
            TxtOrgDbPath.Text = clsSql.GetDbFilePath();

            //�A�v���A�C�R����ݒ�
            WindowParts.SetAppIcon(this);
            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 600, 500);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= DbBackup_Activated;
        }

        /// <summary>
        /// ����{�^���N���b�N�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// �I���W�i��DB�t�@�C���p�X�{�^���N���b�N�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void BtnOrgDbPath_Click(object sender, RoutedEventArgs e)
        {
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.SqliteFiles);
            if (wPath != null)
            {
                TxtOrgDbPath.Text = wPath;
            }
        }

        /// <summary>
        /// �o�b�N�A�b�vDB�t�@�C���p�X�{�^���N���b�N�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnBkupDbPath_Click(object sender, RoutedEventArgs e)
        {
            var wPath = await IoParts.SaveFileAsync(this, IoParts.EnumFileType.SqliteFiles);
            if (wPath != null)
            {
                TxtBkupDbPath.Text = wPath;
            }
        }

        /// <summary>
        /// ���s�{�^���N���b�N�C�x���g
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

                if (string.IsNullOrEmpty(TxtOrgDbPath.Text))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�I���W�i��DB�t�@�C���p�X���ݒ肳��Ă��܂���B");
                    await wDialog.ShowAsync();
                    return;
                }
                else if (File.Exists(TxtOrgDbPath.Text) == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�I���W�i��DB�����݂��܂���B");
                    await wDialog.ShowAsync();
                    return;
                }

                if (string.IsNullOrEmpty(TxtBkupDbPath.Text))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�o�b�N�A�b�vDB�t�@�C���p�X���ݒ肳��Ă��܂���B");
                    await wDialog.ShowAsync();
                    return;
                }

                if(TxtOrgDbPath.Text == TxtBkupDbPath.Text)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�I���W�i��DB�ƃo�b�N�A�b�vDB����v���Ă��܂��B");
                    await wDialog.ShowAsync();
                    return;
                }

                #endregion

                //�v���O���X�E�B���h�E�̕\��                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("DB�o�b�N�A�b�v��...", 2);

                #region DB�o�b�N�A�b�v����
                if (await wProgressWindow.SetProgress("�e���v���[�g�t�@�C�����R�s�[��...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�������f", "�������ɃL�����Z�������s���܂����B");
                    await wDialog.ShowAsync();
                    return;
                }
                System.IO.File.Copy(TxtOrgDbPath.Text, TxtBkupDbPath.Text, true);
                #endregion

                #region �o�b�N�A�b�v�t�@�C���̃`�F�b�N

                if (await wProgressWindow.SetProgress("�o�b�N�A�b�vDB�`�F�b�N��...") == false)
                {
                    if (File.Exists(TxtBkupDbPath.Text) == false)
                    {
                        var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�o�b�N�A�b�v�t�@�C���̍쐬�Ɏ��s���܂����B");
                        await wDialog.ShowAsync();
                        return;
                    }
                }

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
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "����", "DB�̃o�b�N�A�b�v���������܂����B");
                    await wDialog.ShowAsync();
                }
            }
        }

        #endregion

    }
}
