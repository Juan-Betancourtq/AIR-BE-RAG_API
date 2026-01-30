FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["backend/ResumeChat.RagApi/ResumeChat.RagApi.csproj", "backend/ResumeChat.RagApi/"]
RUN dotnet restore "backend/ResumeChat.RagApi/ResumeChat.RagApi.csproj"
COPY . .
WORKDIR "/src/backend/ResumeChat.RagApi"
RUN dotnet build "ResumeChat.RagApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ResumeChat.RagApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ResumeChat.RagApi.dll"]
