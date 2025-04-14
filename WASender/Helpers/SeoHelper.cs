using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WASender.Helpers
{
    public static class SeoHelper
    {
        public static void SetHomePageMetadata(ViewDataDictionary viewData, dynamic seo)
        {
            var siteName = seo?.site_name ?? Environment.GetEnvironmentVariable("APP_NAME");
            var description = seo?.matadescription ?? string.Empty;
            var preview = seo?.preview ?? string.Empty;
            var tags = seo?.matatag ?? string.Empty;
            var twitterTitle = seo?.twitter_site_title ?? string.Empty;

            SetCommonMetadata(viewData, siteName, description, preview, tags, twitterTitle);
        }

        public static void SetPageMetadata(ViewDataDictionary viewData, Dictionary<string, string?> data)
        {
            var title = data.ContainsKey("title") ? data["title"] : Environment.GetEnvironmentVariable("APP_NAME");
            var description = data.ContainsKey("description") ? data["description"] : null;
            var preview = data.ContainsKey("preview") ? data["preview"] : null;
            var tags = data.ContainsKey("tags") ? data["tags"] : null;

            SetCommonMetadata(viewData, title, description, preview, tags, null);
        }

        private static void SetCommonMetadata(ViewDataDictionary viewData, string? title, string? description, string? preview, string? tags, string? twitterTitle)
        {
            viewData["MetaTitle"] = title;
            viewData["MetaDescription"] = description;
            viewData["MetaKeywords"] = tags;
            viewData["MetaImage"] = preview;
            viewData["TwitterTitle"] = twitterTitle ?? title;
        }
    }
}
