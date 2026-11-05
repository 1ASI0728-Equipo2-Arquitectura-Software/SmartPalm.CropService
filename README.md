# SmartPalm Crop Service

Microservicio independiente para plantaciones, sectores de monitoreo y afiliaciones de agrónomos. Conserva los contratos REST del monolito y es dueño exclusivo del esquema PostgreSQL `crop`.

## Rol en la plataforma

Crop define el contexto de propiedad física utilizado por la telemetría: un PalmGrower es dueño de una plantación, una plantación contiene sectores y cada dispositivo IoT registrado se asigna a un sector. Valida la plantación y el sector durante el registro del dispositivo y publica proyecciones consumidas por Device, Ingestion, Alert y Agronomy.

El sector puede crearse antes de conocer un dispositivo, por lo que su MAC es opcional durante la configuración inicial. El administrador completa la asignación final del dispositivo a través de Device y esta se refleja aquí mediante el evento de integración. Crop sigue siendo dueño de los datos de plantaciones y sectores; no persiste lecturas ni credenciales de dispositivos.

## Relaciones de integración

- **Identity Service:** proporciona el usuario propietario y los eventos de roles/suscripciones.
- **Device Service:** solicita la validación del sector y publica la asociación del registro del dispositivo.
- **Ingestion Service:** recibe la proyección resultante de sector/dispositivo para validar lecturas y calcular la salud del sector.
- **Alert y Agronomy:** consumen proyecciones de sectores y afiliaciones para resolver destinatarios y limitar informes/recomendaciones.
- **Clientes web/móvil:** administran plantaciones y sectores a través del API Gateway.

## Responsabilidades

- Crear, consultar y actualizar plantaciones.
- Calcular el plan de instalación (un sector de sensores por cada 10 hectáreas).
- Asociar un dispositivo IoT a un único sector.
- Gestionar afiliaciones activas de agrónomos.
- Publicar eventos mediante un outbox transaccional.

## Ejecución local

1. Copiar `.env.example` a `.env` y usar el mismo `JWT_KEY` de Identity Service.
2. Mantener `RABBITMQ_ENABLED=false` para trabajar solo con PostgreSQL, o usar el perfil `messaging` para iniciar un broker aislado.
3. Ejecutar `docker compose --env-file .env -f compose.local.yaml up --build`.

Para incluir RabbitMQ: `docker compose --env-file .env -f compose.local.yaml --profile messaging up --build`.

La API queda en `http://localhost:5102`; Swagger está disponible en `/swagger` y readiness en `/health/ready`.

## Desarrollo

```powershell
dotnet restore SmartPalm.CropService.slnx
dotnet build SmartPalm.CropService.slnx --no-restore
dotnet test SmartPalm.CropService.slnx --no-build
dotnet tool restore
dotnet tool run dotnet-ef migrations add MigrationName --project SmartPalm.CropService.API --startup-project SmartPalm.CropService.API --output-dir Infrastructure/Persistence/EFC/Migrations
```
