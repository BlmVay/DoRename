using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace do9Rename.Core
{
    internal class AddNumberSuffixCommand : IRenameCommand
    {

        private int count;
        private int insterPosition;

        public AddNumberSuffixCommand(int count, int insterPosition)
        {
            this.count = count;
            this.insterPosition = insterPosition;
        }

        public string Execute(string input, int index)
        {
            string formattedNumber = index.ToString().PadLeft(count, '0');
            var startIndex = insterPosition <= input.Length ? insterPosition : input.Length;
            string ret = input.Insert(startIndex, formattedNumber);
            return ret;
        }

        public string ToString(bool isDisplayText = false)
        {
            return "添加末尾数字";
        }
    }
}
