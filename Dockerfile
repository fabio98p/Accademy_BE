FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CodeAcademy.sln ./
COPY AcademyModel/AcademyModel.csproj AcademyModel/
COPY AcademyPersistance/AcademyEfPersistance.csproj AcademyPersistance/
COPY CodeAcademyWeb/CodeAcademyWeb.csproj CodeAcademyWeb/
RUN dotnet restore CodeAcademyWeb/CodeAcademyWeb.csproj

COPY . .
RUN dotnet publish CodeAcademyWeb/CodeAcademyWeb.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CodeAcademyWeb.dll"]
