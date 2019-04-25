FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app
EXPOSE 5000
EXPOSE 5001
# Copy csproj and restore
COPY ./*.sln ./
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore

COPY ./src ./src
RUN dotnet publish ./src/Auth.WebApp/Auth.WebApp.csproj -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build-env app/out ./

ENTRYPOINT ["dotnet", "Auth.WebApp.dll"]