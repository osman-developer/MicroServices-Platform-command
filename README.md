# Platform Service and Command Service Architecture

This project demonstrates a **microservices architecture** consisting of **Platform Service** and **Command Service**, deployed in a **Kubernetes** environment. The services communicate using synchronous (HTTPClient, gRPC) and asynchronous (RabbitMQ) messaging patterns.

## Tech Stack

- **.NET 8**
- **MSSQL**
- **Docker**
- **Kubernetes**
- **RabbitMQ**
- **gRPC**
- **Ingress Nginx**

## Architecture Overview

- **Platform Service**: Manages platform-related data and provides both synchronous (HTTPClient, gRPC) and asynchronous (RabbitMQ) communication mechanisms to interact with the Command Service.
- **Command Service**: Handles command processing logic, manages an in-memory database, and listens for RabbitMQ events to persist data.

### Communication Flow

- **Synchronous Calls**:
  1. **HTTPClient**: The Platform Service notifies the Command Service upon creation of a platform.
  2. **gRPC**: The Command Service fetches platform data from the Platform Service at startup to seed its database.
  
- **Asynchronous Calls**:
  - **RabbitMQ**: When a platform is created, an asynchronous message is sent from the Platform Service to the Command Service, triggering an event to inject data into the Command Service's database.

## Kubernetes Setup

The project is fully containerized and deployed in a Kubernetes environment. Below are the components and services set up:

- **Ingress Nginx**: Serves as an API Gateway, routing traffic to the appropriate services.
- **MSSQL**: Deployed with persistent storage using a Persistent Volume Claim (PVC).
- **Platform Service**: Exposed with a NodePort for testing with Postman (only for local testing).
- **Command Service**: Manages business logic and interacts with RabbitMQ and MSSQL.
- **RabbitMQ**: Acts as the message bus for asynchronous communication between services.
- **Local PVC**: Ensures persistent storage for the MSSQL database.
- **NodePort Service for Platform**: Exposes the Platform Service for local testing using Postman.

### Kubernetes Deployments

- **MSSQL Deployment**: Includes persistent volume claims to ensure data is retained across pod restarts.
- **Command Service Deployment**: Deployed in the Kubernetes cluster, handling business logic and interacting with RabbitMQ and MSSQL.
- **Platform Service Deployment**: Exposed with a NodePort for manual testing.
- **RabbitMQ Deployment**: Set up as the message bus for asynchronous communication.

### Docker Configuration

- Each service (Platform Service and Command Service) has its own **Dockerfile** to build the respective images.
- A global **Dockerfile** is provided to containerize the entire project for deployment.

### Service Interactions

#### **Command Service**

- **In-Memory Database**: The Command Service uses an in-memory database for simplicity.
- **gRPC Initialization**: On startup, the Command Service makes a gRPC call to the Platform Service to fetch and seed initial data.
- **RabbitMQ Event Processing**: The Command Service listens for RabbitMQ events, processes the data, and stores it in the database.
- **Mappers**: Data transfer objects (DTOs) are mapped to entity models using custom mappers.

#### **Platform Service**

- **MSSQL Database**: The Platform Service uses MSSQL for platform data storage.
- **gRPC Server**: Acts as a gRPC server to serve platform data to the Command Service.
- **Synchronous HTTPClient Call**: The Platform Service sends a sync HTTP request to notify the Command Service upon platform creation.
- **Asynchronous RabbitMQ Call**: Sends an object asynchronously to the Command Service via RabbitMQ.
- **Mappers**: DTOs are mapped to entity models using custom mappers.

## Known Limitations

- **No Circuit Breaker / Retry Mechanisms**: No circuit breaker, retry policies (e.g., Polly), or fault tolerance mechanisms implemented.
- **No Persistent Volume for RabbitMQ**: RabbitMQ data will be lost if the service is down.
- **Architecture Could Be Improved**: The code could be refactored into layered architecture (e.g., service layer, data access layer).
- **Generic Repository Missing**: A generic repository pattern is not used but would improve the abstraction and reusability of data access logic.

## Possible Improvements

1. **Circuit Breaker and Retry**: Implement a circuit breaker and retry mechanism (e.g., Polly) for better fault tolerance.
2. **Persistent RabbitMQ Storage**: Set up persistent volume claims for RabbitMQ to ensure data retention during downtime.
3. **Refactor to Layers**: Refactor the codebase into more modular layers for better maintainability (e.g., service, repository, controller layers).
4. **Generic Repository**: Implement a generic repository pattern for better abstraction and reusability in data access logic.
5. **Scalability**: Set up Horizontal Pod Autoscaling (HPA) for services to scale based on load.

## How to Test Locally

1. **Build and Deploy with Docker**: 
   - Use the provided `Dockerfile`s to build images for each service.
   - Build and run the containers locally.

2. **Deploy to Kubernetes**: 
   - Use the deployment YAML files to deploy services to your Kubernetes cluster.

3. **Access Platform Service**: 
   - Use Postman to test the Platform Service exposed via NodePort (for local testing).

4. **Verify Communication**: 
   - Test both synchronous (HTTPClient, gRPC) and asynchronous (RabbitMQ) communication between services.

---

> **Note**: This repository is designed to showcase the basic communication between two services in a Kubernetes environment using multiple communication patterns. Further improvements in terms of reliability, scalability, and maintainability can be added as needed.
