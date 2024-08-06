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
using TscMasterMente.DataProc;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageFileProc : Page
    {
        #region �R���X�g���N�^

        public PageFileProc()
        {
            this.InitializeComponent();
        }

        #endregion

        #region �C�x���g

        /// <summary>
        /// �I�T�C�G���X�}�X�^�o��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuMasterExport_Click(object sender, RoutedEventArgs e)
        {

            if (MenteParts.FindWindowTitleActivatee("�}�X�^�t�@�C���o��")) return;            

            MasterExport wMstExp = new MasterExport();
            ((App)Application.Current).ProWindowMng.AddWindow(wMstExp);
            wMstExp.Activate();
        }

        /// <summary>
        /// �I�T�C�G���X�}�X�^�捞
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuMasterImport_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("�}�X�^�t�@�C���捞")) return;

            MasterImport wMstImp = new MasterImport();
            ((App)Application.Current).ProWindowMng.AddWindow(wMstImp);
            wMstImp.Activate();
        }

        /// <summary>
        /// Excel�o��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemExp_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("Excel���i�}�X�^�o��")) return;

            ExcelItemMasterExport wExp = new ExcelItemMasterExport();
            ((App)Application.Current).ProWindowMng.AddWindow(wExp);
            wExp.Activate();
            
        }

        /// <summary>
        /// Excel�捞
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemImp_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("Excel���i�}�X�^�捞")) return;

            ExcelItemMasterImport wImp = new ExcelItemMasterImport();
            ((App)Application.Current).ProWindowMng.AddWindow(wImp);
            wImp.Activate();
        }

        /// <summary>
        /// Excel�V�K�쐬
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemBlank_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("Excel���i�}�X�^�o��-�u�����N")) return;

            ExcelItemMasterBlank wBlank = new ExcelItemMasterBlank();
            ((App)Application.Current).ProWindowMng.AddWindow(wBlank);
            wBlank.Activate();
        }

        /// <summary>
        /// Planet�f�[�^�ϊ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlanet_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("�v���l�b�g�f�[�^�o��")) return;

            PlanetConvert wPlanet = new PlanetConvert();
            ((App)Application.Current).ProWindowMng.AddWindow(wPlanet);
            wPlanet.Activate();
        }
        #endregion


    }
}
