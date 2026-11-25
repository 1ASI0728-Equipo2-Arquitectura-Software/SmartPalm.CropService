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

## Despliegue en Vultr/Coolify

CropService se publica en Vultr 1 como una aplicación .NET independiente. PostgreSQL y RabbitMQ deben permanecer en Vultr 2, únicamente accesibles por la red privada.

En Coolify:

1. Crear una aplicación desde el repositorio público `SmartPalm.CropService`.
2. Seleccionar la rama `prepare-deployment`.
3. Usar **Dockerfile** como método de compilación.
4. Configurar el Dockerfile como `Dockerfile`, el contexto de compilación como `/` y el puerto interno `8080`.
5. Configurar `/health/ready` como health check.
6. Registrar las variables siguientes como secretos o variables de ejecución; no versionar valores reales:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__CropDatabase=Host=10.6.96.4;Port=5432;Database=smartpalm_crop;Username=<usuario>;Password=<secreto>
Jwt__Key=<misma clave compartida de al menos 32 bytes>
Jwt__Issuer=smartpalm.identity
Jwt__Audience=smartpalm.clients
RabbitMq__Enabled=true
RabbitMq__Uri=amqp://<usuario>:<password>@10.6.96.4:5672/
RabbitMq__Exchange=smartpalm.events
```

Una alternativa declarativa para pruebas con Coolify Compose está disponible en `compose.coolify.yaml`. Esa composición no crea PostgreSQL ni RabbitMQ: consume los recursos privados de Vultr 2.

La configuración de la composición usa estas variables:

```text
CROP_DB_HOST=10.6.96.4
CROP_DB_PORT_REMOTE=5432
CROP_DB_NAME=smartpalm_crop
CROP_DB_USERNAME=<usuario>
CROP_DB_PASSWORD=<secreto>
CROP_API_PORT=8080
JWT_KEY=<misma clave compartida>
JWT_ISSUER=smartpalm.identity
JWT_AUDIENCE=smartpalm.clients
RABBITMQ_ENABLED=true
RABBITMQ_URI=amqp://<usuario>:<password>@10.6.96.4:5672/
RABBITMQ_EXCHANGE=smartpalm.events
```

No publicar `5432`, `5672` ni `15672` desde Vultr 1. Mantener `15672` cerrado al acceso público. No se deben incorporar credenciales, JWT ni tokens al repositorio.

## Desarrollo

```powershell
dotnet restore SmartPalm.CropService.slnx
dotnet build SmartPalm.CropService.slnx --no-restore
dotnet test SmartPalm.CropService.slnx --no-build
dotnet tool restore
dotnet tool run dotnet-ef migrations add MigrationName --project SmartPalm.CropService.API --startup-project SmartPalm.CropService.API --output-dir Infrastructure/Persistence/EFC/Migrations
```
