# PraktikWallet

## Running the Project with Docker

To run the PraktikWallet project using Docker, follow these steps:

### Prerequisites

- Ensure Docker and Docker Compose are installed on your system.
- The project requires .NET version 8.0 as specified in the Dockerfile.

### Environment Variables

- No specific environment variables are required to run the project as per the provided Docker configuration.

### Build and Run Instructions

1. Navigate to the project root directory containing the `docker-compose.yml` file.
2. Execute the following command to build and start the services:

   ```bash
   docker-compose up --build
   ```

3. Access the Wallet API service at `http://localhost:80`.

### Exposed Ports

- `walletapi` service: Port `80` is exposed for HTTP traffic.
- `mongodb` service: Port `27017` is exposed for database access.

### Additional Notes

- The `mongodb` service uses a named volume `mongodb_data` to persist data.
- The `walletapi` service depends on the `mongodb` service to be running.

For further details, refer to the Dockerfile and Docker Compose configuration provided in the project.