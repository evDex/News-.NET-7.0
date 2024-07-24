using Microsoft.Data.SqlClient;

namespace News.DAL.Dapper
{
    public class NewsEntities
    {
        public static SqlConnection Connection()
        {
            return new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=NewsEntities;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}
