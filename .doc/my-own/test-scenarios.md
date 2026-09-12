# Test Scenarios

Listing the test scenarios before implementing.

## Identity And Access

### Domain

`User`

- Activating A Suspended User Makes The User Active
- Deactivating An Active User Makes The User Inactive
- Suspending An Active User Makes The User Suspended
- Registering A User Raises Exactly One Registration Event
- Clearing The Domain Events Empties The Collection
- Validating A User Built From Valid Data Succeeds
- Validating A User With Sentinel Status And Role Fails

`Email`

- Constructing An Email From An Invalid Value Throws
- Constructing An Email From A Valid Value Keeps The Value
- Two Emails With The Same Value Are Equal
- Two Emails With Different Values Are Not Equal

`Phone`

- Constructing A Phone From An Invalid Value Throws
- Constructing A Phone From A Valid International Number Keeps The Value
- Two Phones With The Same Value Are Equal

`EmailValidator`

- A Valid Email Passes Validation
- An Empty Email Fails Validation
- An Invalid Email Format Fails Validation
- An Email Longer Than 100 Characters Fails Validation

`PasswordValidator`

- A Valid Password Passes Validation
- An Empty Password Fails Validation
- A Password Shorter Than The Minimum Length Fails Validation
- A Password Without An Uppercase Letter Fails Validation
- A Password Without A Lowercase Letter Fails Validation
- A Password Without A Number Fails Validation
- A Password Without A Special Character Fails Validation

`PhoneValidator`

- The Phone Validator Enforces The International Format

`UserValidator`

- A Valid User Passes Every Validation Rule
- A User With An Unknown Status Fails Validation
- A User With The None Role Fails Validation

`ActiveUserSpecification`

- Only An Active User Satisfies The Active User Specification

### Application

`CreateUserHandler`

- Creating A User From Valid Data Returns The Created User
- Creating A User Stores The Password Hashed
- Creating A User Raises The Registration Event
- Creating A User With An Email That Is Taken Returns A Conflict
- Creating A User With A Username That Is Taken Returns A Conflict
- Creating A User With An Elevated Role And No Admin Caller Is Refused
- Creating An Admin As An Admin Caller Succeeds

### Endpoints

`/api/users`

- Creating A User Returns 201 With The Documented Body
- Creating A User With An Elevated Role And No Admin Caller Returns 400
- Creating A User With An Email That Is Taken Returns 409
- Creating A User From An Invalid Body Returns 400 With The Error Body
- Reading A User That Does Not Exist Returns 404 With The Error Body
- Listing Users Without A Token Returns 401
- Listing Users Pages Orders And Reports The Documented Totals
- Listing Users Filters With A Partial Match And A Range
- Updating A User Replaces The Mutable Fields And Keeps The Password
- Updating A User With A New Password Rehashes It
- Updating A User That Does Not Exist Returns 404
- Updating A User To An Email That Is Taken Returns 409
- Promoting A User To Admin Without An Admin Caller Returns 403

---

## Catalog

### Domain

`Product`

- Creating A Product From Valid Values Populates The Aggregate
- Creating A Product Raises The Created Event
- Updating The Details Raises Exactly One Updated Event
- Deleting A Product Raises The Deleted Event
- Repricing A Product Raises No Event Because The Replica Only Tracks Titles
- Rating A Product Raises No Event Because The Replica Only Tracks Titles
- Creating A Product Without A Description Normalizes It To Empty
- Creating A Product Without A Required Value Object Throws
- Repricing A Product Replaces The Price And Stamps The Update Time
- Rating A Product Replaces The Rating And Stamps The Update Time
- Updating The Details Replaces The Descriptive Fields And Keeps The Price
- Mutating A Product With A Missing Value Object Throws
- Restoring A Product Keeps Its Identity And Timestamps

`ProductTitle`

- Constructing A Product Title From A Blank Value Throws
- Constructing A Product Title Longer Than 200 Characters Throws
- Constructing A Product Title Of Exactly 200 Characters Is Allowed
- Constructing A Product Title Trims The Padding
- Two Product Titles With The Same Value Are Equal
- Two Product Titles With Different Values Are Not Equal

`Money`

- Constructing Money From An Amount That Is Not Positive Throws
- Constructing Money From A Positive Amount Keeps The Amount
- Two Amounts With The Same Value Are Equal
- Two Amounts With Different Values Are Not Equal

`Category`

- Constructing A Category From A Blank Value Throws
- Constructing A Category Longer Than 100 Characters Throws
- Constructing A Category Trims The Padding And Keeps The Casing
- Two Categories Differing Only By Casing Or Padding Are Equal
- Two Categories With Different Names Are Not Equal

`ImageUrl`

- Constructing An Image Url From An Invalid Value Throws
- Constructing An Image Url From An Absolute Url Keeps The Value
- Constructing An Image Url Trims The Padding
- Two Image Urls With The Same Value Are Equal
- Two Image Urls With Different Values Are Not Equal

`Rating`

