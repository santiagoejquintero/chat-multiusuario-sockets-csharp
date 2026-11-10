
using System.Net.Sockets;
using System.Text;

namespace ChatCliente
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("       CHAT MULTIUSUARIO");
            Console.WriteLine("=================================");

            Console.Write("Nombre de usuario: ");
            string? nombre = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(nombre) ||
                nombre.Contains('|') ||
                nombre.Contains(','))
            {
                Console.WriteLine("Nombre de usuario no válido.");
                return;
            }

            Console.Write("IP del servidor [127.0.0.1]: ");
            string? ip = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(ip))
                ip = "127.0.0.1";

            const int puerto = 5000;

            using TcpClient cliente = new TcpClient();

            try
            {
                await cliente.ConnectAsync(ip, puerto);

                using NetworkStream stream = cliente.GetStream();
                using StreamReader reader =
                    new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer =
                    new StreamWriter(stream, new UTF8Encoding(false))
                    {
                        AutoFlush = true
                    };

                // Identificarse ante el servidor
                await writer.WriteLineAsync(nombre);

                Console.WriteLine("\nConectado al servidor.");
                Console.WriteLine("Comandos disponibles:");
                Console.WriteLine("  /usuarios - Mostrar usuarios conectados");
                Console.WriteLine("  /para usuario mensaje - Enviar mensaje");
                Console.WriteLine("  chao - Desconectarse\n");

                using CancellationTokenSource cts = new();

                // Recibe mensajes mientras el usuario escribe
                async Task RecibirMensajes()
                {
                    while (!cts.IsCancellationRequested)
                    {
                        string? respuesta =
                            await reader.ReadLineAsync(cts.Token);

                        if (respuesta == null)
                        {
                            Console.WriteLine(
                                "\nServidor desconectado.");
                            break;
                        }

                        string[] partes = respuesta.Split('|', 3);

                        switch (partes[0])
                        {
                            case "INFO":
                                Console.WriteLine(
                                    $"\n[SERVIDOR] {respuesta.Substring(5)}");
                                break;

                            case "USERS":
                                Console.WriteLine(
                                    $"\n[USUARIOS CONECTADOS] " +
                                    respuesta.Substring(6));
                                break;

                            case "MSG":
                                if (partes.Length == 3)
                                {
                                    Console.WriteLine(
                                        $"\n[{partes[1]}]: {partes[2]}");
                                }
                                break;

                            case "ERROR":
                                Console.WriteLine(
                                    $"\n[ERROR] {respuesta.Substring(6)}");
                                break;
                        }
                    }
                }

                // Envía comandos y mensajes
                async Task EnviarMensajes()
                {
                    while (!cts.IsCancellationRequested)
                    {
                        string? entrada = Console.ReadLine();

                        if (entrada == null)
                            break;

                        if (entrada.Equals("chao",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            await writer.WriteLineAsync("CHAO");
                            break;
                        }

                        if (entrada == "/usuarios")
                        {
                            await writer.WriteLineAsync("LIST");
                            continue;
                        }

                        if (entrada.StartsWith("/para "))
                        {
                            string[] datos = entrada.Split(' ', 3);

                            if (datos.Length < 3 ||
                                string.IsNullOrWhiteSpace(datos[2]))
                            {
                                Console.WriteLine(
                                    "Uso: /para usuario mensaje");
                                continue;
                            }

                            await writer.WriteLineAsync(
                                $"TO|{datos[1]}|{datos[2]}");

                            Console.WriteLine(
                                $"[TÚ -> {datos[1]}]: {datos[2]}");
                        }
                        else
                        {
                            Console.WriteLine(
                                "Comando no reconocido.");
                        }
                    }
                }

                Task recibir = RecibirMensajes();
                Task enviar = EnviarMensajes();

                await Task.WhenAny(recibir, enviar);

                cts.Cancel();
                cliente.Close();

                try
                {
                    await Task.WhenAll(recibir, enviar);
                }
                catch (Exception ex) when (
                    ex is OperationCanceledException ||
                    ex is IOException ||
                    ex is ObjectDisposedException)
                {
                    // Cierre normal
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nCliente finalizado.");
            }
        }
    }
}
