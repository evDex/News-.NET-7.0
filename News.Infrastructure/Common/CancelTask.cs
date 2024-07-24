
using System.Threading;


namespace News.Infrastructure.Common
{
    public class CancelTask
    {
        public static CancellationToken GetToken()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            return token;
        }
    }
}
