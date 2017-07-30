cd Vecc.QuickResponseCodes.Api
dotnet build -c Release
docker build -t vecc.quickresponsecodes.api .
docker run -i -t -p 5000:5000 --env ASPNETCORE_URLS=http://+:5000 --name vecc vecc.quickresponsecodes.api
#docker kill vecc
docker rm vecc
docker image rm vecc.quickresponsecodes.api
cd ..
