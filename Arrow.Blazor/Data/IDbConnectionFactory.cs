using System.Data;

namespace Arrow.Blazor.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
