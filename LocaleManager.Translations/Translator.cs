using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleTranslateFreeApi;

namespace LocaleManager.Translations
{
    public class Translator
    {
        private readonly GoogleTranslator _client = new GoogleTranslator();
        public async Task<string> Translate(string text, string to, string from = "en")
        {
            var fromLang = GoogleTranslator.GetLanguageByISO(from) ?? Language.Auto;;
            var toLang = GoogleTranslator.GetLanguageByISO(to);
            if (fromLang == null || toLang == null)
            {
                return null;
            }
            var result = await _client.TranslateLiteAsync(text, fromLang, toLang);

            return result.MergedTranslation;
        }
    }
}
