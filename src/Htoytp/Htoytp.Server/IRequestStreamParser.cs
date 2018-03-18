using System.IO;
using System.Threading.Tasks;

namespace Htoytp.Server
{
    public interface IRequestStreamParser
    {
        Task<RequestMessage> ParseRequestAsync(Stream requestStream);
    }
}