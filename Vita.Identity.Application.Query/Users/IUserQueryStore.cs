namespace Vita.Identity.Application.Query.Users
{
    public interface IUserQueryStore
    {
        public Task<UserDto> GetUserByEmail(string email);
        public Task<UserDto> GetUserById(Guid id);
        public Task<IEnumerable<ExternalLoginProviderDto>> GetUserExternalLoginProviders(Guid id);
        public Task<AccessTokenDto> GetUserAccessToken(Guid id, Guid loginProviderId);
    }
}
