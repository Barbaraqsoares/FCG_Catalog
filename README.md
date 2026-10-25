# FCG Catalog

API REST de catálogo de jogos e promoções, com operações de biblioteca de jogos
e publicação de eventos de compra no RabbitMQ.

## Visão geral

A solução contém o projeto ASP.NET Core `FCG_CatalogAPI`, desenvolvido em .NET 8.
O projeto utiliza **arquitetura hexagonal** (Ports and Adapters): o domínio e os
casos de uso definem as regras e as portas de que precisam, enquanto adaptadores
de entrada e saída conectam essas regras a tecnologias externas. Assim, controllers,
SQL Server e RabbitMQ são detalhes de integração, e não devem conter as regras
centrais do negócio.

No código, a arquitetura está organizada assim:

- **Domain**: núcleo do domínio — entidades, eventos, exceções e portas
  (interfaces), como `IGameRepository` e `IEventPublisher`.
- **Application/UseCases**: coordena os fluxos da aplicação e aplica as regras
  de uso por meio das portas do domínio, sem depender diretamente dos serviços
  externos.
- **Adapters/Inbound**: adaptadores de entrada HTTP — controllers, requests e
  responses que recebem chamadas e invocam os casos de uso.
- **Adapters/Outbound**: adaptadores de saída — implementam portas para acessar
  o SQL Server e publicar eventos no RabbitMQ.
- **Infrastructure**: configura e conecta os adaptadores, o Entity Framework
  Core, as migrations e a inicialização do banco.

Por exemplo, ao publicar um evento, o caso de uso depende da porta
`IEventPublisher`; `RabbitMqEventPublisher` é o adaptador de saída que implementa
essa porta usando MassTransit. Isso permite manter o fluxo de negócio separado
da tecnologia de mensageria.

### Fluxos principais

1. A API recebe uma requisição no controller e delega a operação ao use case.
2. O use case aplica as regras da operação e usa os repositórios para consultar
   ou persistir dados no SQL Server.
3. Ao solicitar a compra de um jogo (`POST /api/LibraryOfGames/{gameId}`), a
   aplicação publica um `OrderPlacedCommand` no RabbitMQ usando MassTransit,
   contendo `UserId`, `GameId` e `Price`. Essa solicitação não adiciona o jogo à
   biblioteca. A API consome `PaymentProcessedEvent` e somente quando `Status`
   é `Approved` registra o jogo para o `UserId` do evento, usando
   `ProcessedAtUtc` como data da compra. Outros status são ignorados e eventos
   aprovados repetidos não duplicam uma entrada já existente.
4. Na inicialização, a API aplica as migrations do Entity Framework Core e
   insere jogos de exemplo se o catálogo estiver vazio. A biblioteca não recebe
   dados de seed: suas entradas são criadas somente após pagamento aprovado.
   Se a preparação do banco falhar, a aplicação não inicia.

O publisher (`IEventPublisher`) é um serviço interno da API, não um recurso que
aparece no painel do RabbitMQ. A API configura um consumer para
`PaymentProcessedEvent`; o MassTransit cria a fila/endereço de consumo ao iniciar
o bus. A API não configura um consumer para `OrderPlacedCommand`; outro serviço
deve consumi-lo.

## Requisitos

- .NET SDK 8.0 para executar a API localmente.
- Docker Desktop com Docker Compose, para iniciar SQL Server e RabbitMQ.
- Portas disponíveis:
  - `1433`: SQL Server.
  - `5672`: conexão com RabbitMQ.
  - `15672`: painel de administração do RabbitMQ.
  - `5208`: API local (perfil HTTP).
  - `8080`: API no Docker Compose.

Ao iniciar pelo Docker Compose, o Swagger fica disponível em
`http://localhost:8080/swagger`. No Visual Studio, use o perfil
`Container (Dockerfile)` para abrir essa mesma URL automaticamente; o perfil
usa HTTP e publica a porta 8080 do host para a porta 8080 do container.

## Configuração

### SQL Server

A aplicação usa a connection string `ConnectionStrings:SqlConnection` (ou,
alternativamente, `ConnectionStrings:DefaultConnection`). O SQL Server precisa
estar acessível antes de iniciar a API.

**Atenção:** as connection strings atualmente presentes em `appsettings.json` e
`docker-compose.yaml` têm `******` no lugar da senha/parâmetro de senha. Esse
texto não é uma connection string válida e deve ser corrigido antes de executar
a aplicação. A senha de desenvolvimento definida no Compose é
`Your_password123`; a string correspondente deve conter
`User Id=sa;Password=Your_password123;`.

Para desenvolvimento local, configure a connection string por variável de
ambiente (ela sobrescreve o valor de `appsettings.json`):

