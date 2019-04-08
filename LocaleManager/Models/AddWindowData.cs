using System.Collections.Generic;

namespace LocaleManager.Models
{
    public class AddWindowData
    {
        public AddWindowData(List<TranslationData> translations)
        {
            Translations = translations;
        }

        public string Key { get; set; } = "";

        public List<TranslationData> Translations { get; set; }


        public class TranslationData
        {
            public string Locale { get; set; }
            public string Text { get; set; }

            public TranslationData(string locale, string text)
            {
                Locale = locale;
                Text = text;
            }
        }
    }
}