FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CourseEvaluation.Api/CourseEvaluation.Api.csproj", "CourseEvaluation.Api/"]
COPY ["CourseEvaluation.Core/CourseEvaluation.Core.csproj", "CourseEvaluation.Core/"]
COPY ["CourseEvaluation.Infrastructure/CourseEvaluation.Infrastructure.csproj", "CourseEvaluation.Infrastructure/"]
COPY ["CourseEvaluation.Tests/CourseEvaluation.Tests.csproj", "CourseEvaluation.Tests/"]

RUN dotnet restore "CourseEvaluation.Api/CourseEvaluation.Api.csproj"

COPY . .
WORKDIR "/src/CourseEvaluation.Api"
RUN dotnet build "CourseEvaluation.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CourseEvaluation.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CourseEvaluation.Api.dll"]
