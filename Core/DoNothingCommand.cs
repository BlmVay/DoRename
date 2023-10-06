namespace do9Rename.Core
{
    internal class DoNothingCommand : IRenameCommand
    {
        public string Execute(string input, int index)
        {
            return input;
        }

        public override string ToString()
        {
            return "无操作";
        }

        public string ToString(bool isDisplayText = false)
        {
            return isDisplayText ? base.ToString() : "DoNothingCommand[]";
        }
    }
}
