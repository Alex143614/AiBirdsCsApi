# Этап 1: Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости (для кэша)
COPY ["BirdApi.csproj", "."]
RUN dotnet restore "BirdApi.csproj"

# Копируем всё остальное и публикуем
COPY . .
RUN dotnet publish "BirdApi.csproj" -c Release -o /app/publish

# Этап 2: Запуск
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Твой проект слушает порт 5208, поэтому открываем его
EXPOSE 5208
ENTRYPOINT ["dotnet", "BirdApi.dll"]