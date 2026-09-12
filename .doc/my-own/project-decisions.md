### Important Notes/Decisions

#### About Architecture

- Initially i've drawn `SalesAPI` as a separate microsservice, i changed my mind because a sale conceptually is a checked out cart that has to have it's sale's business rules validated and become effectively a sale.
  If we split the sales entities in a separate context/microservice we would have to handle different distributed software problemas such as:
    - Where to put the discount rules considering that is a sale concept but the carts need it to guarantee the check out event conditions.
    - The response for the user would become async in a pure EDA environment.
      So thinking about boundaries and simplicity, i decided to merge both microservices that i initially have drawn as separate.

- A sale with no active items is not a valid state. Cancelling the last (only) active item of a sale triggers the transitions the whole sale to `Cancelled` raising `ItemCancelled` and `SaleCancelled`.

- I designed 3 APIs: `ProductsAPI`, `CartsAPI`, `UsersAPI`. The single auth endpoint lives inside `UsersAPI` for simplicity.

- I coded all of the 3 API codebases in a single .NET solution de-modularized. This serves only as a simplicity decision for this exercise. Ideally they would be located in differente repositories of differente folder structures (solutions) of the same repository. I worked in architectures before that had the microservices separated by modules inside the same solution in different projects (each one with it's own Program.cs, appsettings and executables).

#### Decisions/Details

- I've used `Guid` as the default identity type.

- Unit prices and product titles come from `ProductsAPI` at sale time. This assures that a later change in pricing never rewrites an existing sale.

- A snapshot is basically the copy of the product title and price taken at the moment of the sale and written into the sale item, so the sale keeps what was true back then instead of pointing at a product that can still change.

- The sale number is a global sequential value in the format `SALE-000123` - this is a database sequence.

- Cancellation never deletes a row. `PATCH /{id}/cancel` sets the status, `DELETE /{id}` applies a soft delete (logical exclusion) marker so the sale leaves the GET results.

- User context: I promoted Email, Username, PasswordHash, PersonName, Address, Geolocation, Phone to real value objects.

- Only Admins can change role of users to Admin or create Admin users.

- I renamed UserRegisteredEvent to UserRegisteredDomainEvent. I've made a lot of changes to the user template.

- In catalog context, i created following events so we can keep a small reference table of what it needs from another context: `ProductCreated`, `ProductUpdated`, `ProductDeleted`.

- I implemented Login rate limiting. Ideally this would be implemented inside our cloud provider environment (such as Azure or AWS API Gateways).