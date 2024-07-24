namespace News.Models
{
    public enum TagName
    {
        Text,
        Image
    }
    public class TagTextView
    {
        public TagName TagName { get; set; }
        public string Value { get; set; }
    }
}
