# RamPaged

A library for seamlessly adding pagination to your dotnet core applications.

## Installing Package

```
dotnet add package RamPaged
```

## Startup Configuration

#### Configuring the DI Container: `ConfigureServices`

There is an extension of `IServiceCollection` named `AddRamPaged`.

```csharp
services.RamPagedPolicyBuilder();
```

## Implementation

Create a query object that inherits from `Pageable`:

```csharp
public class UserQuery : Pageable
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}
```

Using the query object a `PagedList` object can be created.

```csharp
public async Task<PagedList<Contact>> GetUsers(UserQuery query)
{
    bool queryFirstName = !string.IsNullOrWhiteSpace(query.FirstName);
    bool queryLastName = !string.IsNullOrWhiteSpace(query.LastName);
    bool queryEmail = !string.IsNullOrWhiteSpace(query.Email);

    var queryResult = _contactRepo.EntitySet
        .WhereIf(queryFirstName, x => x.FirstName.Contains(query.FirstName))
        .WhereIf(queryLastName, x => x.LastName.Contains(query.LastName))
        .WhereIf(queryEmail, x => x.PrimaryEmail.Contains(query.Email))
        .OrderBy(x => x.LastName)
        .ThenBy(x => x.FirstName);

    var list = await PagedList<Contact>.CreateAsync(queryResult, query);

    return list;
}
```

The factory method for creating a `PagedList` will take a `IQueryable<T>` and return a `PagedList<T>`. There factory method can be called synchronously or asynchronously using `Create` or `CreateAsync`.

You do not have to pass the entire query object into the factory method like the example above. However, you will just need to manually supply the `PageNumber` and `PageSize`:

```csharp
var list = await PagedList<Contact>.CreateAsync(queryResult, query.PageNumber, query.PageSize);
```

I know what you might be thinking... _"But Mr. Repo man, I alter the query result using AutoMapper or some jazz like that... I've already executed the IQueryable."_. Don't worry, we got you covered! You can create a new instance of `PagedList` from an existing list. However, it will then be up to you to supply the count of the total results before pagination.

Lets look at an example of this.

```csharp
public async Task<PagedList<User>> GetUsers(UserQuery query)
{
    bool queryFirstName = !string.IsNullOrWhiteSpace(query.FirstName);
    bool queryLastName = !string.IsNullOrWhiteSpace(query.LastName);
    bool queryEmail = !string.IsNullOrWhiteSpace(query.Email);

    var queryResult = _contactRepo.EntitySet
        .WhereIf(queryFirstName, x => x.FirstName.Contains(query.FirstName))
        .WhereIf(queryLastName, x => x.LastName.Contains(query.LastName))
        .WhereIf(queryEmail, x => x.PrimaryEmail.Contains(query.Email))
        .OrderBy(x => x.LastName)
        .ThenBy(x => x.FirstName);

    // use deferred execution to get the full count of your filtered query
    var resultCount = await queryResult.CountAsync();

    // get the filtered list with just the paged subset
    var results = await queryResult
        .Paged(query)
        .ToListAsync();

    // here we use AutoMapper to convert the list of Contacts to UserSummaries
    var users = _mapper.Map<List<User>>(results);

    // and then create the PagedList of UserSummaries
    var pagedItems = new PagedList<User>(users, resultCount, query);

    return pagedItems;
}
```

When using the `Paged` extension of `IQueryable`, you only provide the `SkipCount` and `PageSize` instead of passing in the whole query object.

```csharp
var results = await queryResult
    .Paged(query.SkipCount, query.PageSize)
    .ToListAsync();
```

Same goes for the `PagedList` constructor:

```csharp
var list = await PagedList<User>.CreateAsync(queryResult, query.PageNumber, query.PageSize);
```

Use the `ControllerBase` extension to place the `X-Pagination` header in the response.

```csharp
[HttpGet]
[Route("", Name = "GetUsers")]
public async Task<ActionResult> GetUsers([FromQuery] UserQuery query)
{
    var result = await _userService.GetUsers(query);

    this.CreatePageableHeader("GetUsers", result, query, _urlHelper);

    return Ok(result);
}
```

To take advantage of this feature, you must give the route a name. In the example above, the route is called `GetUsers`. You must also inject the `UrlHelper` into the controller:

```csharp
private readonly IUrlHelper _urlHelper;

public UserController(IUrlHelper urlHelper)
{
    _urlHelper = urlHelper;
}
```

This creates a fancy header called `X-Pagination` with a json of all our pagination data:

```json
{
  "totalCount": 6,
  "pageSize": 4,
  "currentPage": 1,
  "totalPages": 2,
  "previousPageLink": null,
  "nextPageLink": "https://api.fancymadeupurl.com/things/v1?firstName=oliver&PageNumber=2&PageSize=4"
}
```
