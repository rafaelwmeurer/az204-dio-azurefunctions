using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;

public class DataModels
{
    // Modelo de dados genérico para exemplo
    public class UserDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

public class AzureFunctions
{
    // Função para Salvar Arquivos no Storage Account
    [FunctionName("SaveFileToStorage")]
    public static async Task<IActionResult> SaveFileToStorage(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processando upload de arquivo para Storage.");

        // Configurações de conexão do Storage Account
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        string containerName = "documentos"; // Nome do container no Storage

        try
        {
            var file = req.Form.Files[0]; // Assume o primeiro arquivo do form
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            // Cria o container se não existir
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(file.FileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return new OkObjectResult($"Arquivo {file.FileName} salvo com sucesso!");
        }
        catch (Exception ex)
        {
            log.LogError($"Erro ao salvar arquivo: {ex.Message}");
            return new BadRequestObjectResult($"Erro: {ex.Message}");
        }
    }

    // Função para Salvar no CosmosDB
    [FunctionName("SaveToCosmosDB")]
    public static async Task<IActionResult> SaveToCosmosDB(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function salvando documento no CosmosDB.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<DataModels.UserDocument>(requestBody);

        // Configurações do CosmosDB
        string endpoint = Environment.GetEnvironmentVariable("CosmosDBEndpoint");
        string key = Environment.GetEnvironmentVariable("CosmosDBKey");
        string databaseId = "MinhaDatabase";
        string containerId = "Usuarios";

        try 
        {
            var cosmosClient = new CosmosClient(endpoint, key);
            var container = cosmosClient.GetContainer(databaseId, containerId);

            data.Id = Guid.NewGuid().ToString(); // Gera ID único
            data.CreatedAt = DateTime.UtcNow;

            var response = await container.CreateItemAsync(data);
            
            return new OkObjectResult($"Documento salvo com ID: {data.Id}");
        }
        catch (Exception ex)
        {
            log.LogError($"Erro ao salvar no CosmosDB: {ex.Message}");
            return new BadRequestObjectResult($"Erro: {ex.Message}");
        }
    }

    // Função para Filtrar Registros no CosmosDB
    [FunctionName("FilterCosmosDBRecords")]
    public static async Task<IActionResult> FilterCosmosDBRecords(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function filtrando registros no CosmosDB.");

        string nameFilter = req.Query["name"];

        // Configurações do CosmosDB
        string endpoint = Environment.GetEnvironmentVariable("CosmosDBEndpoint");
        string key = Environment.GetEnvironmentVariable("CosmosDBKey");
        string databaseId = "MinhaDatabase";
        string containerId = "Usuarios";

        try 
        {
            var cosmosClient = new CosmosClient(endpoint, key);
            var container = cosmosClient.GetContainer(databaseId, containerId);

            var query = new QueryDefinition("SELECT * FROM c WHERE CONTAINS(c.name, @name)")
                .WithParameter("@name", nameFilter);

            var queryResultSetIterator = container.GetItemQueryIterator<DataModels.UserDocument>(query);
            
            var results = new List<DataModels.UserDocument>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                results.AddRange(currentResultSet);
            }

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            log.LogError($"Erro ao filtrar registros: {ex.Message}");
            return new BadRequestObjectResult($"Erro: {ex.Message}");
        }
    }

    // Função para Listar Registros no CosmosDB
    [FunctionName("ListCosmosDBRecords")]
    public static async Task<IActionResult> ListCosmosDBRecords(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function listando registros no CosmosDB.");

        // Configurações do CosmosDB
        string endpoint = Environment.GetEnvironmentVariable("CosmosDBEndpoint");
        string key = Environment.GetEnvironmentVariable("CosmosDBKey");
        string databaseId = "MinhaDatabase";
        string containerId = "Usuarios";

        try 
        {
            var cosmosClient = new CosmosClient(endpoint, key);
            var container = cosmosClient.GetContainer(databaseId, containerId);

            var query = new QueryDefinition("SELECT * FROM c");
            var queryResultSetIterator = container.GetItemQueryIterator<DataModels.UserDocument>(query);
            
            var results = new List<DataModels.UserDocument>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                results.AddRange(currentResultSet);
            }

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            log.LogError($"Erro ao listar registros: {ex.Message}");
            return new BadRequestObjectResult($"Erro: {ex.Message}");
        }
    }
}
