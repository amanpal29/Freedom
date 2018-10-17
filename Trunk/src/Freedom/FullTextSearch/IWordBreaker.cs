namespace Freedom.FullTextSearch
{
    public interface IWordBreaker
    {
        string[] BreakText(string text);
    }
}