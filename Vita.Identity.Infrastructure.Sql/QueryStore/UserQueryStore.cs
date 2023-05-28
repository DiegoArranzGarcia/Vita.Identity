using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Vita.Identity.Application.Query.Users;
using Vita.Identity.Infrastructure.Sql.Configuration;

namespace Vita.Identity.Infrastructure.Sql.QueryStore;

public class UserQueryStore : IUserQueryStore
{              
    private readonly IConnectionStringProvider _connectionStringProvider;

    public UserQueryStore(IConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider ?? throw new ArgumentNullException(nameof(connectionStringProvider));
    }

    public async Task<UserDto> GetUserByEmail(string email)
    {
        const string query = "Select Id, Email, GivenName, FamilyName  from Users where Email = @Email;";

        using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
        connection.Open();

        return await connection.QueryFirstOrDefaultAsync<UserDto>(query, new { email });
    }

    public async Task<UserDto> GetUserById(Guid id)
    {
        const string query = "Select Id, Email, GivenName, FamilyName from Users where Id = @Id;";

        using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
        connection.Open();

        return await connection.QueryFirstOrDefaultAsync<UserDto>(query, new { id });
    }

    public async Task<IEnumerable<ExternalLoginProviderDto>> GetUserExternalLoginProviders(Guid id)
    {
        const string query = "Select Id, ExternalUserId, Name from LoginProviders where UserId = @Id";

        using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
        connection.Open();

        return await connection.QueryAsync<ExternalLoginProviderDto>(query, new { id });
    }

    public async Task<AccessTokenDto> GetUserAccessToken(Guid id, Guid loginProviderId)
    {
        const string query = "Select Token from LoginProviders where UserId = @Id and Id = @loginProviderId";

        using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
        connection.Open();

        return await connection.QueryFirstAsync<AccessTokenDto>(query, new { id, loginProviderId });
    }

}
