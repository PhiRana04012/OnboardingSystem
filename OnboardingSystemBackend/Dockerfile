# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["OnboardingSystem.csproj", "./"]
RUN dotnet restore "OnboardingSystem.csproj"
COPY . .
RUN dotnet build "OnboardingSystem.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "OnboardingSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "OnboardingSystem.dll"]
