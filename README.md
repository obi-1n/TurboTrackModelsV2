# Turbo Track Models

ASP.NET Core MVC application for managing a Scalextric model-car store. It includes customer accounts, staff roles, product and stock management, shopping carts, order processing, wishlists, PDF invoices, and Excel reports.

## Requirements

- .NET 10 SDK
- Stripe test or live account credentials for checkout

The application uses SQLite. Entity Framework Core migrations are applied automatically at startup, and the database seeder creates the initial roles and sample data.

## Run locally

From the project directory (`TurboTrackModels-Dotnet Core/`):

```bash
dotnet restore
dotnet run
```

Open the local URL printed by `dotnet run`. The default SQLite database is `turbo-track-models.db` in the application content directory.

## Configure Stripe

Do not store Stripe credentials in source control or committed configuration files. For local development, set them with .NET User Secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "Stripe:SecretKey" "your-stripe-secret-key"
dotnet user-secrets set "Stripe:PublishableKey" "your-stripe-publishable-key"
```

For a deployed server, configure the same settings through the hosting provider's secret store or environment variables:

```text
Stripe__SecretKey
Stripe__PublishableKey
```

Use Stripe test keys while testing and live keys only for a production deployment. If a key has been committed or otherwise exposed, rotate it in the Stripe dashboard.

## Database migrations

The application runs `Database.MigrateAsync()` on startup. To apply migrations manually from the project directory:

```bash
dotnet ef database update
```

To create a migration after changing the EF Core model:

```bash
dotnet ef migrations add DescriptiveMigrationName
```

## Build

```bash
dotnet build
```

## Static assets and exports

Site stylesheets are served from `/Content`, and car images are served from `/Scalextric Car Images`. Invoice PDFs are generated with Syncfusion PDF; report spreadsheets are generated with ClosedXML.
