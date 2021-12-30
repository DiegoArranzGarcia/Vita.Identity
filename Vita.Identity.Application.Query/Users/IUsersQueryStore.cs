namespace Vita.Identity.Application.Query.Users
{
    public interface IUsersQueryStore
    {
        public Task<UserDto> GetUserByEmail(string email);
        public Task<UserDto> GetUserById(Guid id);
    }
}
