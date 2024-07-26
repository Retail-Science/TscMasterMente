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
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TscMasterMente.Common;
using TscMasterMente.DataProc;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenu : Window
    {
        #region private�ϐ�

        private string _DbVer = "";
        private string _AppVer = "";
        #endregion

        #region �R���X�g���N�^


        public MainMenu(string dbVer, string appVer)
        {
            this.InitializeComponent();
            this.Activated += MainMenu_Activated;
            _DbVer = dbVer;
            _AppVer = appVer;

            NvMain.SelectionChanged += NavView_SelectionChanged;
            // �����y�[�W��\������
            contentFrame.Navigate(typeof(PageFileProc));
        }

        #endregion

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
        private void MainMenu_Activated(object sender, WindowActivatedEventArgs args)
        {

            this.Title = $"�}�X�^�����e AppVer({_DbVer}) DbVer({_AppVer})";

            //�E�B���h�E�T�C�Y��ݒ�
            WindowParts.SetWindowSize(this, 800, 600);
            //window�𒆉��ɕ\��
            WindowParts.SetCenterPosition(this);
            //�E�B���h�E�T�C�Y�Œ�
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= MainMenu_Activated;
        }

        /// <summary>
        /// �i�r�Q�[�V�����r���[�I��ύX�C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer.Tag.ToString() == "PageFileProc")
            {
                contentFrame.Navigate(typeof(PageFileProc), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else if (args.SelectedItemContainer.Tag.ToString() == "PageMasterMente")
            {
                contentFrame.Navigate(typeof(PageMasterMente), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else if(args.SelectedItemContainer.Tag.ToString() == "PageDbMente")
            {
                contentFrame.Navigate(typeof(PageDbMente), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        #endregion
    }
}
