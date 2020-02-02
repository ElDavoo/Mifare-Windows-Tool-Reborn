namespace MifareWindowsTool.Properties
{
    using Gu.Localization;
    using Gu.Localization.Properties;

    public static class Translate
    {
        /// <summary>Call like this: Translate.Key(nameof(Resources.Saved_file__0_)).</summary>
        /// <param name="key">A key in Properties.Resources</param>
        /// <param name="errorHandling">How to handle translation errors like missing key or culture.</param>
        /// <returns>A translation for the key.</returns>
        public static string Key(string key, ErrorHandling errorHandling = ErrorHandling.ReturnErrorInfoPreserveNeutral)
        {
            return TranslationFor(key, errorHandling).Translated;
        }

        /// <summary>Call like this: Translate.Key(nameof(Resources.Saved_file__0_)).</summary>
        /// <param name="key">A key in Properties.Resources</param>
        /// <param name="errorHandling">How to handle translation errors like missing key or culture.</param>
        /// <returns>A translation for the key.</returns>
        public static ITranslation TranslationFor(string key, ErrorHandling errorHandling = ErrorHandling.ReturnErrorInfoPreserveNeutral)
        {
            return Gu.Localization.Translation.GetOrCreate(Resources.ResourceManager, key, errorHandling);
        }
    }
}
