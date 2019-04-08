using LocaleManager.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForm = System.Windows.Forms;

namespace LocaleManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TranslationManager _fileProvider = new TranslationManager();
        private readonly ObservableCollection<DataGridRow> _rows = new ObservableCollection<DataGridRow>();

        public MainWindow()
        {
            InitializeComponent();
            Init(AppDomain.CurrentDomain.BaseDirectory);
            dgTranslations.ItemsSource = _rows;
            dgTranslations.AutoGenerateColumns = false;
            dgTranslations.CanUserAddRows = false;
            dgTranslations.CanUserDeleteRows = false;
        }
        
        private void Init(string directory)
        {
            _rows.Clear();
            _fileProvider.LoadFiles(directory);
            if (_fileProvider.Locales.Count == 0)
            {
                MessageBox.Show("No Locale file was found");
                return;
            }

            dgTranslations.Columns.Clear();
            dgTranslations.Columns.Add(new DataGridTextColumn
            {
                Binding = new Binding("[0]"),
                Header = "Key",
                Width = 400,
                IsReadOnly = true
            });
            var i = 1;
            foreach (var locale in _fileProvider.Locales)
            {
                dgTranslations.Columns.Add(new DataGridTextColumn
                {
                    Binding = new Binding($"[{i}]"),
                    Header = locale,
                    Width = 300
                });
                i++;
            }
            RefreshGridData();
        }

        private void RefreshGridData()
        {
            _rows.Clear();
            var invalid = _fileProvider.Translations.GetInvalidNodes();

            var columnsCount = _fileProvider.Locales.Count + 1;
            foreach (var path in _fileProvider.Translations.Keys.OrderBy(k => k))
            {
                var columns = new List<string>(columnsCount) { path };

                foreach (var locale in _fileProvider.Locales)
                {
                    var text = _fileProvider.Translations.Get(path, locale);
                    columns.Add(text);
                }

                var observable = new ObservableCollection<string>(columns);
                observable.CollectionChanged += OnTextChange;
                var row = new DataGridRow
                {
                    Item = observable
                };

                if (invalid.Contains(path))
                {
                    row.Background = Brushes.IndianRed;
                    row.Foreground = Brushes.White;
                    row.ToolTip = "This kind of node is not allowed and will be ignored during saving";
                }

                _rows.Add(row);
            }
        }

        private void OnTextChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            var collection = (ObservableCollection<string>)sender;
            var path = collection.First();
            var skip = 1;
            foreach (var locale in _fileProvider.Locales)
            {
                var text = collection.Skip(skip).First();
                _fileProvider.Translations.Set(path, locale, text);
                skip++;
            }
        }

        #region Buttons

        private void MenuItem_LoadClick(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForm.FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                SelectedPath = AppDomain.CurrentDomain.BaseDirectory
            };
            var result = folderDialog.ShowDialog();

            if (result == WinForm.DialogResult.OK)
            {
                Init(folderDialog.SelectedPath);
            }
        }

        private void MenuItem_SaveClick(object sender, RoutedEventArgs e)
        {
            _fileProvider.Save();
        }

        private void MenuItem_AddClick(object sender, RoutedEventArgs e)
        {
            var dlg = new AddWindow(_fileProvider.Locales.ToList())
            {
                Owner = this
            };

            if (dlg.ShowDialog() == true)
            {
                foreach (var translation in dlg.Data.Translations)
                {
                    _fileProvider.Translations.Set(dlg.Data.Key, translation.Locale, translation.Text);
                }

                RefreshGridData();
            }
        }

        private void MenuItem_RemoveClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in dgTranslations.SelectedItems)
            {
                var lvItem = item as DataGridRow;
                if (lvItem?.Item is ObservableCollection<string> items)
                {
                    var path = items.First();
                    _fileProvider.Translations.Remove(path);
                }
            }
            RefreshGridData();
        }

        #endregion 
    }
}
