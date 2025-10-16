# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["poker.net.csproj", "."]
RUN dotnet restore "./poker.net.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./poker.net.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./poker.net.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "poker.net.dll"]


#Example Development!
# docker build --no-cache -t jbelthoff/poker.calc:razor-dev-latest .
# docker scan jbelthoff/poker.calc:razor-dev-latest
# docker push jbelthoff/poker.calc:razor-dev-latest

#Example Production!
# docker build --no-cache --no-cache -t jbelthoff/poker.calc:razor-prod-latest .
# docker scan jbelthoff/poker.calc:razor-prod-latest
# docker push jbelthoff/poker.calc:razor-prod-latest