using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetXMLData
{
    public static class Util
    {
        #region convert font

        private static char[] convertTable = null;

        private static char[] tcvnchars = {
        'µ', '¸', '¶', '·', '¹', 
        '¨', '»', '¾', '¼', '½', 'Æ', 
        '©', 'Ç', 'Ê', 'È', 'É', 'Ë', 
        '®', 'Ì', 'Ð', 'Î', 'Ï', 'Ñ', 
        'ª', 'Ò', 'Õ', 'Ó', 'Ô', 'Ö', 
        '×', 'Ý', 'Ø', 'Ü', 'Þ', 
        'ß', 'ã', 'á', 'â', 'ä', 
        '«', 'å', 'è', 'æ', 'ç', 'é', 
        '¬', 'ê', 'í', 'ë', 'ì', 'î', 
        'ï', 'ó', 'ñ', 'ò', 'ô', 
        '­', 'õ', 'ø', 'ö', '÷', 'ù', 
        'ú', 'ý', 'û', 'ü', 'þ', 
        '¡', '¢', '§', '£', '¤', '¥', '¦'
        };
        private static char[] unichars = {
        'à', 'á', 'ả', 'ã', 'ạ', 
        'ă', 'ằ', 'ắ', 'ẳ', 'ẵ', 'ặ', 
        'â', 'ầ', 'ấ', 'ẩ', 'ẫ', 'ậ', 
        'đ', 'è', 'é', 'ẻ', 'ẽ', 'ẹ', 
        'ê', 'ề', 'ế', 'ể', 'ễ', 'ệ', 
        'ì', 'í', 'ỉ', 'ĩ', 'ị', 
        'ò', 'ó', 'ỏ', 'õ', 'ọ', 
        'ô', 'ồ', 'ố', 'ổ', 'ỗ', 'ộ', 
        'ơ', 'ờ', 'ớ', 'ở', 'ỡ', 'ợ', 
        'ù', 'ú', 'ủ', 'ũ', 'ụ', 
        'ư', 'ừ', 'ứ', 'ử', 'ữ', 'ự', 
        'ỳ', 'ý', 'ỷ', 'ỹ', 'ỵ', 
        'Ă', 'Â', 'Đ', 'Ê', 'Ô', 'Ơ', 'Ư'
        };
        private static void InitConvert()
        {
            convertTable = new char[256];
            for (int i = 0; i < 256; i++)
                convertTable[i] = (char)i;
            for (int i = 0; i < tcvnchars.Length; i++)
                convertTable[tcvnchars[i]] = unichars[i];
        }
        public static string convertTCVN3ToUnicode(string value)
        {
            if (convertTable == null)
                InitConvert();
            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (chars[i] < (char)256)
                {
                    chars[i] = convertTable[chars[i]];
                }

            return new string(chars);
        }
        private static void PublishService()
        {
            if (null == convertTable)
            {
                convertTable = new char[256];
                for (int i = 0; i < 256; i++)
                    convertTable[i] = (char)i;
                for (int i = 0; i < tcvnchars.Length; i++)
                    convertTable[tcvnchars[i]] = unichars[i];
            }
        }
        #endregion

        //public static string convertSpecialCharacter(string xmlData)
        //{
        //    string rv = "";
        //    rv = xmlData.Replace("&", "&amp;");
        //    rv = rv.Replace("<", "&lt;");
        //    rv = rv.Replace(">", "&gt;");           
        //    return RemoveControlCharacters(rv);
        //}
        public static string convertSpecialCharacter(string xmlData)
        {
            //return "<![CDATA[" + xmlData + "]]>";
            return string.Format("<![CDATA[{0}]]>", xmlData);
        }

        public static string RemoveControlCharacters(string inString)
        {
            if (inString == null) return null;

            StringBuilder newString = new StringBuilder();
            char ch;

            for (int i = 0; i < inString.Length; i++)
            {

                ch = inString[i];

                if (!char.IsControl(ch))
                {
                    newString.Append(ch);
                }
            }
            return newString.ToString();
        }

    }
}
