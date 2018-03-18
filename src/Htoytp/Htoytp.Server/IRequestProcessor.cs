using System.IO;
using System.Threading.Tasks;

namespace Htoytp.Server
{
    public interface IRequestProcessor
    {
        Task<ResponseMessage> ProcessAsync(RequestMessage request);
        IRequestProcessor AddMiddleware(IMiddleware middleware);
    }
}