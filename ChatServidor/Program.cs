
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServidor
{
    internal class Program
    {
        // Almacena los usuarios conectados
        static ConcurrentDictionary<string, Usuario> usuarios = new();

        const int puerto = 5000;

        static async Task Main(string[] args)
        {
            TcpListener servidor = new TcpListener(
                IPAddress.Any, puerto);

            servidor.Start();

            Console.WriteLine("=================================");
            Console.WriteLine("       SERVIDOR MULTIUSUARIO");
            Console.WriteLine("=================================");
            Console.WriteLine($"Escuchando en puerto {puerto}...");

            // Acepta clientes continuamente
            while (true)
            {
                TcpClient cliente = await servidor.AcceptTcpClientAsync();

                // Cada cliente se atiende de forma independiente
                _ = AtenderCliente(cliente);
            }
        }

        static async Task AtenderCliente(TcpClient cliente)
        {
            Usuario? usuario = null;

            try
            {
                using (cliente)
                using (NetworkStream stream = cliente.GetStream())
                using (StreamReader reader = new StreamReader(
                    stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(
                    stream, new UTF8Encoding(false))
                {
                    AutoFlush = true
                })
                {
                    // El primer mensaje debe ser el nombre de usuario
                    string? nombre = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(nombre) ||
                        nombre.Contains('|') ||
                        nombre.Contains(',') ||
                        !usuarios.TryAdd(nombre,
                            usuario = new Usuario(nombre, writer)))
                    {
                        await writer.WriteLineAsync(
                            "ERROR|Nombre de usuario no disponible");
                        return;
                    }

                    Console.WriteLine($"Usuario {nombre} conectado");

                    await usuario.Enviar(
                        $"INFO|Bienvenido {nombre}");

                    await ActualizarUsuarios();

                    string? mensaje;

                    while ((mensaje = await reader.ReadLineAsync()) != null)
                    {
                        if (mensaje.Equals("CHAO",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        if (mensaje == "LIST")
                        {
                            await usuario.Enviar(
                                "USERS|" + string.Join(",",
                                    usuarios.Keys.OrderBy(x => x)));
                            continue;
                        }

                        // Formato: TO|destinatario|mensaje
                        if (mensaje.StartsWith("TO|"))
                        {
                            string[] partes = mensaje.Split('|', 3);

                            if (partes.Length != 3)
                            {
                                await usuario.Enviar(
                                    "ERROR|Formato de mensaje incorrecto");
                                continue;
                            }

                            string destinatario = partes[1];
                            string texto = partes[2];

                            if (usuarios.TryGetValue(
                                destinatario, out Usuario? destino))
                            {
                                await destino.Enviar(
                                    $"MSG|{nombre}|{texto}");

                                Console.WriteLine(
                                    $"{nombre} -> {destinatario}: {texto}");
                            }
                            else
                            {
                                await usuario.Enviar(
                                    "ERROR|Usuario no encontrado");
                            }
                        }
                        else
                        {
                            await usuario.Enviar(
                                "ERROR|Comando no reconocido");
                        }
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Conexión interrumpida.");
            }
            catch (SocketException)
            {
                Console.WriteLine("Error de conexión.");
            }
            finally
            {
                if (usuario != null)
                {
                    // Elimina únicamente la sesión correspondiente
                    if (usuarios.TryGetValue(usuario.Nombre,
                        out Usuario? registrado) &&
                        ReferenceEquals(registrado, usuario))
                    {
                        usuarios.TryRemove(
                            new KeyValuePair<string, Usuario>(
                                usuario.Nombre, usuario));

                        Console.WriteLine(
                            $"Usuario {usuario.Nombre} desconectado");

                        await ActualizarUsuarios();
                    }
                }

                cliente.Close();
            }
        }

        // Comunica a todos la lista de usuarios conectados
        static async Task ActualizarUsuarios()
        {
            string lista = "USERS|" + string.Join(",",
                usuarios.Keys.OrderBy(x => x));

            foreach (Usuario usuario in usuarios.Values)
            {
                try
                {
                    await usuario.Enviar(lista);
                }
                catch (IOException)
                {
                    // El usuario se eliminará cuando termine su sesión
                }
                catch (ObjectDisposedException)
                {
                    // La conexión ya fue cerrada
                }
            }
        }
    }

    // Representa a cada usuario conectado
    internal class Usuario
    {
        public string Nombre { get; }
        private readonly StreamWriter writer;
        private readonly SemaphoreSlim bloqueo = new(1, 1);

        public Usuario(string nombre, StreamWriter writer)
        {
            Nombre = nombre;
            this.writer = writer;
        }

        public async Task Enviar(string mensaje)
        {
            await bloqueo.WaitAsync();

            try
            {
                await writer.WriteLineAsync(mensaje);
            }
            finally
            {
                bloqueo.Release();
            }
        }
    }
}
