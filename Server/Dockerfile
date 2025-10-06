FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5176
EXPOSE 7032

# Create a non-root user (better security)
RUN adduser --disabled-password --home /app --uid 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Add size check for base image
RUN du -sh /app 2>/dev/null || echo "Base image size check"

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server.csproj", "./"]
RUN dotnet restore "Server.csproj"
RUN du -sh /src  # Show size after restore
COPY . .
RUN dotnet build "Server.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN du -sh /app/build  # Show size after build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN du -sh /app/publish  # Show size after publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for certificates (as root)
USER root
RUN mkdir -p /https && chown -R appuser:appuser /https

# Switch back to appuser
USER appuser

# Environment variables with default values (can be overridden by docker-compose)
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=.NETCore2024
ENV ASPNETCORE_URLS=https://+:7032;http://+:5176
ENV DB_HOST=localhost
ENV DB_PORT=5432
ENV DB_NAME=wss_prod
ENV DB_USER=wss
ENV DB_PASS=WSS

RUN du -sh /app  # Show final image size
ENTRYPOINT ["dotnet", "Server.dll"]
