FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /source

# 安裝必要的編譯工具
RUN apk add clang build-base zlib-dev

# Copy everything
COPY . .
# Restore dependencies
RUN dotnet restore
# Publish with Native AOT
RUN dotnet publish -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./MinimalApiAot"]
