using OwaspHeaders.Core.Models;

namespace OwaspHeaders.IsolatedFunction;

public interface IOwaspMiddlewareConfigurationProvider
{
    SecureHeadersMiddlewareConfiguration CustomConfiguration();
}