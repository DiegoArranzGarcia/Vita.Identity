namespace Vita.Identity.Infrastructure.Sql.Configuration;

public interface IConnectionStringProvider
{
    string ConnectionString { get; }
}
