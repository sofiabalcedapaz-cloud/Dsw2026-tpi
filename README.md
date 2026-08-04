## Dsw2026Tpi

## Integrantes 
* Balceda Paz, Sofia - 57927
* Barrientos, Victoria - 57911
* Lopez, Mauro - 58066
* Lorca, Nicole - 60705 

## Configuración y ejecución del proyecto

## Requisitos
- .NET 10 SDK
- SQL Server LocalDB
- Visual Studio 2022/2026 o Visual Studio Code

## Pasos para ejecutar el proyecto
1. Clonar el repositorio 
	```bash
	git clone <https://github.com/sofiabalcedapaz-cloud/Dsw2026-tpi/tree/development>
	```
2. Ingresar a la carpeta del proyecto
	```bash
	cd Dsw2026-tpi
	```
3. Restaurar las dependencias
	```bash
	dotnet restore
	```
4. Aplicar las migraciones de la base de datos
	```bash
	dotnet ef database update --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api
	```
5. Ejecutar la Api
	```bash
	dotnet run --project Dsw2026Tpi.Api
	```
6. Acceder al Swagger 
	```bash
	https://localhost:7075/swagger/index.html
	```

## Endpoints implementados

## Autenticación
-`POST /api/auth/admin/register:` Registra un administrador

-`POST /api/auth/admin/login:` Inicia sesión como administrador

-`POST /api/auth/patient/login:` Inicia sesión como paciente

## Especialidades
- `POST /api/specialities:` Crea una especialidad.

- `GET /api/specialities:` Lista las especialidades.

### Médicos

- `POST /api/doctors:` Registra un médico.

- `GET /api/doctors:` Obtiene médicos con paginación.

### Disponibilidad

- `POST /api/availabilities:` Crea una regla de disponibilidad.

- `PUT /api/availabilities:` Actualiza una regla de disponibilidad.

### Turnos

- `POST /api/appointments:` Reserva un turno.

- `DELETE /api/appointments/{id}:` Cancela un turno.

- `GET /api/appointments/patient:` Obtiene los turnos de un paciente.

- `GET /api/appointments:` Lista turnos por fecha.

- `GET /api/appointments/search:` Busca turnos utilizando filtros.

## Tecnologías utilizadas

- ASP.NET Core 
- Entity Framework Core 
- SQL Server 
- ASP.NET Identity
- JWT
- Swagger
- Serlog
- xUnit