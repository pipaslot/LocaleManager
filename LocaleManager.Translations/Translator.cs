using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GoogleTranslateFreeApi;

namespace LocaleManager.Translations
{
    public class Translator
    {
        private readonly GoogleTranslator _client = new GoogleTranslator();
        public async Task<Result> Translate(string text, string to, string from = "en")
        {
            try
            {
                var fromLang = GoogleTranslator.GetLanguageByISO(from) ?? Language.Auto; ;
                var toLang = GoogleTranslator.GetLanguageByISO(to);
                if (fromLang == null || toLang == null)
                {
                    return new Result(ErrorCode.LanguageNotRecognized);
                }

                var result = await _client.TranslateLiteAsync(text, fromLang, toLang);
                return new Result(result.MergedTranslation);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Error: "+ex.Message);
                if (ex.Message.Contains("429"))
                {
                    return new Result(ErrorCode.TooManyRequests);
                }
                return new Result(ErrorCode.UnknownError);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: "+ex.Message);
                return new Result(ErrorCode.UnknownError);
            }
        }
    }

    public class Result
    {
        public bool Success => ErrorCode == ErrorCode.NoError;
        public string Translation { get; set; }
        public ErrorCode ErrorCode { get; set; }

        public Result(string translation)
        {
            Translation = translation;
        }
        public Result(ErrorCode code)
        {
            ErrorCode = code;
        }
    }

    public enum ErrorCode
    {
        NoError = 0,
        LanguageNotRecognized = 1,
        TooManyRequests = 2,
        UnknownError = 3
    }
}
