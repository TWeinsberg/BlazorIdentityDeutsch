namespace BlazorIdentityDeutsch.Data {

    // Step2.1: Rollenmanagement konfigurieren
    public static class ApplicationRoles {
        public const string Admin = "Admin";
        public const string Registered = "Registered";
        public const string Guest = "Guest";

        public static readonly string[] All =
        [
            Admin,
            Registered,
            Guest
        ];
    }
}
