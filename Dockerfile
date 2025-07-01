FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies
COPY Dualite/*.csproj Dualite/
COPY Dualite.Models/*.csproj Dualite.Models/
COPY Dualite.Business/*.csproj Dualite.Business/
COPY Dualite.Data/*.csproj Dualite.Data/
RUN dotnet restore Dualite/Dualite.csproj

# Copy the rest of the code and build
COPY . ./
RUN dotnet publish Dualite/Dualite.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Dualite.dll"]
