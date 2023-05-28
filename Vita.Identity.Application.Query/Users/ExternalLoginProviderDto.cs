namespace Vita.Identity.Application.Query.Users
{
    public record ExternalLoginProviderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ExternalUserId { get; set; }
    }
}
