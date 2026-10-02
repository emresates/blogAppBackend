using System.Text;
using System.Text.RegularExpressions;

namespace BlogApi.Helpers;

public static class SlugHelper
{
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        var slug = text
            .Trim()
            .ToLowerInvariant();

        slug = slug
            .Replace("ç", "c")
            .Replace("ğ", "g")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ş", "s")
            .Replace("ü", "u");

        slug = slug.Normalize(
            NormalizationForm.FormD
        );

        slug = string.Concat(
            slug.Where(
                c =>
                    System.Globalization.CharUnicodeInfo
                        .GetUnicodeCategory(c)
                    !=
                    System.Globalization.UnicodeCategory
                        .NonSpacingMark
            )
        );

        slug = slug.Normalize(
            NormalizationForm.FormC
        );

        slug = Regex.Replace(
            slug,
            @"[^a-z0-9\s-]",
            ""
        );

        slug = Regex.Replace(
            slug,
            @"\s+",
            "-"
        );

        slug = Regex.Replace(
            slug,
            @"-+",
            "-"
        );

        return slug.Trim('-');
    }
}