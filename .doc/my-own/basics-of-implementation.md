# Basics of Implementation

This is my working summary (macro analysis) of the DeveloperStore evaluation.

## Summary

We're building a set of REST API endpoints for the DeveloperStore team. The core deliverable is a Sales API with full CRUD. The other resources (Products, Carts, Users, Auth) come from the template and I keep them working and consistent.
TODO

Layer Architecture: Clean Architecture (Domain, Application, ORM, Messaging and WebAPI) + Event-Driven Architecture.

Patterns and designs used:
- Repository pattern (following the template).
- External identity provider.
- Domain Driven Design.
- Mediator pattern.

## A brief about the APIs

### Sales API

Fields: 
* Sale number
* Date when the sale was made
* Customer (external id plus name)
* Total sale amount
* Branch where the sale was made (external id plus name)
* Each one of the products/cart_items (external id plus name)
    - Quantities per item
    - Unit prices per item
    - Discounts per item
    - Total amount per item
    - Cancelled or not cancelled. Obs: at sale level and at item level

Endpoints:

- `POST /sales` must create a sale. Validate the items, apply the discount rules, calculate item totals and the sale total, then persist. SaleCreated must be raised.
- `GET /sales` must list sales with pagintion, ordering and filtering.
- `GET /sales/{id}` must return one sale by id with all its items.
- `PUT /sales/{id}` must must update a sale and recalculate discounts/totals. SaleModified must be raised.
- `PATCH /sales/{id}/cancel` must cancel the whole sale without deleting it. sale. SaleCancelled must be raised.
- `DELETE /sales/{id}` must cancel + apply the logical exclusion of a sale. SaleCancelled must be raised
- `PATCH /sales/{id}/items/{itemId}/cancel` must cancel a single item and recalc sale total. ItemCancelled must be raised.

#### Business rules for discounts (they will be implemented in Domain Layer):

- 4 or more identical items: 10 percent discount on that item.
- 10 to 20 identical items: 20 percent discount on that item.
- More than 20 identical items: not allowed.
- Fewer than 4 identical items: no discount.

### Products API

Fields: id, title, price, description, category, image and a rating object (rate and count).

- `GET /products` must list products with pagination, ordering and filtering.
- `POST /products` must create a product.
- `GET /products/{id}` must get one product.
- `PUT /products/{id}` must update a product.
- `DELETE /products/{id}` must delete a product.
- `GET /products/categories` must list all disticnt categories.
- `GET /products/category/{category}` must list products in one category, with pagination, ordering and filtering.

### Carts API

Fields: id, userId, date and a list of products with product Id and quantity.

- `GET /carts` must list carts with pagination, ordering and filtering.
- `POST /carts` must create a cart
- `GET /carts/{id}` must get one cart.
- `PUT /carts/{id}` must update a cart.
- `DELETE /carts/{id}` must delete a cart.

### Users API

Fields: id, email, username, password, name (firstname, lastname), address (city, street, number, zipcode, geolocation), phone, status (Active, Inactive, Suspended) and role (Customer, Manager, admin)

- `GET /users` must list users with pagination, ordering and filtering.
- `POST /users` must create a user. Password is hashed, never stored in plain text.
- `GET /users/{id}` must get one user.
- `PUT /users/{id}` must update a user.
- `DELETE /users/{id}` must delete a user.

Part of this already exists in the template. I keep it aligned with the rest.

### Auth API

- `POST /auth/login` must take username and password, check the credentials and return a JWT token

Protected endpoints will be excepting that token in the authorization header.

## Rules that apply to every list endpoint

### Pagination:

Request:
- `_page` is page number - default 1
- `_size` is page size - default 10

Response:

```json
{
  "data": [],
  "totalItems": 0,
  "currentPage": 1,
  "totalPages": 0
}
```

### Ordering:

- `_order`: for example `price desc, title asc`. Default direction is asc. Field names match the JSON response.

### Filtering:

- `field=value`: exact match.
- `field=value*` or `field=*value`: partial match on string fields.
- `_minField` and `_maxField`: range filter on numeric and date fields.
- Combine filters with `&`.

## Error handling

Standard HTTP status codes. 2xx success, 4xx client error, 5xx server error.

Error body:

```json
{
  "type": "string",
  "error": "string",
  "detail": "string"
}
```

`type` is pre-setted, `error` is a short summary, `detail` explains this specific case.

## Events

For the sales flow I raise and publish: SaleCreated, SaleModified, SaleCancelled and ItemCancelled.

Publishing to a real message broker is not mandatory. I can log the payload in the application log, or send it through the service bus. Either way the event contract stays clean so I could plug a broker in later.

## Tech that has to be used

- .NET 8.0
- C#
- PostgreSQL
- MongoDB
- Angular (frontend must not be implemented - i checked with the recruiter)
- MediatR
- AutoMapper
- Rebus
- EF Core
- FluentValidation
- xUnit, NSubstitute and Bogus

## Project structure and workflow

```
root
  src/
  tests/
  README.md
```

### Important Notes/Decisions

- A sale with no active items is not a valid state. Cancelling the last (only) active item of a sale triggers the transitions the whole sale to `Cancelled` raising `ItemCancelled` and `SaleCancelled`.

- I designed 4 APIs: `SalesAPI`, `ProductsAPI`, `CartsAPI`, `UsersAPI`. The single auth endpoint lives inside `UsersAPI` for simplicity.

- I've used `Guid` as the default identity type.

- The discount rules belong only to the Sales context. The cart dont know about them.

- Main flow: `POST /sales` is synchronous and is the only way a sale is created. 
  1. it takes `{ cartId, branchId }`
  2. reads the cart lines
  3. read price and title for each `ProductsAPI`
  4. it resolves the branch name locally
  5. applies the discounts
  6. persists and answers `201` or `422`
  7. raises the event `SaleCreated`

- `CartsAPI` consumes `SaleCreated` and moves the matching cart to `CheckedOut`. A checked-out cart is the final state for carts.

- Unit prices and product titles come from `ProductsAPI` at sale time. This assures that a later change in pricing never rewrites an existing sale.

- The sale number is a global sequential value in the format `SALE-000123` - database sequence.

- `SalesAPI` and `UsersAPI` run on PostgreSQL with EF Core. `ProductsAPI` and `CartsAPI` run on MongoDB, because the product and the cart fitted the document model with the use of nested value objects and line collections - we dont have cross-row variants neither.

- Cancellation never deletes a row. `PATCH /{id}/cancel` sets the status, `DELETE /{id}` applies a soft delete (logical exclusion) marker so the sale leaves the GET results.

- I promoted Email/Username/PasswordHash/PersonName/Address/Geolocation/Phone to real value objects.

- I'm using brazilian phone format for the phoneNumber ValueObject.

- Only Admins can change role of users to Admin or create Admin users.

- I deleted UserRegisteredEvent to create UserRegisteredDomainEvent. I've made a lot of changes to the user template.