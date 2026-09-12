# Diagrams

## Context Map

```mermaid
flowchart LR
    subgraph IAM["UsersAPI"]
        U[User Aggregate]
        AUTH[Authentication Service]
    end
    subgraph CAT["ProductsAPI"]
        P[Product Aggregate]
    end
    subgraph SALES["CartsAPI"]
        C[Cart Aggregate]
        S[Sale Aggregate]
        BR[Branch Reference]
    end

    C -- "checkout, same transaction" --> S
    SALES -- "price and title" --> CAT
    IAM -- "identity using JWT" --> SALES
    IAM -- "identity using JWT" --> CAT
    SALES -.->|"SaleCreated / SaleModified / SaleCancelled / ItemCancelled"| BUS[(Logging - Abstraction of Bus)]
```

## Checkout Flow

```mermaid
flowchart TD
    A["We POST /sales with cartId and branchId"] --> B[resolve branch from the seeded list in appsettings]
    B --> C["get the whole Cart aggregate"]
    C --> D{cart is active, owned and not empty}
    D -- no --> E["Error: 422 or 409"]
    D -- yes --> F["get price and title per product"]
    F -- "a product is missing" --> E
    F --> G[open the transaction]

    subgraph TX["transaction"]
        G --> H["allocate the sale number using the db sequence"]
        H --> I["apply the discount tiers and build the Sale"]
        I --> J["checking out saleId - MarkCheckedOut"]
        J --> K["write the sale, its items, the cart and the outbox row"]
    end

    I -- "validation: line above 20 units" --> E
    K --> L[Commit dbTransaction]
    L --> M["Success! 201 with the sale done & cart already CheckedOut"]
    L --> N[Outbox job publishes SaleCreated]
```

## Identity and Access Management (IAM) - Domain Diagram

```mermaid
classDiagram
    class User {
        <<AggregateRoot>>
        +Guid Id
        +Email Email
        +Username Username
        +PasswordHash Password
        +PersonName Name
        +Address Address
        +Phone Phone
        +UserStatus Status
        +UserRole Role
        +Activate() void
        +Deactivate() void
        +Suspend() void
    }
    class Email { <<ValueObject>> +string Value }
    class Username { <<ValueObject>> +string Value }
    class PasswordHash { <<ValueObject>> +string Value }
    class PersonName { <<ValueObject>> +string FirstName +string LastName }
    class Address {
        <<ValueObject>>
        +string City
        +string Street
        +int Number
        +string ZipCode
    }
    class Geolocation { <<ValueObject>> +string Lat +string Long }
    class Phone { <<ValueObject>> +string Value }
    class Authentication {
        <<Svc>>
        +Authenticate(username, rawPassword) User
    }
    class IJwtToken { <<Svc>> +Generate(User) string }

    User *-- Email
    User *-- Username
    User *-- PasswordHash
    User *-- PersonName
    User *-- Address
    User *-- Phone
    Address *-- Geolocation
    Authentication ..> User : verifies
    Authentication ..> IJwtToken : uses
```

## Product Catalog Context - Domain Diagram

```mermaid
classDiagram
    class Product {
        <<AggregateRoot>>
        +Guid Id
        +ProductTitle Title
        +Money Price
        +string Description
        +Category Category
        +ImageUrl Image
        +Rating Rating
        +UpdateDetails(...) void
        +Reprice(Money) void
        +SetRating(Rating) void
    }
    class ProductTitle { <<ValueObject>> +string Value }
    class Money { <<ValueObject>> +decimal Amount }
    class Category { <<ValueObject>> +string Name }
    class ImageUrl { <<ValueObject>> +string Value }
    class Rating { <<ValueObject>> +decimal Rate +int Count }

    Product *-- ProductTitle
    Product *-- Money : Price
    Product *-- Category
    Product *-- ImageUrl
    Product *-- Rating
```

## Sales Context - Domain Diagram

