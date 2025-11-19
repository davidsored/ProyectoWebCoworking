using System.Diagnostics;

namespace ProyectoWebCoworking.Services
{
    public class FakeEmailService : IEmailService
    {
        public Task SendEmailAsync(string emailDestino, string asunto, string mensaje)
        {
            Debug.WriteLine("-----------------------------");
            Debug.WriteLine($"SIMULANDO ENVÍO DE CORREO");
            Debug.WriteLine($"Para: {emailDestino}");
            Debug.WriteLine($"Asunto: {asunto}");
            Debug.WriteLine($"Mensaje: {mensaje}");
            Debug.WriteLine("-----------------------------");

            return Task.CompletedTask;
        }
    }
}
