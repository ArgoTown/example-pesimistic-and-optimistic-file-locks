FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "PessimisticLockingPodTwoExample/PessimisticLockingPodTwoExample.csproj"

RUN dotnet publish -c Release -o /app PessimisticLockingPodTwoExample/PessimisticLockingPodTwoExample.csproj

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS publish
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "PessimisticLockingPodTwoExample.dll"]
