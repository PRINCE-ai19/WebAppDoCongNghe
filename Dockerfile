# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy đúng file .slnx (có chữ x) và .csproj
COPY ["WebAppDoCongNghe.sln", "./"]
COPY ["WebAppDoCongNghe/WebAppDoCongNghe.csproj", "WebAppDoCongNghe/"]

# Restore thư viện
RUN dotnet restore "WebAppDoCongNghe/WebAppDoCongNghe.csproj"

# Copy toàn bộ code còn lại
COPY . .

# Chuyển vào thư mục chứa code để build
WORKDIR "/src/WebAppDoCongNghe"
RUN dotnet publish "WebAppDoCongNghe.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình Port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebAppDoCongNghe.dll"]