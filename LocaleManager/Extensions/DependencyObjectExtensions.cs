using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LocaleManager.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static bool IsValid(this DependencyObject node, bool forceValidation = true)
        {
            if (node != null)
            {
                if (forceValidation && node is TextBox elm)
                {
                    elm.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                }

                if (Validation.GetHasError(node))
                {
                    if (node is IInputElement element) Keyboard.Focus(element);
                    return false;
                }
                foreach (var subnode in LogicalTreeHelper.GetChildren(node))
                {
                    if (subnode is DependencyObject dependencyObject)
                    {
                        if (IsValid(dependencyObject, forceValidation) == false) return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