```mermaid
classDiagram
    class Cart {
        <<AggregateRoot>>
        +Guid Id
        +Guid CustomerId
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +CartStatus Status
        +Guid SaleId
        +DateTime CheckedOutAt
        +ReplaceItems(lines) void
        +MarkCheckedOut(saleId) void
    }
    class CartItem {
        <<ValueObject>>
        +ProductRef Product
        +Quantity Quantity
    }
    class CartStatus {
        <<enumeration>>
        Active
        CheckedOut
        Abandoned
    }
    class Sale {
        <<AggregateRoot>>
        +Guid Id
        +SaleNumber Number
        +DateTime SoldAt
        +CustomerRef Customer
        +BranchRef Branch
        +Guid CartId
        +SaleStatus Status
        +Money Total
        +bool IsDeleted
        +Create(number, customer, branch, cartId, lines, policy) Sale
        +ModifyItems(lines, policy) void
        +Cancel(reason) void
        +Delete() void
        +CancelItem(itemId) void
        -Recalculate() void
    }
    class SaleItem {
        <<Entity>>
        +Guid Id
        +ProductRef Product
        +Quantity Quantity
        +Money UnitPrice
        +DiscountRate Rate
        +SaleItemTotals Totals
        +SaleItemStatus Status
        +Cancel() void
    }
    class SaleNumber { <<ValueObject>> +string Value }
    class Money { <<ValueObject>> +decimal Amount }
    class Quantity { <<ValueObject>> +int Value }
    class DiscountRate { <<ValueObject>> +decimal Value }
    class CustomerRef { <<ValueObject>> +Guid Id +string Name }
    class BranchRef { <<ValueObject>> +Guid Id +string Name }
    class ProductRef { <<ValueObject>> +Guid Id +string Title }
    class SaleItemTotals { <<ValueObject>> +Money Gross +Money Discount +Money Net }
    class IDiscountPolicy { <<DomainService>> +Resolve(Quantity) DiscountRate }
    class ISaleNumberGenerator { <<DomainService>> +NextAsync() SaleNumber }
    class IBranchDirectory { <<DomainService>> +Find(Guid) BranchRef }

    Cart "1" *-- "0..*" CartItem : contains
    CartItem *-- Quantity
    Cart ..> Sale : checkout produces

    Sale "1" *-- "1..*" SaleItem : contains
    Sale *-- SaleNumber
    Sale *-- CustomerRef
    Sale *-- BranchRef
    Sale *-- Money : Total
    SaleItem *-- ProductRef
    SaleItem *-- Quantity
    SaleItem *-- DiscountRate
    SaleItem *-- SaleItemTotals
    Sale ..> IDiscountPolicy : uses
    Sale ..> ISaleNumberGenerator : numbered by
    Sale ..> IBranchDirectory : branch resolved by
```

## Sale Event Publishing

```mermaid
flowchart LR
    A[The Sale aggregate raises a domain event] --> B[OutboxInterceptor drains it on SaveChanges]
    B --> C[(outbox_messages row)]
    C --> D[ProcessOutboxJob reads the not yet processed rows]
    D --> E[Mediator publishes the domain event]
    E --> F[IdempotentDomainEventHandler drops a redelivery]
    F --> G[one handler per event maps it to the integration contract]
    G --> H[(Rebus - log)]
    G --> I[payload logged as JSON at Information]
```

## Sale Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Active : POST /sales
    Active --> Recalc: PUT recalculates
    Recalc --> Active: recalc
    Active --> Active : an item is cancelled and others remain
    Active --> Cancelled : PATCH cancel
    Active --> Cancelled : the last active item is cancelled
    Cancelled --> Cancelled : cancel again, nothing raised
    Active --> Deleted : DELETE cancels then soft deletes
    Cancelled --> Deleted : DELETE soft deletes
    Deleted --> Deleted : DELETE again, still 204
```
