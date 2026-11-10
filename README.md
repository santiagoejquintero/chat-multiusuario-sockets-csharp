# Chat Multiusuario con Sockets TCP en C#

Aplicación de chat multiusuario desarrollada en **C# y .NET 9**, utilizando sockets TCP y programación asíncrona para permitir la comunicación entre múltiples clientes conectados a un servidor central.

El proyecto fue desarrollado como parte de la asignatura **Persistencia de Datos y Transaccionalidad** del Politécnico Grancolombiano.

## Objetivo

Implementar un sistema de comunicación bidireccional mediante una arquitectura cliente-servidor, permitiendo que diferentes usuarios intercambien mensajes en tiempo real y que el servidor administre sus conexiones.

## Tecnologías utilizadas

- **Lenguaje:** C#
- **Framework:** .NET 9
- **Entorno de desarrollo:** Visual Studio Code
- **Protocolo de comunicación:** TCP/IP
- **Bibliotecas:** System.Net, System.Net.Sockets, System.Text y System.Collections.Concurrent
- **Programación asíncrona:** async, await y Task

## Funcionalidades

- Conexión simultánea de múltiples clientes.
- Registro e identificación mediante nombres de usuario.
- Validación para evitar nombres duplicados.
- Consulta de usuarios conectados.
- Envío de mensajes privados entre usuarios.
- Comunicación bidireccional en tiempo real.
- Manejo de errores cuando un destinatario no existe.
- Desconexión voluntaria mediante el comando `chao`.
- Actualización automática de la lista de usuarios.
- Reconexión sin necesidad de reiniciar el servidor.

## Arquitectura del proyecto

La aplicación utiliza una arquitectura cliente-servidor.

**ChatServidor:** recibe y administra las conexiones, registra los usuarios activos y transmite los mensajes al destinatario correspondiente.

**ChatCliente:** permite conectarse al servidor, identificarse, consultar usuarios, enviar y recibir mensajes.

Los programas se comunican mediante sockets TCP utilizando un protocolo sencillo basado en mensajes de texto.

## Estructura del proyecto

```text
ChatSockets/
├── ChatServidor/
│   ├── Program.cs
│   └── ChatServidor.csproj
│
├── ChatCliente/
│   ├── Program.cs
│   └── ChatCliente.csproj
│
└── README.md
```

## Instalación y ejecución

### Requisitos

- .NET SDK 9
- Visual Studio Code o un entorno compatible con C#
- Windows, Linux o macOS con soporte para .NET 9

### 1. Clonar el repositorio

```bash
git clone https://github.com/USUARIO/chat-multiusuario-sockets-csharp.git
cd chat-multiusuario-sockets-csharp
```

Reemplaza `USUARIO` por el propietario real del repositorio.

### 2. Ejecutar el servidor

Desde la carpeta principal, abre una terminal y ejecuta:

```bash
dotnet run --project ChatServidor
```

El servidor comenzará a escuchar conexiones en el puerto `5000`.

### 3. Ejecutar los clientes

Abre otra terminal y ejecuta:

```bash
dotnet run --project ChatCliente
```

Ingresa un nombre de usuario y utiliza la dirección IP predeterminada `127.0.0.1` para conectarte localmente.

Para conectar más participantes, abre nuevas terminales y ejecuta nuevamente el cliente con nombres diferentes.

## Comandos disponibles

| Comando | Descripción |
|---|---|
| `/usuarios` | Muestra los usuarios conectados. |
| `/para usuario mensaje` | Envía un mensaje privado al usuario indicado. |
| `chao` | Finaliza la conexión del cliente. |

### Ejemplo de conversación

Usuario Santiago:

```text
/para Andres Hola Andres, ¿cómo estás?
[TÚ -> Andres]: Hola Andres, ¿cómo estás?
```

Usuario Andres:

```text
[Santiago]: Hola Andres, ¿cómo estás?
/para Santiago Hola Santiago, todo bien.
```

## Configuración de red

Para las pruebas locales se utilizó:

- **Dirección IP:** 127.0.0.1 (localhost)
- **Puerto:** 5000
- **Protocolo:** TCP

El servidor utiliza `IPAddress.Any` para escuchar conexiones IPv4 en las interfaces disponibles.

Para conectar equipos diferentes es necesario utilizar la dirección IP alcanzable del servidor y permitir el tráfico correspondiente en el firewall.

Se recomienda utilizar el proyecto en un entorno de prueba o red de confianza.

## Pruebas realizadas

Se verificaron los siguientes escenarios:

1. Conexión de múltiples clientes al servidor.
2. Registro de usuarios con nombres diferentes.
3. Consulta de la lista de participantes conectados.
4. Envío y recepción de mensajes privados.
5. Rechazo de nombres de usuario duplicados.
6. Manejo de mensajes enviados a usuarios inexistentes.
7. Desconexión individual mediante `chao`.
8. Reconexión de usuarios sin reiniciar el servidor.

Las pruebas funcionales se realizaron ejecutando el servidor y varios clientes desde terminales independientes.

## Conceptos aplicados

Este proyecto permitió trabajar con:

- Programación de sockets TCP.
- Arquitectura cliente-servidor.
- Programación asíncrona.
- Administración de conexiones concurrentes.
- Manejo de excepciones.
- Transmisión de información mediante flujos de datos.
- Colecciones concurrentes y sincronización.

