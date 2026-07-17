# syntax=docker/dockerfile:1

# restore (single-project only)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY *.csproj ./
RUN dotnet restore

# build
COPY ./ ./
RUN dotnet publish -c Release -o app/ --no-restore

# final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS app
WORKDIR /app

RUN apt update && apt install -y curl
COPY --from=build /build/app/ ./

USER $APP_UID
ENTRYPOINT ["dotnet", "Surl.Api.dll"]
