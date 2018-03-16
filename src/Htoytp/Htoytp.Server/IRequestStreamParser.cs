using System.IO;
using System.Threading.Tasks;

namespace Htoytp.Server
{
    public interface IRequestStreamParser
    {
        Task<IRequestMessage> ParseRequestAsync(Stream requestStream);
    }
}