# MinimalApiAot

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![MongoDB](https://img.shields.io/badge/MongoDB-Driver-47A248)
![AOT](https://img.shields.io/badge/Native%20AOT-enabled-1f6feb)

## What the project does
MinimalApiAot is a .NET 10 Minimal API service for portfolio management with MongoDB. It supports users, portfolios, stocks, position events, and portfolio daily value summaries. The app is built with Native AOT for fast startup and small runtime images.

## Why the project is useful
- **Native AOT** build for fast startup and lower memory usage.
- **MongoDB-first** data access using MongoDB.Driver.
- **Minimal API** with built-in OpenAPI endpoint.
- **Health checks** for MongoDB connectivity.
- Ready-to-run **Docker** images for Linux amd64 and ARM64.

## How users can get started

### Prerequisites
- Docker
- MongoDB (local or hosted)

### Quick start (Linux amd64)
```bash
docker build -t minimalapiaot:amd64 .

docker run -d --name minimalapiaot \
  -p 8080:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e MongoSettings__ConnectionString="mongodb://<mongo-host>:27017" \
  -e MongoSettings__DatabaseName="portfolio_db" \
  -e MongoSettings__UseSsl=true \
  -e MongoSettings__AllowInsecureSsl=false \
  minimalapiaot:amd64
```

### ARM64 build
```bash
docker build -t minimalapiaot:arm64 -f Dockerfile.arm64 .
```

### Verify
- OpenAPI: http://localhost:8080/openapi/v1.json
- Health: http://localhost:8080/health

### Smoke test
```bash
chmod +x scripts/smoke-test.sh

BASE_URL="http://localhost:8080" \
MONGO_URI="mongodb://localhost:27017" \
MONGO_DB="portfolio_db" \
SEED_DB=true \
./scripts/smoke-test.sh
```

## Where users can get help
- Deployment and test steps: [.github/instruction.md](.github/instruction.md)
- Zeabur deployment notes: [ZEABUR_DEPLOYMENT.md](ZEABUR_DEPLOYMENT.md)
- HTTP samples: [MinimalApiAot.http](MinimalApiAot.http)

## Who maintains and contributes
Maintained by project contributors. Contributions are welcome via pull requests and issues.

## Configuration
Mongo settings are configured via environment variables with the `MongoSettings__` prefix. See [Configurations/MongoSettings.cs](Configurations/MongoSettings.cs) for available options.

## License
See the LICENSE file (if present in the repository).
