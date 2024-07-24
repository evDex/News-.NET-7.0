using System.IO;

namespace News.Infrastructure.Common
{
    public class DeleteFile
    {
        public static void DeleteFileAt(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
