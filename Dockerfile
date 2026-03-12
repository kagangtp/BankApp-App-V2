# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Proje dosyalarını (.csproj) kopyala (Restore aşamasını hızlandırır)
COPY ["IlkProjem.API/IlkProjem.API.csproj", "IlkProjem.API/"]
COPY ["IlkProjem.BLL/IlkProjem.BLL.csproj", "IlkProjem.BLL/"]
COPY ["IlkProjem.Core/IlkProjem.Core.csproj", "IlkProjem.Core/"]
COPY ["IlkProjem.DAL/IlkProjem.DAL.csproj", "IlkProjem.DAL/"]

# 2. Bağımlılıkları yükle
RUN dotnet restore "IlkProjem.API/IlkProjem.API.csproj"

# 3. Kalan tüm dosyaları kopyala
COPY . .

# 4. Uygulamayı derle ve yayınla
RUN dotnet publish "IlkProjem.API/IlkProjem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Derlenen dosyaları build aşamasından al
COPY --from=build /app/publish .

# Railway için port ayarı
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IlkProjem.API.dll"]