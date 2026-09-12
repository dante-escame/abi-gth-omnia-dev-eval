# Local Environment

Taking some notes about running this locally.

## Docker Compose

The API container comes up as soon as Postgres reports healthy, but applying migrations is always a separate manual step.

- `docker compose up -d` starts everything in the background
- `docker compose up -d --build` rebuilds the API image first
- `docker compose stop` stops the containers and keeps the data
- `docker compose down` removes the containers and keeps the volumes
- `docker compose down -v` removes the containers and wipes the database

Only the database, for when the API runs from the IDE:

- `docker compose up -d ambev.developerevaluation.database`

What is running and where:

- `docker compose ps --format 'table {{.Name}}\t{{.Status}}\t{{.Ports}}'`
- `docker logs ambev_developer_evaluation_webapi`

Ports:

- Postgres `5432:5432`
- Mongo `27017:27017`
- API `8080:8080`

## Waiting for Postgres

The compose healthcheck already gates the API, but a script that runs migrations right after
bring-up still needs its own wait:

```
for i in (seq 1 30)
    docker exec ambev_developer_evaluation_database pg_isready -U developer -d developer_evaluation >/dev/null 2>&1
    and break
    sleep 1
end
```

## Migrations (Important Commands)

```
dotnet tool install --global dotnet-ef
fish_add_path ~/.dotnet/tools
```

```
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
dotnet ef migrations add <Name> --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
dotnet ef migrations remove --force --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
dotnet ef database update InitialMigrations --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
dotnet ef dbcontext info --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

ENV-VARS:

```
set -x ConnectionStrings__DefaultConnection "Host=localhost;Port=5432;Database=other;Username=developer;Password=ev@luAt10n"
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

## Looking inside the database

```
docker exec ambev_developer_evaluation_database psql -U developer -d developer_evaluation -c '\dt'
docker exec ambev_developer_evaluation_database psql -U developer -d developer_evaluation -c '\d "Users"'
docker exec ambev_developer_evaluation_database psql -U developer -d developer_evaluation -c 'select "MigrationId" from "__EFMigrationsHistory" order by 1'
docker exec ambev_developer_evaluation_database psql -U developer -d developer_evaluation -c 'select "Type", "ProcessedOnUtc" is not null as processed, "Error" from outbox_messages'
```

## Smoke test

```
curl -X POST http://localhost:8080/api/users -H 'Content-Type: application/json' -d '{"email":"seed@example.com","username":"seeduser","password":"Passw0rd@1","name":{"firstname":"Seed","lastname":"User"},"address":{"city":"Sao Paulo","street":"Rua Um","number":42,"zipcode":"01000-000","geolocation":{"lat":"-23.5","long":"-46.6"}},"phone":"+5511987654321","status":"Active","role":"Customer"}'

curl -X POST http://localhost:8080/api/auth -H 'Content-Type: application/json' -d '{"email":"seed@example.com","password":"Passw0rd@1"}'

curl "http://localhost:8080/api/users?_page=1&_size=10&_order=username%20asc" -H "Authorization: Bearer $TOKEN"
```

## Throwaway Postgres

```
docker run -d --rm --name devevalprobe -e POSTGRES_PASSWORD=check -e POSTGRES_USER=check -e POSTGRES_DB=check -p 55432:5432 postgres:13
docker stop devevalprobe
```

`--rm` removes it on stop, so nothing is left behind.

## For Bash

- `fish_add_path ~/.dotnet/tools` -> `export PATH="$PATH:$HOME/.dotnet/tools"`
- `set -x NAME value` -> `export NAME=value`
- `for i in (seq 1 30)` -> `for i in $(seq 1 30)`
