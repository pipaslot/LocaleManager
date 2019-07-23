using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LocaleManager.Extensions
{
    public static class DataGridColumnExtensions
    {
        public static List<string> GetLocales(this ObservableCollection<DataGridColumn> columns)
        {
            return columns
                .Skip(1)
                .Cast<DataGridTextColumn>()
                .Select(c => c.Header.ToString())
                .ToList();
        }
    }
}
