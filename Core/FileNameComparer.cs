using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace do9Rename.Core
{
    public class FileNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            // 使用正则表达式提取文件名中的字母和数字部分
            string xAlphaPart = Regex.Match(x, "[a-zA-Z]+").Value;
            string yAlphaPart = Regex.Match(y, "[a-zA-Z]+").Value;

            string xNumPart = Regex.Replace(x, "[^0-9]", "");
            string yNumPart = Regex.Replace(y, "[^0-9]", "");

            // 如果字母部分不同，则按字母部分的字符串排序
            int result = xAlphaPart.CompareTo(yAlphaPart);
            if (result != 0)
            {
                return result;
            }

            // 将数字部分解析为整数
            int xNum = int.Parse(xNumPart);
            int yNum = int.Parse(yNumPart);

            // 比较数字部分
            return xNum.CompareTo(yNum);
        }
    }
}
