# PruebaCapitole

Micro-MVP de gestión de reservas de recursos (salas, puestos, vehículos...).
Backend .NET con arquitectura hexagonal, DDD, CQRS y PostgreSQL; eventos con RabbitMQ (Outbox + Worker de notificaciones); frontal en Angular.

## Requisitos

| Herramienta | Para qué |
|---|---|
| [Git](https://git-scm.com/) | Clonar el proyecto |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Levantar todo el sistema y ejecutar los tests de integración |
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Solo si ejecutas la API o los tests fuera de Docker |
| [Node.js LTS](https://nodejs.org/) | Solo si ejecutas el frontal fuera de Docker |

> **Por qué .NET 10 y no .NET 8:** el enunciado menciona .NET 8, pero .NET 8 y .NET 9 ya no tienen soporte (su soporte terminó en noviembre de 2026). Se eligió .NET 10 (LTS) para no construir sobre una versión sin parches de seguridad.

## Cómo ejecutarlo (un solo comando)

Desde la carpeta raíz del repositorio:

```
cp .env.example .env
docker compose up -d --build
```

En PowerShell usa `copy .env.example .env`. El `.env` guarda usuarios y contraseñas de desarrollo y no se sube a git.

Las migraciones ya están en el repositorio: la API y el Worker crean sus tablas solos al arrancar (entorno Development, que es el que usa el compose). También se insertan datos de ejemplo (ver [Datos de ejemplo](#datos-de-ejemplo)).

| Servicio | URL |
|---|---|
| Frontal (Angular) | <http://localhost:8081> |
| API (Scalar) | <http://localhost:8080/scalar> |
| Documento OpenAPI | <http://localhost:8080/openapi/v1.json> |
| Salud de la API | <http://localhost:8080/health> |
| Salud del Worker | <http://localhost:8082/health> |
| RabbitMQ (consola) | <http://localhost:15672> (usuario y contraseña en `.env`) |
| pgAdmin | <http://localhost:5050> (`admin@admin.com` / `admin`) |

- `/health` de la API revisa PostgreSQL y RabbitMQ: responde 200 si todo funciona y 503 si algo falla.
- El nginx del frontal reenvía `/api` a la API, así que en Docker el navegador usa un solo origen.
- **CORS:** la API permite los orígenes de `Cors:AllowedOrigins` (`appsettings.json`): `http://localhost:4200` (`ng serve`) y `http://localhost:8081` (frontal en Docker). Si sirves el frontal desde otro origen, añádelo ahí.

Para detener todo: `docker compose down` (añade `-v` para borrar también los datos).

## Ejecutar sin Docker para la aplicación

Útil para depurar. Solo la infraestructura va en Docker:

```
docker compose up -d db pgadmin rabbitmq
dotnet run --project src/PruebaCapitole.Api --urls http://localhost:8080
dotnet run --project src/PruebaCapitole.Notifications.Worker
cd web && npm install && npm start
```

La API usa el puerto 8080 porque el proxy de desarrollo del frontal (`web/proxy.conf.json`) apunta ahí. El frontal queda en <http://localhost:4200>.

## Migraciones (solo si cambias el modelo)

Instala `dotnet-ef` una vez: `dotnet tool install --global dotnet-ef`. Después, para la API y para el Worker:

```
dotnet ef migrations add <Nombre> --project src/PruebaCapitole.Infraestructure --startup-project src/PruebaCapitole.Api
dotnet ef migrations add <Nombre> --project src/PruebaCapitole.Notifications.Worker --startup-project src/PruebaCapitole.Notifications.Worker
```

Guarda los archivos que se generan en git.

## Datos de ejemplo

Al arrancar la API en Development, si la tabla de reservas está vacía, se insertan datos de ejemplo (con la librería Bogus). Se configura en `src/PruebaCapitole.Api/appsettings.Development.json`:

```json
"Seeding": {
  "Enabled": true,
  "BookingsCount": 50,
  "RandomSeed": 12345
}
```

- `Enabled`: pon `false` para no insertar nada.
- `BookingsCount`: cuántas reservas aleatorias crear (Pending, Confirmed y Cancelled).
- `RandomSeed`: la misma semilla genera siempre los mismos datos.

Además se crean 5 reservas fijas en el recurso `Sala-Demo` para probar las reglas: dos confirmadas adyacentes (10:00-11:00 y 11:00-12:00), dos Pending que se solapan con ellas (confirmarlas debe dar `booking.overlap`) y una cancelada con motivo.

En Docker puedes cambiar la cantidad con la variable `Seeding__BookingsCount`.

## Mensajería (RabbitMQ, Outbox y Notifications.Worker)

Flujo al confirmar: `POST /confirm` → `ConfirmBookingCommand` → `Booking.Confirm()` → evento de dominio `BookingConfirmed` → un handler lo traduce a `BookingConfirmedV1` y lo guarda en la tabla `OutboxMessages` **en el mismo commit** → un proceso en segundo plano de la API (`OutboxPublisher`) publica en RabbitMQ y marca el mensaje como procesado → el Worker lo consume y registra la notificación.

- **Outbox:** si RabbitMQ está caído, confirmar sigue devolviendo 200; los mensajes quedan pendientes y se reintentan cada pocos segundos (`Outbox:PollingIntervalSeconds`). La entrega es "al menos una vez".
- **Notifications.Worker** es un proceso independiente que consume `BookingConfirmedV1` y `BookingCancelledV1`, registra una notificación simulada (log + tabla `notifications.notifications`) y expone `/health`.
- **Idempotencia:** `EventId` es único en la tabla; si el mismo mensaje llega dos veces solo se guarda una notificación.
- **Reintentos:** 3 intentos con espera creciente (1 s, 2 s, ...), configurable en la sección `Retry`. Si sigue fallando, el mensaje va a la cola de errores (DLQ).
- **CorrelationId:** la cabecera `X-Correlation-Id` viaja de la API al mensaje y a los logs del Worker.

| Elemento | Nombre |
|---|---|
| Exchange principal (topic) | `bookings.events` |
| Exchange de errores (direct) | `bookings.events.dlx` |
| Cola de confirmadas | `notifications.booking-confirmed.v1` (binding `booking.confirmed.v1`) |
| Cola de canceladas | `notifications.booking-cancelled.v1` (binding `booking.cancelled.v1`) |
| Colas de errores | `<cola>.dlq` (binding por su propio nombre en el DLX) |

## Estructura del proyecto

```
src/
  PruebaCapitole.Domain                Reglas de negocio (agregado, value objects, specifications)
  PruebaCapitole.Service               Casos de uso (commands, queries, handlers, behaviors, puertos)
  PruebaCapitole.Infraestructure       EF Core, repositorios, Outbox y publicación en RabbitMQ
  PruebaCapitole.Shared                Eventos de integración (contratos públicos)
  PruebaCapitole.Api                   Endpoints REST y configuración
  PruebaCapitole.Notifications.Worker  Consumidor de RabbitMQ (solo depende de Shared)
web/                                   Frontal Angular
tests/
  PruebaCapitole.Domain.Tests          Tests de dominio
  PruebaCapitole.Service.Tests         Tests de casos de uso y behaviors
  PruebaCapitole.Api.IntegrationTests  Tests de integración (Docker)
docs/ADR                               Decisiones de arquitectura
```

Las dependencias apuntan siempre hacia el dominio: `Domain` no depende de nadie.

## Tests

```
dotnet test
```

Hay tres proyectos en `tests/`:

| Proyecto | Qué prueba | Necesita Docker |
|---|---|---|
| `PruebaCapitole.Domain.Tests` | Reglas del dominio: transiciones válidas e inválidas, `DateRange` y solape (incluida la adyacencia), `Customer` y la Specification | No |
| `PruebaCapitole.Service.Tests` | Casos de uso: `ConfirmBookingHandler` con y sin solape, `ValidationBehavior` y `ExceptionHandlingBehavior` | No |
| `PruebaCapitole.Api.IntegrationTests` | La API real con PostgreSQL y RabbitMQ (Testcontainers): confirmar publica `BookingConfirmedV1`, solape 409, transición inválida 422, validación 400 | **Sí** |

Para ejecutar solo los que no usan Docker:

```
dotnet test tests/PruebaCapitole.Domain.Tests
dotnet test tests/PruebaCapitole.Service.Tests
```

Los tests de integración necesitan Docker en marcha (la primera vez descargan las imágenes de PostgreSQL y RabbitMQ). Los del frontal están en `web/`:

```
cd web && npm test
```

## Problemas frecuentes

- **La API no conecta con la base de datos o RabbitMQ:** comprueba que Docker esté en marcha con `docker compose ps` y mira <http://localhost:8080/health>.
- **No existen las tablas:** la API las crea al arrancar solo en entorno Development. Si ejecutas la API a mano, no cambies `ASPNETCORE_ENVIRONMENT`.
- **Error de caché de NuGet al construir las imágenes:** ejecuta `docker builder prune -f` y vuelve a lanzar `docker compose up -d --build`.
- **`dotnet ef` no se encuentra:** instala la herramienta (ver Migraciones) y abre una terminal nueva.

## Conectar pgAdmin a la base de datos

En pgAdmin, clic derecho en **Servers**, **Register**, **Server**:

| Campo | Valor |
|---|---|
| Host | `db` |
| Port | `5432` |
| Maintenance database | `pruebacapitole` |
| Username | `postgres` |
| Password | `postgres` |

Si te conectas desde una herramienta instalada en tu equipo (no desde pgAdmin en Docker), usa `localhost` como host.

## Estado actual

Pendiente: estrategia de concurrencia entre confirmaciones simultáneas y los ADRs restantes.

## Declaración de uso de IA

Se usó Claude como ayuda para:

- Crear el `docker-compose` correctamente.
- Crear toda la estructura de CQRS con MediatR y los handlers vacíos.
- Crear el formato de los ADR.
- Crear la configuración del `DbContext` para el aggregate.
- Crear el compose de Docker con la configuración del Worker, usando el `DbContext` base.
- Crear el proceso en segundo plano que publica mensajes en RabbitMQ con `channel`.
- Crear los tests a partir de lo solicitado en el documento, para comprobar si se cumplen los criterios pedidos.
- Crear front end Angular desde una base que le di con los componentes y servicios, y añadir la lógica de validación y de llamadas a la API.
- Crear el README resumido.

## Qué haría con más tiempo

- Crear un aggregate para `Resource` y relacionarlo con las reservas. También lo haría con `Customer`, si puede cambiar.
- Implementar un skill de Claude que cree la estructura completa de un aggregate (incluida la configuración de `OnModelCreating` y los `ValueObjects`) a partir de un JSON.
- Validar una alternativa a AutoMapper: pasó a licencia comercial y la última versión MIT tiene una vulnerabilidad conocida.
