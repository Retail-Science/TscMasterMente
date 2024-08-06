using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.Common
{

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgressWindow : Window
    {
        private bool IsCancelled { get; set; } = false;

        #region �R���X�g���N�^

        public ProgressWindow()
        {
            this.InitializeComponent();
            this.Activated += ProgressWindow_Activated;
        }
        #endregion

        #region �C�x���g

        /// <summary>
        /// Activated�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 500, 300);
            //�E�B���h�E�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            //�^�C�g���o�[���\��
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var m_AppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            var op = OverlappedPresenter.Create();
            op.IsResizable = false;
            op.IsAlwaysOnTop = false;
            op.SetBorderAndTitleBar(false, false);
            m_AppWindow.SetPresenter(op);

            this.Activated -= ProgressWindow_Activated;
        }


        /// <summary>
        /// �L�����Z���{�^���N���b�N�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // �L�����Z���C�x���g�𔭐�������
            IsCancelled = true;
        }

        #endregion

        #region ���\�b�h

        /// <summary>
        /// ����������
        /// </summary>
        /// <param name="argProcess"></param>
        /// <param name="argMaxCnt"></param>
        /// <returns></returns>
        public async Task<bool> SetInit(string argProcess, int argMaxCnt)
        {
            //ProgressBar�̏�����
            PbStatus.Minimum = 0;
            if (argMaxCnt <= 0)
            {
                PbStatus.Maximum = 100;
            }
            else
            {
                PbStatus.Maximum = argMaxCnt;
            }
            PbStatus.Value = 0;

            //�e�L�X�g�̏�����
            TxtProcess.Text = argProcess;
            TxtProcessNumCnt.Text = "";
            TxtDetail.Text = "������...";

            //�L�����Z���t���O�̏�����
            IsCancelled = false;

            await Task.Delay(100);
            return true;
        }

        /// <summary>
        /// �v���O���X�o�[�̐i����ݒ肷��
        /// </summary>
        /// <param name="argDtlMsg"></param>
        /// <returns></returns>
        public async Task<bool> SetProgress(string argDtlMsg)
        {
            if (IsCancelled)
            {
                //���f�m�F�_�C�A���O��\��
                //�Ăяo�����ƃv���O���X��ʂ̓������Ƃ�K�v������̂ŁA���̃^�C�~���O�Ń��b�Z�[�W�{�b�N�X��\������
                var wDialog = MessageParts.ShowMessageYesNo(this, "���f�m�F", "���s���̏����𒆒f���܂����H�B");
                if (await wDialog.ShowAsync() == ContentDialogResult.Secondary)
                {
                    IsCancelled = false;
                    await SetProgress(argDtlMsg);
                    return true;
                }
                return false;
            }
            else
            {
                TxtDetail.Text = argDtlMsg;
                PbStatus.Value += 1;
                TxtProcessNumCnt.Text = PbStatus.Value.ToString() + " / " + PbStatus.Maximum.ToString();

                await Task.Delay(100);
                return true;
            }
        }

        #endregion
    }
}
