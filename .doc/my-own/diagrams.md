# DDD Plan — Diagrams

## Context Map

```mermaid
flowchart LR
    subgraph IAM["UsersAPI"]
        U[User Aggregate]
        AUTH[Authentication Service]
    end
    subgraph CAT["Products"]
        P[Product Aggregate]
    end
    subgraph CART["Carts"]
        C[Cart Aggregate]
    end
    subgraph SALES["Sales"]
        S[Sale Aggregate]
        BR[Branch Reference]
    end

    SALES -- "reads cart lines (HTTP Req)" --> CART
    SALES -- "reads price + title (HTTP Req)" --> CAT
    CART  -- "reads product title (HTTP Req)" --> CAT
    IAM   -- "identity via JWT" --> SALES
    IAM   -- "identity via JWT" --> CART
    SALES -- "SaleCreated event" --> CART
    SALES -.->|"SaleModified / SaleCancelled / ItemCancelled"| BUS[(Log - MessageBus Abstracted)]
```

## Identity and Access Management (IAM) — Domain Diagram

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

## Product Catalog Context — Domain Diagram

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

## Cart Context — Domain Diagram

```mermaid
classDiagram
    class Cart {
        <<AggregateRoot>>
        +Guid Id
        +Guid CustomerId
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +CartStatus Status
        +AddItem(productRef, qty) void
        +UpdateItems(lines) void
        +MarkCheckedOut(saleId) void
        +Abandon() void
    }
    class CartItem {
        <<ValueObject>>
        +ProductRef Product
        +Quantity Quantity
    }
    class ProductRef { <<ValueObject>> +Guid Id +string Title }
    class Quantity { <<ValueObject>> +int Value }
    class CartStatus {
        <<enumeration>>
        Active
        CheckedOut
        Abandoned
    }

    Cart "1" *-- "0..*" CartItem : contains
    CartItem *-- ProductRef
    CartItem *-- Quantity
    Cart *-- CartStatus
```

## Sales Context — Domain Diagram

```mermaid
classDiagram
    class Sale {
        <<AggregateRoot>>
        +Guid Id
        +SaleNumber Number
        +DateTime SoldAt
        +CustomerRef Customer
        +BranchRef Branch
        +SaleStatus Status
        +Money Total
        +bool IsDeleted
        +Create(cart, branch, productSnapshots) Sale
        +ModifyItems(lines) void
        +Cancel(reason) void
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

    Sale "1" *-- "1..*" SaleItem : contains
    Sale *-- SaleNumber
    Sale *-- CustomerRef
    Sale *-- BranchRef
    Sale *-- Money : Total
    SaleItem *-- ProductRef
    SaleItem *-- Quantity
    SaleItem *-- DiscountRate
    SaleItem *-- SaleItemTotals
    SaleItem ..> IDiscountPolicy : uses
```