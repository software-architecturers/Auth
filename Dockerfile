FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app
EXPOSE 5000
EXPOSE 5001
# Copy csproj and restore
COPY ./*.sln ./
COPY src/Auth.Application/Auth.Application.csproj  ./src/Auth.Application/
COPY src/Auth.Domain/Auth.Domain.csproj  ./src/Auth.Domain/
COPY src/Auth.Persistence/Auth.Persistence.csproj ./src/Auth.Persistence/
COPY src/Auth.WebApp/Auth.WebApp.csproj ./src/Auth.WebApp/
RUN dotnet restore

COPY src/ ./src/
RUN dotnet publish ./src/Auth.WebApp/Auth.WebApp.csproj -c Release -o /app/out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime AS runtime
WORKDIR /app
COPY --from=build-env app/out ./

ENTRYPOINT ["dotnet", "Auth.WebApp.dll"]