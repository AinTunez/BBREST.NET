# BBREST.NET

This library allows for easy interaction with [Blackboard Learn REST APIs](https://developer.blackboard.com/portal/displayApi) in application code.

## Setup
Declare the library.
```c#
using BBREST;
```
Build a new `RestApp` instance for your application.
```c#
string key = "myApplicationKey";
string secret = "myApplicationSecret";
string origin = "https://myproductionsite.com";

RestApp mySchool = new RestApp(origin, key, secret);
```
## Usage

### `RestApp.Request`

The `RestApp` class has only one true public method –– the `Request` method. The access token is refreshed
automatically as needed. `Request` operates on this syntax:

```c#
mySchool.Request(string method, string path, (string|object) jsonObject);
```

The other public methods (`GET`,`POST`, `PATCH`,`PUT`, and `DELETE`) are shorthand for entering
the corresponding string as the `method` argument. 

##### `method`

The `method` argument is a string referring to any of five HTTP verbs: `GET`,`POST`,
`PATCH`,`PUT`, or `DELETE`. Case does not matter.

##### `path`

The `path` argument is a string referring to the path of the endpoint to which you want
to make a call. It finds the main API directory automatically. You only need to
include the path after `/learn/api/public/` –– for instance, `v1/courses/_9999_1`.

##### `jsonObject`

The `jsonObject` argument contains the data you want to send to the endpoint. It can be any `object`;
the `RestApp` class automatically serializes it into a JSON string. You can also pass a JSON string directly.

If making a `GET` request, you can leave `jsonObject` blank.

#### Example

The `Request` method is awaitable and should generally be used inside an an `async` method:

```c#
string myEndpointPath = "v1/courses/_9999_1";
string myJsonString = @"{""name"":""New Course Name""}";

await mySchool.Request("PATCH", myEndpointPath, myJsonString);
```

### `BlackboardResponse`

`Request` will return an instance of the `BlackboardResponse` class. It contains two properties:

* `HttpResponse Response` –– the `HttpResponse` returned by the request.
* `HttpContent Content` –– the `HttpContent` inside `Response`.

It also contains one helper method:

* `string ReadContentAsync()` –– asyncronously returns the JSON string in the response.

#### Example

Building on the above example, you can retrieve the returned object like so:

```c#
BlackboardResponse response = await mySchool.Request("PATCH", myEndpointPath, myJsonString);
string jsonResponse = await response.ReadContentAsync();
```
