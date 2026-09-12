# Developer Evaluation Project - Sales Environment

Implementation of a Sales API for a DeveloperStore.
Complete CRUD that handles sales records and discounts.

This README.md file was re-written to summarize project architecture and decisions.
The original specifications can be found [here](/.doc/original-specs.md).

---

# Documentation

The files that are stored directly under the `.doc` folder are the specs documentation for the project passed by the recruiter using a template.

The files inside the `.doc/my-own` folder are my personal documentation and they will be referenced here.

I strongly recommend reading the content inside the `.doc/my-own` folder, the intention is to provide a better understanding of the project decisions.

My docs include:

- [Implementation Requirements](/.doc/my-own/basics-of-implementation.md) - A high-level overview of the project architecture written in my words.
- [Project Decisions](/.doc/my-own/project-decisions.md) - A detailed overview of the project architecture and decisions made by me.
- [Architecture Diagrams](/.doc/my-own/diagrams.md) - Bundle of diagrams that show the project architecture - made in mermaid.
- [Local Environment](/.doc/my-own/local-environment.md) - A group of useful commands to setup/reset the local environment.
- [API Documentation](/.doc/my-own/checkout-walkthrough.insomnia.json) - Importable API documentation built in Insomnia.
- [List Of Tests](/.doc/my-own/test-scenarios.md) - Listing the test scenarios before implementing.

---

# How to run the project

Well, you can run the project locally. We have docker compose configured!

1 - Run Docker, add PATH (probably manual in Windows) and then apply the migrations.

```
docker compose down -v
docker compose up -d
export PATH="$PATH:$HOME/.dotnet/tools"   # or fish_add_path once
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

2 - Download Insomnia and import the API documentation.

[API Documentation](/.doc/my-own/checkout-walkthrough.insomnia.json)

3 - Run the Insomnia Journey.

The only manual step of the Insomnia Journey is "04" - Promote the catalog writer do Admin running SQL in the database.
If on Windows, you can do it in Docker Desktop interface probably.

```
docker exec -it ambev_developer_evaluation_database psql -U developer -d developer_evaluation
update "Users" set "Role" = 'Admin' where "Username" = '<admin-username-here>';
```
* These commands will connect to the docker psql instance and promote the user to Admin role.