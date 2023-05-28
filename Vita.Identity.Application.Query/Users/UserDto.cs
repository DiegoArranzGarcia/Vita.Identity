namespace Vita.Identity.Application.Query.Users
{
    public record UserDto
    {
        public Guid Id { get; init; }
        public string Email { get; init; }
        public string GivenName { get; init; }
        public string FamilyName { get; init; }
    }
}