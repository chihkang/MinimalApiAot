# Deployment & Test Instructions

## Dockerfiles
- **Linux server (amd64):** Use the default Dockerfile (linux-x64 AOT, runtime-deps amd64).
- **ARM64:** Use Dockerfile.arm64.
- **Legacy amd64 variant:** Dockerfile.noble-chiseled-amd64 (kept for reference).

## Build & Run (Linux amd64)
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

## Build & Run (ARM64)
```bash
docker build -t minimalapiaot:arm64 -f Dockerfile.arm64 .

docker run -d --name minimalapiaot \
  -p 8080:80 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e MongoSettings__ConnectionString="mongodb://<mongo-host>:27017" \
  -e MongoSettings__DatabaseName="portfolio_db" \
  -e MongoSettings__UseSsl=false \
  -e MongoSettings__AllowInsecureSsl=true \
  minimalapiaot:arm64
```

## Smoke Test Script
Script: `scripts/smoke-test.sh`

### Requirements
- curl
- python3
- docker (only if `SEED_DB=true`)

### Run
```bash
chmod +x scripts/smoke-test.sh

BASE_URL="http://localhost:8080" \
MONGO_URI="mongodb://localhost:27017" \
MONGO_DB="portfolio_db" \
SEED_DB=true \
./scripts/smoke-test.sh
```

## Notes
- The container listens on port **80** internally. Map it to any external port with `-p`.
- For production, TLS must be enabled (UseSsl=true) and insecure TLS disabled.
- The smoke test seeds a stock and daily values using `mongosh` inside a temporary `mongo:7` container.
