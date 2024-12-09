# 建構階段
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /source

# 安裝必要的編譯工具
RUN apk add --no-cache \
    clang \
    build-base \
    lttng-ust \
    zlib-dev \
    linux-headers

# 複製專案檔
COPY *.csproj .
RUN dotnet restore -r linux-musl-x64

# 複製其餘原始碼
COPY . .
RUN dotnet publish -c Release -r linux-musl-x64 --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishAot=true \
    -o /app

# 運行階段 - 使用最小的 alpine 基礎映像
FROM alpine:3.19
WORKDIR /app

# 安裝必要的運行時依賴
RUN apk add --no-cache \
    libstdc++ \
    icu-libs \
    lttng-ust

# 從建構階段複製編譯好的應用程式
COPY --from=build /app .

# 設定時區（選擇性）
ENV TZ=Asia/Taipei
RUN apk add --no-cache tzdata && \
    cp /usr/share/zoneinfo/$TZ /etc/localtime && \
    echo $TZ > /etc/timezone && \
    apk del tzdata

# 設定環境變數（根據需求調整）
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["./MinimalApiAot"]