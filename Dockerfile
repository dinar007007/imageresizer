FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /src
COPY src/ImageResizer/*.csproj .
COPY src/ImageResizer.Core/*.csproj .
COPY src/ImageResizer.sln .
RUN dotnet restore ImageResizer.csproj
COPY src .
RUN dotnet publish ImageResizer.sln -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
ENV AllowedDomain hsto.org
EXPOSE 80
ENTRYPOINT ["dotnet", "ImageResizer.dll"]