namespace do9Rename.Core
{
    public interface IRenameCommand
    {
        string Execute(string input,int index);

        string ToString(bool isDisplayText = false);
    }
}
