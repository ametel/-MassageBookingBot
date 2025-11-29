FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MassageBookingBot.BotWorker/MassageBookingBot.BotWorker.csproj", "src/MassageBookingBot.BotWorker/"]
COPY ["src/MassageBookingBot.Infrastructure/MassageBookingBot.Infrastructure.csproj", "src/MassageBookingBot.Infrastructure/"]
COPY ["src/MassageBookingBot.Application/MassageBookingBot.Application.csproj", "src/MassageBookingBot.Application/"]
COPY ["src/MassageBookingBot.Domain/MassageBookingBot.Domain.csproj", "src/MassageBookingBot.Domain/"]
RUN dotnet restore "src/MassageBookingBot.BotWorker/MassageBookingBot.BotWorker.csproj"
COPY . .
WORKDIR "/src/src/MassageBookingBot.BotWorker"
RUN dotnet build "MassageBookingBot.BotWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MassageBookingBot.BotWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MassageBookingBot.BotWorker.dll"]
