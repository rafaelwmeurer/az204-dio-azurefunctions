# Azure Functions - Manipulação de Dados

## Estrutura do Projeto

### Criar Novo Projeto de Azure Functions

1. Abra o Visual Studio ou Visual Studio Code
2. Crie um novo projeto Azure Functions:
   ```bash
   dotnet new functionapp -n DataManagementFunctions
   cd DataManagementFunctions
   ```

### Configuração do Projeto

1. Substitua o conteúdo do arquivo `AzureFunctions.cs` pelo código fornecido anteriormente

2. Adicione os pacotes necessários:
   ```bash
   dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage
   dotnet add package Microsoft.Azure.Cosmos
   dotnet add package Azure.Storage.Blobs
   ```

3. Crie ou atualize o arquivo `local.settings.json`:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "SUA_CONNECTION_STRING_STORAGE",
       "CosmosDBEndpoint": "SEU_ENDPOINT_COSMOSDB",
       "CosmosDBKey": "SUA_CHAVE_COSMOSDB",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet"
     }
   }
   ```

## Executando o Projeto Localmente

### Pré-requisitos
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/pt-br/azure/azure-functions/functions-run-local)

### Executar Localmente
```bash
func start
```

## Endpoints das Funções

### 1. Salvar Arquivo no Storage
- **Método:** POST
- **Endpoint:** `/api/SaveFileToStorage`
- **Descrição:** Faz upload de um arquivo para o Azure Storage

### 2. Salvar no CosmosDB
- **Método:** POST
- **Endpoint:** `/api/SaveToCosmosDB`
- **Descrição:** Salva um documento no CosmosDB
- **Corpo de Exemplo:**
  ```json
  {
    "Name": "João Silva",
    "Email": "joao@exemplo.com"
  }
  ```

### 3. Filtrar Registros no CosmosDB
- **Método:** GET
- **Endpoint:** `/api/FilterCosmosDBRecords?name=João`
- **Descrição:** Filtra registros por nome

### 4. Listar Registros do CosmosDB
- **Método:** GET
- **Endpoint:** `/api/ListCosmosDBRecords`
- **Descrição:** Lista todos os registros

## Deploy no Azure

### Criar Recursos no Azure
1. Crie um Storage Account
2. Crie uma conta CosmosDB
3. Crie um Resource Group

### Publicar Functions
```bash
# Login no Azure
az login

# Criar Resource Group
az group create --name MeuGrupoRecursos --location brazilsouth

# Criar Function App
az functionapp create \
    --name MinhasFunctions \
    --resource-group MeuGrupoRecursos \
    --runtime dotnet

# Publicar
func azure functionapp publish MinhasFunctions
```

## Configurações Importantes

- Substitua as variáveis de ambiente com suas credenciais reais
- No CosmosDB, crie:
  - Database: `MinhaDatabase`
  - Container: `Usuarios`

## Troubleshooting

- Verifique os logs no Portal Azure
- Confirme que todas as variáveis de ambiente estão configuradas
- Valide as permissões de acesso

## Segurança

- Nunca exponha suas connection strings
- Use Azure Key Vault para gerenciar segredos
- Configure níveis de autorização nas Functions
