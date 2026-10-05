FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/ECMA.Domain/ECMA.Domain.csproj", "src/ECMA.Domain/"]
COPY ["src/ECMA.Application/ECMA.Application.csproj", "src/ECMA.Application/"]
COPY ["src/ECMA.Web/ECMA.Web.csproj", "src/ECMA.Web/"]
RUN dotnet restore "src/ECMA.Web/ECMA.Web.csproj"
COPY . .
WORKDIR "/src/src/ECMA.Web"
RUN dotnet build "ECMA.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ECMA.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECMA.Web.dll"]
