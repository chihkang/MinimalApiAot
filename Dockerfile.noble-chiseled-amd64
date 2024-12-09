# Build 階段
FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
WORKDIR /src

# 複製專案檔並還原相依套件
COPY ["MinimalApiAot.csproj", "."]
RUN dotnet restore "MinimalApiAot.csproj"

# 複製所有檔案並建置應用程式
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=true

# 運行階段
FROM mcr.microsoft.com/dotnet/aspnet:9.0.0-noble-chiseled-amd64
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

USER $APP_UID
ENTRYPOINT ["dotnet", "MinimalApiAot.dll"]