- Constructing A Rating Outside The Zero To Five Range Throws
- Constructing A Rating With A Negative Count Throws
- Constructing A Rating Inside The Range Keeps The Rate And The Count
- The None Rating Has No Votes
- Two Ratings With The Same Rate And Count Are Equal
- Two Ratings Differing In Count Are Not Equal

---

## Cart

### Domain

`Cart`

- Creating A Cart Produces An Active Aggregate Owned By The Customer
- Creating A Cart Raises No Domain Event
- Creating A Cart Merges Duplicate Lines Summing The Quantities
- Creating A Cart Without A Customer Throws
- Creating A Cart Without A Line List Throws
- Creating A Cart With An Empty Line Throws
- Replacing The Lines Swaps The Whole Set And Stamps The Update Time
- Replacing The Lines Merges Duplicates The Same Way Creation Does
- A Cart That Is No Longer Active Refuses Every Mutation
- Checking A Cart Out Stamps The Sale It Became
- Checking Out A Cart That Is Already Checked Out Changes Nothing
- Checking A Cart Out Without A Sale Throws
- Restoring A Cart Keeps Its Identity Status And Timestamps
- The Line Set Cannot Be Changed From Outside The Aggregate

`CartItem`

- Two Cart Items With The Same Product And Quantity Are Equal
- Two Cart Items Differing In Quantity Are Not Equal
- Replacing The Quantity Leaves The Original Item Untouched
- Constructing A Cart Item Without A Product Reference Throws
- Constructing A Cart Item Without A Quantity Throws

`ProductRef`

- Constructing A Product Reference Without An Id Throws
- Constructing A Product Reference With A Blank Title Normalizes It To Nothing
- Constructing A Product Reference Trims The Title
- A Product Reference Without A Title Is Allowed
- Two Product References Compare By Id And Title

`Quantity`

- Constructing A Quantity Below One Throws
- A Quantity Has No Upper Bound Because The Cap Belongs To Sales
- Adding Two Quantities Sums Them Into A New Instance
- Adding A Quantity That Is Missing Throws
- Adding Past The Integer Ceiling Throws
- Two Quantities With The Same Value Are Equal

### Application

`ProductCatalogEventHandler`

- A Created Product Becomes An Upsert Carrying The Event Time
- An Updated Product Becomes An Upsert Carrying The Event Time
- A Deleted Product Becomes A Removal Carrying The Event Time
- The Bus Handler Does Nothing But Send A Command

`UpsertProductTitleHandler` and `RemoveProductTitleHandler`

- An Event Newer Than The Stored Snapshot Overwrites The Title
- An Event Older Than The Stored Snapshot Is Ignored
- A Redelivered Event Leaves The Replica Exactly As It Was
- A Removal Clears The Title So Later Cart Lines Resolve To Nothing
- A Removal Older Than The Stored Snapshot Is Ignored
- The Upsert Handler Forwards The Event Time Untouched
- The Removal Handler Forwards The Event Time Untouched

### Persistence

`MongoCartRepository`

- A Cart With Several Lines Survives A Round Trip
- The Stored Document Uses The Documented Element Names
- Updating A Cart Replaces The Whole Embedded Line Array
- Deleting A Cart Reports Whether A Document Actually Matched

`MongoCartQueries`

- A Minimum Date Keeps Only The Carts Created On Or After It
- A Maximum Date Keeps Only The Carts Created On Or Before It
- A Date Range Combines Both Limits And Keeps The Cart In The Middle
- A Date In Iso Format Is Compared In Utc And Not Shifted Into The Local Zone
- Ordering By Date Descending Sorts The Newest Cart First
- An Exact User Id Filter Keeps Only That Customers Carts
- An Exact Status Filter Runs As A Plain String Comparison
- The Embedded Line Array Materializes Through The Projection

### Endpoints

`/api/carts`

- Every Cart Endpoint Refuses A Caller Without A Token
- Creating A Cart Returns 201 With A Location Header And The Token Owner
- Creating A Cart With An Empty Product List Returns 400 With The Error Body
- Replacing The Lines Of A Checked Out Cart Returns 409
- Deleting A Checked Out Cart Returns 409
- A Cart Belonging To Someone Else Looks Like It Does Not Exist
- A Customer Listing Carts Sees Only Their Own Totals Included
- A Manager Is Not Scoped To A Single Customer
- Deleting A Cart The Caller Owns Removes It For Good

---

## Sales

### Domain

`Money`

- Constructing An Amount Below Zero Throws
- An Amount Of Zero Is Legal Because A Discount Can Be Nothing
- Constructing An Amount Rounds To Two Decimals Away From Zero
- Adding Two Amounts Produces Their Sum
- Subtracting A Discount From A Gross Produces The Net
- Subtracting Past Zero Throws
- Multiplying By A Quantity Produces The Gross
- Multiplying By A Rate Rounds The Discount On The Totals Path
- An Operator Given A Missing Amount Throws
- Two Amounts With The Same Value Are Equal

`Quantity`

