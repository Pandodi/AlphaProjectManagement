using Domain.Models;

namespace Business.Models;

public class MemberResult : ServiceResult
{
    public IEnumerable<Member>? Result { get; set; }
}

public class MemberResult<T> : ServiceResult
{
    public T? Result { get; set; }
}

