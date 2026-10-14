// Fake Mailserver Dienst zu Entwicklung
// https://ethereal.email/create //

namespace BlazorIdentityDeutsch.Models {
    public sealed class SmtpSettings {
        public bool Enabled { get; set; } = false;
        public string Host { get; set; } = "smtp.ethereal.email";
        public int Port { get; set; } = 587;
        public SecureSocketOptions? SslOptions { get; set; } = SecureSocketOptions.Auto;
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";

        public string SenderEmail { get; set; } = "";
        public string SenderName { get; set; } = "PROJECT_DISPLAY_NAME";
    }
}
