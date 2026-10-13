# Card Service

Card Service is the third independent service in the local `credit-platform`
demonstration. It owns card records and consumes approved-credit events from the
shared RabbitMQ broker. It does not query the Credit database and does not store
real card numbers, CVVs, or payment credentials.

## Local prerequisites

- .NET SDK 10.
- The shared SQL Server container used by Customer and Credit, exposed on port
  `1433`.
- The shared RabbitMQ container, exposed on ports `5672` and `15672`.
- Credit Service running with its `credit.events` exchange declared.

## Local secrets

Set the SQL Server connection string and RabbitMQ credentials using User Secrets
from this repository root. Use the same SQL Server administrator credentials and
RabbitMQ credentials already configured for the other local services; do not put
them in appsettings or commit them.

```powershell
dotnet user-secrets set "ConnectionStrings:CardService" "Server=localhost,1433;Database=CardServiceDb;User Id=sa;Password=<sql-server-password>;Encrypt=True;TrustServerCertificate=True" --project src\CardService.API\CardService.API.csproj
dotnet user-secrets set "RabbitMq:UserName" "<rabbitmq-user>" --project src\CardService.API\CardService.API.csproj
dotnet user-secrets set "RabbitMq:Password" "<rabbitmq-password>" --project src\CardService.API\CardService.API.csproj
```

## Create the Card database

Install `dotnet-ef` if needed, then apply the migration:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef database update --project src\CardService.Infrastructure\CardService.Infrastructure.csproj --startup-project src\CardService.API\CardService.API.csproj
```

Card Service creates its own `CardServiceDb`; it does not share tables or foreign
keys with Customer or Credit.

## Run the service

```powershell
dotnet run --project src\CardService.API\CardService.API.csproj
```

The API listens on `http://localhost:5234`. The interactive API reference is
available at `http://localhost:5234/swagger`, and the raw OpenAPI document is at
`http://localhost:5234/openapi/v1.json`.

## RabbitMQ flow

On startup, Card Service declares a durable queue named `card.credit-approved`
and binds it to the shared topic exchange `credit.events` using the exact routing
key `credit.approved`. The previously created inspection queue
`credit-platform.credit-decisions` is separate and is not modified.

Each valid approval event is stored with its card in one database save. The
`eventId` inbox key and unique `creditApplicationId` index prevent redelivery from
creating another card. The consumer acknowledges only after persistence. Invalid
or failed deliveries are rejected into the durable
`card.credit-approved.dead-letter` queue for inspection.

RabbitMQ queues do not receive messages retroactively. Start Card Service before
sending a new approval from Credit; an approval already sitting in the manual
inspection queue will not automatically be copied into the Card queue.

For the local demonstration, a card has type `StandardCredit`, its limit equals
the approved amount, and its initial status is `Active`. It contains only internal
identifiers and simulated attributes.

## HTTP endpoints

- `GET /api/customers/{customerId}/cards` — list a customer's cards.
- `GET /api/cards/{id}` — get a card by internal id.
- `PUT /api/cards/{id}/status` — set the status to `Active`, `Blocked`, or
  `Cancelled`. A cancelled card cannot be reactivated.

Example request body:

```json
{
  "status": "Blocked"
}
```

Use `src\CardService.API\CardService.API.http` for sample requests.

## Verify

```powershell
dotnet build CardService.slnx
dotnet test CardService.slnx
```
