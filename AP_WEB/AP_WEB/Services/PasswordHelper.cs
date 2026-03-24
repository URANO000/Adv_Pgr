using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace AP_WEB.Services
{
    public class PasswordHelper : IPasswordHelper
    {
        private readonly IConfiguration _config;
        public PasswordHelper(IConfiguration config)
        {
            _config = config;
        }

        public string Encrypt(string texto)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("G7kP2mX9Qa4ZtL8wR1bY6HcD3sN5uFjV");
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(texto);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }

        public void EnviarCorreo(string destinatario, string asunto, string contenido)
        {
            var host = _config.GetValue<string>("Smtp:Host");
            var port = _config.GetValue<int>("Smtp:Port");
            var usuario = _config.GetValue<string>("Smtp:Usuario");
            var contrasenna = _config.GetValue<string>("Smtp:Contrasenna");

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(usuario, contrasenna),
                EnableSsl = true
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(usuario!),
                Subject = asunto,
                Body = contenido,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            if (!string.IsNullOrEmpty(contrasenna))
            {
                smtp.Send(mensaje);
            }

        }

    }
}
