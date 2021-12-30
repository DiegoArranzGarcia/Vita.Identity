using Dapper;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Vita.Identity.Application.Query.Users;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Infrastructure.Sql.Configuration;

namespace Vita.Identity.Infrastructure.Sql.QueryStore
{
    public class UserQueryStore : IUsersQueryStore
    {
        private const string GetUserByEmailQuery = "Select Id, Email from Users where Email = @Email;";

        private readonly IUsersRepository _usersRepository;
        private readonly IConnectionStringProvider _connectionStringProvider;

        public UserQueryStore(IUsersRepository usersRepository, IConnectionStringProvider connectionStringProvider)
        {
            _usersRepository = usersRepository;
            _connectionStringProvider = connectionStringProvider ?? throw new ArgumentNullException(nameof(connectionStringProvider));

        }
        public async Task<UserDto> GetUserByEmail(string email)
        {
            using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
            connection.Open();

            return await connection.QueryFirstOrDefaultAsync<UserDto>(GetUserByEmailQuery, new { email });
        }

        public async Task<UserDto> GetUserById(Guid id)
        {
            var user = await _usersRepository.FindByIdAsync(id);
            return ToUserDto(user);
        }

        private static UserDto ToUserDto(User user)
        {
            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email.Address,
                GivenName = user.GivenName,
                FamilyName = user.FamilyName,
                UserName = user.UserName,
            };
        }
    }
}
