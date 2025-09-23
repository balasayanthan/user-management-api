namespace Api.Auth
{
    public sealed class StaticBearerOptions
    {
        public const string SectionName = "Auth:StaticBearer";
        public List<StaticTokenEntry> Tokens { get; init; } = new();
    }

    public sealed class StaticTokenEntry
    {
        // The literal token clients will send:  Authorization: Bearer <Token>
        public string Token { get; init; } = string.Empty;

        // Cosmetic only (shows up as a claim)
        public string Name { get; init; } = "User";

        // Example values: "perm:CanViewReports", "perm:CanManageUsers"
        public List<string> Claims { get; init; } = new();
    }
}
