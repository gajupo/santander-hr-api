# Santander HR API

A .NET 8 Web API that integrates with HackerNews API to retrieve top stories and their comment counts. This project serves as a caching layer between clients and the HackerNews API, demonstrating:

- **Polly** integration for retry mechanisms.
- **In-memory caching** to reduce calls to Hacker News.
- **Parallel Programming** to efficiently fetch multiple stories simultaneously.
- **Docker** support for containerization.
- **AutoMapper** for object mapping.
- **Problem details** for standardized error responses.
- **Clean Architecture** style separation of concerns.
- **Serilog** for logging.

## Prerequisites

- **.NET 8 SDK**  
  Download from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).
- **Docker**  
  Install Docker Desktop or use Docker with WSL2 on Windows.
- **HackerNews API**
  - Documentation: [HackerNews API Guide](https://github.com/HackerNews/API)
  - Best Stories Endpoint: `https://hacker-news.firebaseio.com/v0/beststories.json`
  - Story Details Endpoint: `https://hacker-news.firebaseio.com/v0/item/{id}.json`  
    Example: `https://hacker-news.firebaseio.com/v0/item/21233041.json`

## Running Locally

1. **Restore** dependencies:
   ```bash
   dotnet restore
   ```
2. **Build:**
   ```bash
   dotnet build
   ```
3. **Run** the API on 5000 port:
   ```bash
   dotnet run --project api/santander-hr-api.csproj --urls "http://localhost:5000"
   ```

After starting, the app listens on ports like **http://localhost:5000**.

4. **Getting** n best stories by providing the count as query paramenter

```js
http://localhost:5000/api/stories?count=10
```

By the default the api returns 10 stories if no count is given.

5. Testing the API from **Swagger**

```js
http://localhost:5000/swagger/index.html
```

## Running Tests

```bash
dotnet test
```

This command runs all tests in the solution, ensuring the API functionalities behave as expected.

## Running with docker

1. **Build** the Docker image (from the repo root), it will execute all tests, if it fails, no image is created:

   ```bash
   docker build -t santander-hr-api .
   ```

2. **Run** the container:
   ```bash
   docker run -d -p 8080:8080 santander-hr-api
   ```

Access the API at `http://localhost:8080/api/stories?count=10`
