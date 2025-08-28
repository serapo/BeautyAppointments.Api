
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY BeautyAppointments.Api.csproj .
RUN dotnet restore "BeautyAppointments.Api.csproj"


COPY . .
RUN dotnet publish "BeautyAppointments.Api.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .


ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}


ENTRYPOINT ["dotnet", "BeautyAppointments.Api.dll"]
