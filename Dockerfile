# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first, so the restore layer is reused until a dependency changes.
COPY nuget.config Directory.Build.props Directory.Packages.props ./
COPY Clinic.Data/Clinic.Data.csproj Clinic.Data/
COPY Clinic.Application/Clinic.Application.csproj Clinic.Application/
COPY Clinic.Mvc/Clinic.Mvc.csproj Clinic.Mvc/
RUN dotnet restore Clinic.Mvc/Clinic.Mvc.csproj

COPY Clinic.Data/ Clinic.Data/
COPY Clinic.Application/ Clinic.Application/
COPY Clinic.Mvc/ Clinic.Mvc/
RUN dotnet publish Clinic.Mvc/Clinic.Mvc.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# The official image ships a non-root "app" user; never run the site as root.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Clinic.Mvc.dll"]
