# Use the official .NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files
COPY ["SR_Audio_Video.sln", "./"]
COPY ["SReddy_Audio_Video/SReddy_Audio_Video.csproj", "SReddy_Audio_Video/"]

# Restore dependencies
RUN dotnet restore "SReddy_Audio_Video/SReddy_Audio_Video.csproj"

# Copy all files and build the application
COPY . .
WORKDIR "/src/SReddy_Audio_Video"
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SReddy_Audio_Video.dll"]
