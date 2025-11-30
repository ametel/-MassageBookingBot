FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MassageBookingBot.Api/MassageBookingBot.Api.csproj", "src/MassageBookingBot.Api/"]
COPY ["src/MassageBookingBot.Infrastructure/MassageBookingBot.Infrastructure.csproj", "src/MassageBookingBot.Infrastructure/"]
COPY ["src/MassageBookingBot.Application/MassageBookingBot.Application.csproj", "src/MassageBookingBot.Application/"]
COPY ["src/MassageBookingBot.Domain/MassageBookingBot.Domain.csproj", "src/MassageBookingBot.Domain/"]
RUN dotnet restore "src/MassageBookingBot.Api/MassageBookingBot.Api.csproj"
COPY . .
WORKDIR "/src/src/MassageBookingBot.Api"
RUN dotnet build "MassageBookingBot.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MassageBookingBot.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MassageBookingBot.Api.dll"]
