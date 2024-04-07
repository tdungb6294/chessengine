# Use the official ASP.NET Core SDK as a build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the Blazor application source code
COPY . .

# Build the Blazor application
RUN dotnet publish -c Release -o out

# Use a lighter runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the built Blazor application to the runtime image
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Chess.dll"]