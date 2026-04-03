using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FootballBlog.Core.Services;

/// <summary>Chuyển title tiếng Việt thành URL slug chuẩn SEO.</summary>
public static class SlugService
{
    private static readonly Dictionary<string, string> VietnameseMap = new()
    {
        { "à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ", "a" },
        { "è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ", "e" },
        { "ì|í|ị|ỉ|ĩ", "i" },
        { "ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ", "o" },
        { "ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ", "u" },
        { "ỳ|ý|ỵ|ỷ|ỹ", "y" },
        { "đ", "d" },
        { "À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ", "a" },
        { "È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ", "e" },
        { "Ì|Í|Ị|Ỉ|Ĩ", "i" },
        { "Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ", "o" },
        { "Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ", "u" },
        { "Ỳ|Ý|Ỵ|Ỷ|Ỹ", "y" },
        { "Đ", "d" },
    };

    /// <summary>
    /// Tạo slug từ title — hỗ trợ tiếng Việt.
    /// Ví dụ: "Trận Đấu Hấp Dẫn 2025!" → "tran-dau-hap-dan-2025"
    /// </summary>
    public static string Generate(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var slug = title.ToLowerInvariant();

        // Thay thế ký tự tiếng Việt
        foreach (var (pattern, replacement) in VietnameseMap)
        {
            slug = Regex.Replace(slug, pattern, replacement);
        }

        // Chuẩn hóa Unicode còn lại (ký tự có dấu khác)
        slug = slug.Normalize(NormalizationForm.FormD);
        var chars = slug.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        slug = new string(chars.ToArray()).Normalize(NormalizationForm.FormC);

        // Chỉ giữ chữ thường, số và dấu gạch ngang
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Thay khoảng trắng và nhiều dấu gạch liên tiếp thành 1 dấu
        slug = Regex.Replace(slug, @"[\s-]+", "-");

        return slug.Trim('-');
    }

    /// <summary>
    /// Tạo slug unique bằng cách thêm suffix số nếu slug đã tồn tại.
    /// Ví dụ: "tran-dau" đã có → "tran-dau-2"
    /// </summary>
    public static string GenerateUnique(string title, IEnumerable<string> existingSlugs)
    {
        var baseSlug = Generate(title);
        var existing = existingSlugs.ToHashSet();

        if (!existing.Contains(baseSlug))
        {
            return baseSlug;
        }

        var suffix = 2;
        string candidate;
        do { candidate = $"{baseSlug}-{suffix++}"; }
        while (existing.Contains(candidate));

        return candidate;
    }
}
