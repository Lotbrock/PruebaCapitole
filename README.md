# PruebaCapitole

Micro-MVP de gestión de reservas de recursos (salas, puestos, vehículos...).
Backend en .NET con arquitectura hexagonal, DDD, CQRS y PostgreSQL.

## Requisitos

Antes de empezar, instala:

| Herramienta | Para qué |
|---|---|
| [Git](https://git-scm.com/) | Clonar el proyecto |
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Compilar y ejecutar la API |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Base de datos PostgreSQL y pgAdmin |
| `dotnet-ef` | Crear las tablas de la base de datos |

Instala `dotnet-ef` una sola vez:

```
dotnet tool install --global dotnet-ef
```

## Cómo iniciar el proyecto

Ejecuta los comandos desde la carpeta raíz del repositorio.

**1. Crea tu archivo de configuración**

```
cp .env.example .env
```

El `.env` guarda usuarios y contraseñas de desarrollo. No se sube a git.

**2. Levanta la base de datos**

```
docker compose up -d db pgadmin
```

**3. Crea la migración (solo la primera vez)**

Si todavía no existe la carpeta `Migrations` dentro de `src/PruebaCapitole.Infraestructure`, créala:

```
dotnet ef migrations add InitialCreate --project src/PruebaCapitole.Infraestructure --startup-project src/PruebaCapitole.Api
```

Guarda los archivos que genera en git.

**4. Arranca la API**

```
dotnet run --project src/PruebaCapitole.Api --launch-profile http
```

Al arrancar, la API crea las tablas sola (solo en entorno Development).

**5. Abre la documentación**

- API (Scalar): <http://localhost:5229/scalar>
- Documento OpenAPI: <http://localhost:5229/openapi/v1.json>
- pgAdmin: <http://localhost:5050> (usuario `admin@admin.com`, contraseña `admin`)

## Alternativa: todo en Docker

Si prefieres no instalar .NET para ejecutarlo, levanta todo con Docker (la migración del paso 3 debe existir ya):

```
docker compose up -d --build
```

La API queda en <http://localhost:8080/scalar>.

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

## Estructura del proyecto

```
src/
  PruebaCapitole.Domain          Reglas de negocio (agregado, value objects, specifications)
  PruebaCapitole.Service         Casos de uso (commands, queries, handlers, puertos)
  PruebaCapitole.Infraestructure EF Core, repositorios, acceso a datos
  PruebaCapitole.Shared          Eventos de integración (contratos públicos)
  PruebaCapitole.Api             Endpoints REST y configuración
tests/
  PruebaCapitole.Api.Tests       Pruebas
docs/ADR                         Decisiones de arquitectura
```

Las dependencias apuntan siempre hacia el dominio: `Domain` no depende de nadie.

## Tests

```
dotnet test
```

## Problemas frecuentes

- **`dotnet ef` no se encuentra:** instala la herramienta (ver Requisitos) y abre una terminal nueva.
- **La API no conecta con la base de datos:** comprueba que Docker esté en marcha y ejecuta `docker compose ps`.
- **No existen las tablas:** falta crear la migración (paso 3).

## Estado actual

Proyecto en construcción. Pendiente: implementar los handlers, la Outbox y los eventos, el manejo de errores con `ProblemDetails`, el frontal y los tests. El consumidor de notificaciones (Worker) queda fuera por ahora.
