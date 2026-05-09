FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/OpenPeer.Api/OpenPeer.Api.csproj", "OpenPeer.Api/"]
COPY ["src/OpenPeer.Application/OpenPeer.Application.csproj", "OpenPeer.Application/"]
COPY ["src/OpenPeer.Domain/OpenPeer.Domain.csproj", "OpenPeer.Domain/"]
COPY ["src/OpenPeer.Infrastructure/OpenPeer.Infrastructure.csproj", "OpenPeer.Infrastructure/"]
RUN dotnet restore "OpenPeer.Api/OpenPeer.Api.csproj"
COPY src/ .
RUN dotnet build "OpenPeer.Api/OpenPeer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenPeer.Api/OpenPeer.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/Uploads /app/logs
EXPOSE 8080
ENTRYPOINT ["dotnet", "OpenPeer.Api.dll"]