- Constructing A Quantity Below One Throws
- Constructing A Quantity Above Twenty Throws The Item Cap
- A Quantity Inside The Band Keeps Its Value
- Adding Two Quantities Sums Them Into A New Instance
- Adding Two Quantities Past The Cap Throws The Item Cap
- Adding A Quantity That Is Missing Throws
- Two Quantities With The Same Value Are Equal

`DiscountRate`

- Asking For A Rate Outside The Tier Table Throws
- The Three Supported Rates Round Trip Through Their Value
- A Caller Has No Public Constructor To Invent A Rate With
- Two Rates With The Same Value Are Equal

`SaleNumber`

- Constructing A Sale Number From A Malformed Value Throws
- Constructing A Sale Number From A Valid Value Keeps It
- A Sale Number Is Trimmed Before It Is Validated
- A Sequence Value Is Padded To At Least Six Digits
- A Sequence Value That Is Not Positive Throws
- Two Sale Numbers With The Same Value Are Equal

`CustomerRef`

- Constructing A Customer Reference Without An Id Throws
- Constructing A Customer Reference With A Blank Name Throws
- Constructing A Customer Reference Trims The Name
- Two Customer References Compare By Id And Name

`BranchRef`

- Constructing A Branch Reference Without An Id Throws
- Constructing A Branch Reference With A Blank Name Throws
- Constructing A Branch Reference Trims The Name
- Two Branch References Compare By Id And Name

`ProductRef`

- Constructing A Product Reference Without An Id Throws
- A Sold Product Reference Requires A Title Unlike The Cart One
- Constructing A Product Reference Trims The Title
- Two Product References Compare By Id And Title

`SaleItemTotals`

- Totals Whose Net Does Not Equal Gross Minus Discount Throw
- Totals Whose Discount Is Larger Than The Gross Throw
- Totals Missing Any Of The Three Amounts Throw
- Building Totals Derives Gross Discount And Net
- Building Totals Without A Quantity Or A Rate Throws
- Two Sets Of Totals With The Same Amounts Are Equal

`TieredDiscountPolicy`

- Every Quantity Boundary Resolves To Its Documented Rate
- Twenty One Never Reaches The Policy Because The Quantity Threw First
- Resolving Without A Quantity Throws
- The Policy Needs Nothing Injected So A Test Builds It With New

### Endpoints

`/api/sales`

- Every Sale Endpoint Refuses A Caller Without A Token
- Creating A Sale Returns 201 With A Location Header And The Priced Cart
- Creating A Sale Applies Ten And Twenty Percent On The Lines That Earn Them
- Creating A Sale From A Line Above Twenty Units Returns 422
- Creating A Sale At A Branch Nobody Seeded Returns 422
- Creating A Second Sale From The Same Cart Returns 409
- A Cart Belonging To Someone Else Cannot Be Sold
- Reading A Sale By Id Returns Every Item Including The Cancelled Ones
- Reading A Sale That Does Not Exist Returns 404
- Listing Sales Returns The Documented Paging Envelope
- Listing Sales Orders By Total Descending
- Listing Sales Filters By Status And Keeps Soft Deleted Sales Out
- Listing Sales Filters On A Real Sold At Range And A Partial Sale Number
- A Sale Belonging To Someone Else Looks Like It Does Not Exist
- Updating The Quantities Recalculates Every Total
- Updating A Sale With A Product It Never Sold Returns 422
- Updating A Sale With An Empty Item List Returns 422
- Updating A Cancelled Sale Returns 409
- Cancelling A Sale Keeps The Row And Is Idempotent
- Deleting A Sale Hides It Everywhere And Is Idempotent
- Cancelling A Sale That Does Not Exist Returns 404 On Both Paths
- Cancelling One Item Drops The Sale Total By Exactly That Net
- Cancelling The Last Active Item Cancels The Whole Sale
- Cancelling An Item Twice Or On A Cancelled Sale Returns 409
- Cancelling An Item That Is Not On The Sale Returns 404

### Checkout

- The Cart Is Already Checked Out On The Very Next Read
- A Checked Out Cart Refuses Every Further Change
- A Rejected Sale Writes Nothing And Leaves The Cart Active
- Cancelling Or Deleting A Sale Never Reopens Its Cart
- Creating A Sale Leaves An Unprocessed Row That One Cycle Publishes
- Cancelling The Last Item Puts Both Events In The Outbox

---

## Catalog To Cart Replication

- Creating A Product Parks An Event Inside The Document Until A Cycle Runs
- A Rename Reaches The Cart Line Through The Bus
- A Delete Keeps A Tombstone Until Its Event Is Published Then Clears The Replica
- A Product The Replica Never Heard Of Yields A Line With No Title
- The Replica Keeps The Newest Snapshot Whatever Order The Events Arrive In

---

## Shared Infrastructure

- Saving A User Writes One Outbox Row In The Same Transaction
- A Failed Save Writes Neither The User Nor The Outbox Row
- An Outbox Cycle Publishes The Pending Message And Logs It Once
- A Duplicated Dispatch Runs The Handler Only Once
