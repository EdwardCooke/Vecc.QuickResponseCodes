FROM microsoft/dotnet-framework:4.7
WORKDIR /app
COPY bin/Release/net47/win7-x86 .

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://+:5000

ENTRYPOINT ["Vecc.QuickResponseCodes.Api.exe"]