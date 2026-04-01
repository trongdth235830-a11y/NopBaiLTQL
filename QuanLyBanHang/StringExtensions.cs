using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuanLyBanHang
{
    public  static class StringExtensions
    {
        // Hàm chuyển đổi chuỗi có dấu thành không dấu (Slug)
        public static string GenerateSlug(this string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";

            string str = phrase.ToLower();

            // Xóa dấu tiếng Việt
            str = Regex.Replace(str, @"[áàảãạâấầẩẫậăắằẳẵặ]", "a");
            str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, @"[iíìỉĩị]", "i");
            str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            str = Regex.Replace(str, @"[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, @"[ýỳỷỹỵ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");

            // Xóa ký tự đặc biệt, chỉ giữ lại chữ cái, số và dấu gạch ngang
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // Chuyển khoảng trắng thành dấu gạch ngang
            str = Regex.Replace(str, @"\s+", "-").Trim();

            return str;
        }
    }
}
