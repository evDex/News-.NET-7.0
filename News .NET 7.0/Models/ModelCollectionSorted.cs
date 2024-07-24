using News.BLL.DTO;

namespace News.Models
{
    public class ModelCollectionSorted<T>
    {
        public T SortedCollection { get; set; }
        public string OptionOrderBy { get; set; }
        public string ColumnNameOrderBy { get; set; }
        public string LinkRequest { get; set; }
    }
}
