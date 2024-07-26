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
            MasterExport wMstExp = new MasterExport();
            wMstExp.Activate();
        }

        /// <summary>
        /// �I�T�C�G���X�}�X�^�捞
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuMasterImport_Click(object sender, RoutedEventArgs e)
        {
            MasterImport wMstImp = new MasterImport();
            wMstImp.Activate();
        }

        /// <summary>
        /// Excel�o��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemExp_Click(object sender, RoutedEventArgs e)
        {
            ExcelItemMasterExport wExp = new ExcelItemMasterExport();
            wExp.Activate();
        }

        /// <summary>
        /// Excel�捞
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemImp_Click(object sender, RoutedEventArgs e)
        {
            ExcelItemMasterImport wImp = new ExcelItemMasterImport();
            wImp.Activate();
        }

        /// <summary>
        /// Excel�V�K�쐬
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemBlank_Click(object sender, RoutedEventArgs e)
        {
            ExcelItemMasterBlank wBlank = new ExcelItemMasterBlank();
            wBlank.Activate();
        }

        /// <summary>
        /// Planet�f�[�^�ϊ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlanet_Click(object sender, RoutedEventArgs e)
        {
            PlanetConvert wPlanet = new PlanetConvert();
            wPlanet.Activate();
        }
        #endregion


    }
}
