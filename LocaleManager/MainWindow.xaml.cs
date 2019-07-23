using LocaleManager.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
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
using LocaleManager.Properties;
using LocaleManager.Translations;
using WinForm = System.Windows.Forms;

namespace LocaleManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TranslationManager _fileProvider = new TranslationManager();
        private readonly Translator _translator = new Translator();
        private readonly ObservableCollection<DataGridRow> _rows = new ObservableCollection<DataGridRow>();

        public MainWindow()
        {
            InitializeComponent();
            dgTranslations.ItemsSource = _rows;
            dgTranslations.AutoGenerateColumns = false;
            dgTranslations.CanUserAddRows = false;
            dgTranslations.CanUserDeleteRows = false;
            dgTranslations.CanUserSortColumns = false;
            var directory = GetDirectory();
            Init(directory);
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
                Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                IsReadOnly = true,
                CanUserReorder = false
            });
            var i = 1;
            foreach (var locale in _fileProvider.Translations.GetLocalesByCountOfValues())
            {
                dgTranslations.Columns.Add(new DataGridTextColumn
                {
                    Binding = new Binding($"[{i}]"),
                    Header = locale,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
                i++;
            }
            RefreshGridData();
        }

        private string GetDirectory()
        {
            var directory = Settings.Default.Folder;
            return (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)) ? directory : AppDomain.CurrentDomain.BaseDirectory;
        }

        private void SetDirectory(string directory)
        {
            Settings.Default.Folder = directory;
            Settings.Default.Save();
        }
        private void RefreshGridData()
        {
            _rows.Clear();
            var invalid = _fileProvider.Translations.GetInvalidNodes();
            var locales = _fileProvider.Translations.GetLocalesByCountOfValues();
            var columnsCount = _fileProvider.Locales.Count + 1;
            foreach (var path in _fileProvider.Translations.Keys.OrderBy(k => k))
            {
                var columns = new List<string>(columnsCount) { path };

                foreach (var locale in locales)
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
                SetDirectory(folderDialog.SelectedPath);
                Init(folderDialog.SelectedPath);
            }
        }

        private void MenuItem_SaveClick(object sender, RoutedEventArgs e)
        {
            if (_fileProvider.Save())
            {
                MessageBox.Show("Saved");
            }
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

        private async void MenuItem_TranslateClick(object sender, RoutedEventArgs e)
        {
            var columns = dgTranslations.Columns
                .Where(c => c.CanUserReorder)
                .OrderBy(c => c.DisplayIndex)
                .Select(c => c.Header.ToString());
            if (!columns.Any())
            {
                return;
            }
            var sourceLocale = columns.First();
            var targetLocales = columns.Skip(1).ToList();
            pbStatus.Value = 0;
            pbStatus.Visibility = Visibility.Visible;
            var statusStep = 100m / (_fileProvider.Translations.Keys.Count * targetLocales.Count);
            var total = 0m;
            foreach (var key in _fileProvider.Translations.Keys)
            {
                foreach (var targetLocale in targetLocales)
                {
                    total += statusStep;
                    pbStatus.Value = (int)total;
                    var currentValue = _fileProvider.Translations.Get(key, targetLocale);
                    if (!string.IsNullOrWhiteSpace(currentValue))
                    {
                        continue;
                    }
                    var sourceValue = _fileProvider.Translations.Get(key, sourceLocale);
                    if (string.IsNullOrWhiteSpace(sourceValue))
                    {
                        continue;
                    }
                    try
                    {
                        var translated = await _translator.Translate(sourceValue, targetLocale, sourceLocale);
                        if (!string.IsNullOrWhiteSpace(translated))
                        {
                            _fileProvider.Translations.Set(key, targetLocale, translated);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            pbStatus.Visibility = Visibility.Hidden;
            RefreshGridData();
        }

        #endregion 
    }
}
