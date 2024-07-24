
namespace News.BLL.Infrastructure
{
    public class BaseResponse<T> : IBaseResponse<T>
    {
        public string Description { get; set; }
        public StatusCode StatusCode { get; set; } = StatusCode.Error;
        public T Data { get; set; }
    }
    public interface IBaseResponse<T>
    {
        T Data { get; set; }
        string Description { get; set; }
        StatusCode StatusCode { get; set; } 
    }
    public enum StatusCode
    {
        OK = 200,
        IternalServerError = 500,
        Error = 500
    }
}
