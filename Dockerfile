# ============
# Build stage
# ============
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia dos projetos csproj
COPY SalesOrder.sln ./
COPY src/SalesOrder.Api/SalesOrder.Api.csproj src/SalesOrder.Api/
COPY src/SalesOrder.Application/SalesOrder.Application.csproj src/SalesOrder.Application/
COPY src/SalesOrder.Domain/SalesOrder.Domain.csproj src/SalesOrder.Domain/
COPY src/SalesOrder.Infrastructure/SalesOrder.Infrastructure.csproj src/SalesOrder.Infrastructure/

RUN dotnet restore SalesOrder.sln


COPY . .
RUN dotnet publish src/SalesOrder.Api/SalesOrder.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ============
# Runtime stage
# ============
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Usuário não-root
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

# Porta padrão
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SalesOrder.Api.dll"]
