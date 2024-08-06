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
    public sealed partial class TanaScienceSetting : Window
    {
        #region �R���X�g���N�^
        
        public TanaScienceSetting()
        {
            this.InitializeComponent();

            // �C�x���g�ݒ�
            this.Activated += TanaScienceSetting_Activated;
        }

        #endregion

        #region �C�x���g

        /// <summary>
        /// Activated�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TanaScienceSetting_Activated(object sender, WindowActivatedEventArgs e)
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
            var wIniPath = clsSql.GetAppInfo("TscConfigPath");
            if(!string.IsNullOrEmpty(wIniPath) && File.Exists(wIniPath))
            {
                TxtTanaIniPath.Text = wIniPath;
            }
            else
            {
                TxtTanaIniPath.Text = "";
            }

            this.Activated -= TanaScienceSetting_Activated;
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
        /// �I�T�C�G���XIni�t�@�C���{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnTanaIniPath_Click(object sender, RoutedEventArgs e)
        {
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.IniFiles);
            if (wPath != null)
            {
                TxtTanaIniPath.Text = wPath;
            }
        }

        /// <summary>
        /// ���s�{�^���N���b�N
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnExec_Click(object sender, RoutedEventArgs e)
        {
            var wSucceed = false;

            try
            {

                var clsSql = new SqliteParts();

                #region �`�F�b�N����

                if (string.IsNullOrEmpty(TxtTanaIniPath.Text))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�\���t�@�C���p�X��ݒ肵�Ă��������B");
                    await wDialog.ShowAsync();
                    return;
                }
                else if (File.Exists(TxtTanaIniPath.Text) == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�\���t�@�C�������݂��܂���B");
                    await wDialog.ShowAsync();
                    return;
                }
                else if(TxtTanaIniPath.Text.ToUpper().EndsWith("TANA.INI") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "�G���[", "�I�T�C�G���X�̍\���t�@�C��(Tana.Ini)�ł͂���܂���B�B");
                    await wDialog.ShowAsync();
                    return;
                }

                #endregion
                
                //Ini�t�@�C����DB�ɓo�^
                clsSql.UpdateAppInfo("TscConfigPath", TxtTanaIniPath.Text);

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
                if (wSucceed)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "����", "�\���t�@�C���p�X��ۑ����܂����B");
                    await wDialog.ShowAsync();
                }
            }
        }



        #endregion
    }
}