```powershell
$env:ConnectionStrings__SqlConnection = "Server=localhost,1433;Database=CatalogDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"
```

Se estiver usando outro SQL Server, ajuste servidor, porta, banco, usuário,
senha e opções de certificado conforme seu ambiente.

### RabbitMQ e MassTransit

As chaves de configuração aceitas pela aplicação são:

| Chave | Padrão | Descrição |
| --- | --- | --- |
| `RabbitMq:Host` | `localhost` | Hostname do broker |
| `RabbitMq:VirtualHost` | `/` | Virtual host |
| `RabbitMq:Username` | `guest` | Usuário do broker |
| `RabbitMq:Password` | `guest` | Senha do broker |

No Docker Compose, o host precisa ser `rabbitmq` (nome do serviço na rede
Compose). Fora do Docker, use `localhost` se o broker estiver publicado na
máquina local.

Exemplo de configuração para executar a API localmente no PowerShell:

```powershell
$env:RabbitMq__Host = "localhost"
$env:RabbitMq__VirtualHost = "/"
$env:RabbitMq__Username = "guest"
$env:RabbitMq__Password = "guest"
```

O broker deve estar em execução para que publicação e consumo de eventos
funcionem. As
credenciais `guest/guest` são apenas as credenciais locais de desenvolvimento
definidas no Compose; use credenciais apropriadas em outros ambientes.

## Como executar

### Opção 1: API local e dependências em Docker

Na raiz do repositório, inicie somente os serviços de infraestrutura:

```powershell
docker compose up -d sqlserver rabbitmq
```

Configure as variáveis de ambiente de SQL Server e RabbitMQ mostradas acima e
execute a API:

```powershell
dotnet restore FCG_Catalog.sln
dotnet run --project src/FCG_CatalogAPI/FCG_CatalogAPI.csproj
```

O perfil HTTP do projeto usa `http://localhost:5208`. Abra
`http://localhost:5208` para acessar o Swagger UI. O painel do RabbitMQ fica em
`http://localhost:15672` (usuário e senha de desenvolvimento: `guest` / `guest`).

Para encerrar os serviços de infraestrutura iniciados pelo Compose:

```powershell
docker compose down
```

### Opção 2: API e dependências em Docker Compose

Antes de subir a aplicação, corrija no `docker-compose.yaml` a variável
`ConnectionStrings__SqlConnection` do serviço `webapp`. Ela deve ter o formato:

```yaml
- ConnectionStrings__SqlConnection=Server=sqlserver,1433;Database=CatalogDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;
```

O valor precisa corresponder à senha configurada em `MSSQL_SA_PASSWORD` no mesmo
arquivo. Depois, na raiz do repositório, execute:

```powershell
docker compose up --build
```

O Compose inicia SQL Server, RabbitMQ e API; a API fica disponível em
`http://localhost:8080`, e o RabbitMQ em `http://localhost:15672`. Para parar os
serviços:

```powershell
docker compose down
```

Os dados do SQL Server ficam no volume `sqlvolume` e são mantidos entre
reinicializações. Para uma execução local de desenvolvimento, não use as senhas
de exemplo deste README em um ambiente exposto.

### Kubernetes

Os manifests ficam em [`k8s/`](./k8s/) e criam Deployments e Services para a API,
SQL Server e RabbitMQ. O SQL Server usa um PersistentVolumeClaim; configurações
não sensíveis ficam no ConfigMap e as credenciais são lidas de um Secret.

Antes de aplicar, substitua os valores `CHANGE_ME_...` em
[`k8s/01-secret.yaml`](./k8s/01-secret.yaml) por senhas fortes. A senha do SQL
Server configurada ali é usada tanto no container do banco quanto na connection
string da API. Não use os valores de exemplo em ambientes compartilhados ou de
produção; para esses ambientes, forneça o Secret por um gerenciador seguro e não
armazene credenciais reais no repositório.

Na raiz do repositório, construa a imagem da API e aplique os manifests:

```powershell
docker build -t fcg-catalog-api:latest .
kubectl apply -f k8s/
```

O cluster precisa conseguir acessar a imagem `fcg-catalog-api:latest`. Em
clusters que não compartilham o daemon local do Docker, publique a imagem em um
registry acessível e atualize `image` no Deployment em
[`k8s/04-deployments.yaml`](./k8s/04-deployments.yaml). O provisionamento do
PersistentVolume depende do StorageClass padrão do cluster.

Consulte os Deployments com `kubectl get deployments`. Para acessar a API e o
painel de administração do RabbitMQ localmente, execute cada comando de
port-forward em um terminal separado:

```powershell
kubectl port-forward service/fcg-catalog-api 8080:8080
kubectl port-forward service/rabbitmq 15672:15672
```

