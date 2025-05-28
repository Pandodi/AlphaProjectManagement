using Domain.Models;

namespace Business.Models;

public class ClientResult : ServiceResult
{
    public IEnumerable<Client>? Result { get; set; }
}

public class ClientResult<T> : ServiceResult
{
    public T? Result { get; set; }
}
