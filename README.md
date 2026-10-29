# Demo de sincronização Android + Redis + .NET

Projeto didático com dados fictícios. Não acessa o banco privado do WhatsApp nem a agenda real do aparelho.

## Componentes

- `api/`: API ASP.NET Core. `POST /contacts` publica comandos na Stream `demo:contacts`; um leitor em segundo plano exibe mensagens da Stream `demo:messages` no console.
- `docker-compose.yml`: Redis local para desenvolvimento.
- Próxima etapa: agente Android de demonstração que cria/consulta seu próprio SQLite e conversa com Redis.

## Pré-requisitos

- .NET SDK 10
- Docker Desktop, com o mecanismo Linux ativo
- Android Studio e um emulador iniciado (o agente de demonstração não exige root)

## Executar Redis e API

Na raiz do projeto:

```powershell
docker compose up -d redis
dotnet run --project .\api\DemoApi.csproj
```

Em outro PowerShell, enfileire um contato fictício:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5000/contacts `
  -ContentType 'application/json' `
  -Body '{"name":"Contato de teste","number":"5511999999999"}'
```

A API devolve `202 Accepted`: o comando foi enfileirado para o agente de demonstração. A porta HTTP pode variar; confira a URL impressa pelo `dotnet run`.

## Streams Redis

- `demo:messages`: mensagens fictícias do agente para a API.
- `demo:contacts`: comandos publicados pela API para o agente.