O Swagger fica em `http://localhost:8080/swagger`; o painel do RabbitMQ fica em
`http://localhost:15672`, usando as credenciais definidas no Secret.

## Endpoints

O Swagger UI documenta os endpoints quando a API está em execução.

| Método | Rota | Operação |
| --- | --- | --- |
| `GET` | `/Game` | Lista jogos |
| `GET` | `/Game/{id}` | Consulta um jogo |
| `POST` | `/Game` | Cria um jogo |
| `PUT` | `/Game/{id}` | Atualiza um jogo |
| `DELETE` | `/Game/{id}` | Remove um jogo |
| `GET` | `/api/Promotions` | Lista promoções |
| `GET` | `/api/Promotions/{id}` | Consulta uma promoção |
| `POST` | `/api/Promotions` | Cria uma promoção |
| `DELETE` | `/api/Promotions/{id}` | Remove uma promoção |
| `POST` | `/api/LibraryOfGames/{gameId}` | Adiciona jogo à biblioteca e publica evento |
| `GET` | `/api/LibraryOfGames/{userId}` | Consulta a biblioteca de um usuário |

Exemplo de criação de jogo:

```json
{
  "name": "Novo jogo",
  "description": "Descrição do jogo",
  "price": 49.90,
  "isMultiplayer": false
}
```

Exemplo de criação de promoção:

```json
{
  "name": "Promoção de exemplo",
  "discount": 10,
  "startDate": "2026-10-24T00:00:00Z",
  "endDate": "2026-10-31T23:59:59Z",
  "gameIds": [1, 2]
}
```

O endpoint de criação de promoção procura o identificador do usuário nas claims
da requisição. A solução não configura atualmente um provedor de autenticação,
portanto esse endpoint requer que a autenticação/claim seja integrada para
funcionar. A biblioteca também usa valores de usuário simulados em parte do
fluxo; não representa ainda uma integração completa com identidade de usuário.

## Contrato do evento de pagamento

O consumer recebe a mensagem `PaymentProcessedEvent` com este contrato:

```csharp
PaymentProcessedEvent(
    Guid OrderId,
    int UserId,
    int GameId,
    decimal Price,
    string Status,
    DateTime ProcessedAtUtc,
    string? Reason);
```

`Status` igual a `Approved` (sem distinção entre maiúsculas e minúsculas) adiciona
o jogo à biblioteca do usuário indicado. O consumer é um adaptador de entrada
MassTransit e delega a regra ao `UserGameUseCase`; status diferentes de `Approved`
são confirmados sem alterar a biblioteca. A consulta anterior à gravação torna
reentregas do evento idempotentes para um jogo que já esteja na biblioteca.

## Verificação dos eventos

1. Inicie a API com SQL Server e RabbitMQ acessíveis.
2. Use `POST /api/LibraryOfGames/{gameId}` com um ID de jogo existente (os jogos
   iniciais são inseridos no primeiro start, se a tabela estiver vazia).
3. Verifique o `OrderPlacedCommand` publicado na exchange correspondente no
   painel do RabbitMQ.
4. Publique um `PaymentProcessedEvent` para a exchange do tipo e confirme que
   um evento `Approved` cria a entrada de biblioteca correspondente ao usuário.

O consumer de pagamento usa a configuração RabbitMQ já descrita e é registrado
automaticamente pelo MassTransit. Se o processamento falhar, confira os logs da
API, a fila do consumer, o estado do SQL Server e as credenciais do broker.

## Build

Para restaurar dependências e compilar a solução:

```powershell
dotnet restore FCG_Catalog.sln
dotnet build FCG_Catalog.sln --no-restore
```

## Testes unitários e cobertura

Os testes ficam em `teste/FCG_CatalogAPI.Tests`. Eles cobrem os casos de uso,
controllers, repositórios, middleware, publicação de eventos, modelos de domínio
e o pipeline HTTP. Os repositórios são exercitados com o provedor em memória do
Entity Framework Core; os serviços externos não precisam estar ativos para rodar
os testes.

Execute os testes e a coleta de cobertura a partir da raiz do repositório:

```powershell
dotnet test FCG_Catalog.sln --settings teste\FCG_CatalogAPI.Tests\coverlet.runsettings --collect:"XPlat Code Coverage"
```

A coleta aplica um limite de **100% de cobertura de linhas** ao código de
`Domain`, `Application` e `Adapters`. `Program.cs`, migrations geradas e
`Infrastructure\` são excluídos desse indicador por dependerem de inicialização
da aplicação, código gerado ou configuração/serviços externos. O relatório
de cobertura é salvo em `teste\FCG_CatalogAPI.Tests\TestResults`.
