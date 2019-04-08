using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LocaleManager.Extensions;
using LocaleManager.Models;

namespace LocaleManager
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow(List<string> locales)
        {
            InitializeComponent();
            var data = locales.Select(l => new AddWindowData.TranslationData(l, "")).ToList();
            DataContext = new AddWindowData(data);
            tbKey.Focus();
        }

        public AddWindowData Data => (AddWindowData)DataContext;
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!this.IsValid()) return;
            this.DialogResult = true;
        }
    }
}
