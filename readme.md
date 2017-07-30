# **QR Image Generator**

This is the abstractions, implementation and API for QR image generating. It uses the full .net framework due to needing the System.Drawing namespace.

# Deployment

To run this in a docker container on a dev machine, use the ```.\src\runindocker.ps1 ``` file.

To deploy: 


1. Build a deployment container
    ```
    cd src
    cd Vecc.QuickResponseCodes.Api
    dotnet build -c Release
    docker build -t vecc.quickresponsecodes.api .
    ```
1. Export the image. This takes a while and about 12 gigs of space on the c drive
    ```
    docker save -o d:\vecc.quickresponsecodes.api.tar vecc.quickresponsecodes.api
    ```
1. Copy the ```d:\vecc.quickresponsecodes.api.tar``` to ```\\hyperv1\d$\vecc.quickresponsecodes.api.tar```
1. Import the image on the server.
    ```
    docker load -i d:\vecc.quickresponsecodes.api.tar
    ```
1. If it was running, or a container with the name quickresponsecodes exists, remove it
    ```
    docker container kill quickresponsecodes
    docker container rm quickresponsecodes
    ```
1. Start the container and set it to auto start ```docker run --name quickresponsecodes -d -p 8101:5000 --restart unless-stopped vecc.quickresponsecodes.api```