The Platform Service allows you to manage a platforms, and the command service is how to execute the command of platform.

This Project has No circuit breaker, no Polly , no retry.. no persistent volume for the rabbitMQ in case it was down, it could be improved to be written in layers, it could be improved by using generic repository, but the main purpose of it was to get two services communicating asynch and in Kubernetes environment.

They are written in .NET 8 with MSSQL, Docker, Kubernetes, RabbitMQ, gRPC,Ingress Nginx.
This project has 2 sync calls from platform service to command service (1 using httpclient and 1 using gRPC)
and 1 async call, used RabbitMQ as the bus.

Did a docker file to build the project and a deployment file to deploy to kubernetes, along the deployment there's the clusterip services for the services, for the PlatformService, i have added nodeport, just to test the service using postman so to be able to access the cluster.

-Docker:
Two docker files under each service were written in order to allow us to build the projects

-Kubernetes:
A Ingress Nginx that servers as the Api Gateway
A deployment for mssql with a persistent claim
A deployment for command service
A deployment for platform service
A deployment for rabbit mq 
A deployment for local pvc to claim volume for the mssql
A deployment for the platform nodeport, which allows us to test the platform service using postman

-Command Service uses in memory database for simplicity and on starting of the project a gRPC call was triggered to platform service to grab all data and seed them, for the rabbitMQ, an event processing was written and after that it injects the received data into the tables, used Mappers to map the DTOs

-Platform Service uses the MSSQL, it has gRPC, where it acts as a server for gRPC, so the command service requests the data from it. It has a httpclient sync call, so upon creation of platform, it sends a sync call to command service to notify about the creation, it also has an async call, used rabbitMQ as the message bug, when creating a platform it sends the object to command service. Used mappers to map the DTOs
