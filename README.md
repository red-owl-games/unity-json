# unity-json
Implements the JSON spec in a C# native way allowing for an easy interaction with the JSON spec 

## Why ?!?

Why another Unity json library?

Well i found it a pretty open field and nothing out there supporting the actual Json Spec in a C# native way.

This library is less about serialization/deserialization to Json (though it can do that) and more about crafting
and inspecting complex Json objects.

## Whats it look like?

```cs
// Create a Json Object
Json data = new Json
{
    ["John"] = new Json
    {
        {"age", 30},
        {"height", 168.321},
        {"position", new Vector2(1.025468f, 2.1f)},
        {"rotation", Quaternion.identity},
        {
            "city", new Json {"New York", Json.Null, true, "Los Angeles"}
        }
    },
    ["justin"] = "test"
};
// You can even implictly cast datatypes if needs be
Json implict = 30;

// Log the Json to see what your getting
Debug.Log(data);

// Serialize and Deserialize
Json deserialized = Json.Deserialize(data.Serialize());

// Access data both keyed and indexed
Debug.Log(deserialized["John"]["city"][2]);

// Implicit type conversion
Debug.Log(deserialized["John"]["age"] == 30);
Debug.Log(deserialized["John"]["rotation"] == Quaternion.identity;
// or explict if you need
Debug.Log((Vector2) deserialized["position"]);
Debug.Log((Quaternion) deserialized["rotation"]);
```

### How is this better then something like FullSerializer or SimpleJSon

I built this library mainly to work with custom JSON web API's in a c# way without having to create
domain specific objects that match my web API's.  It also allows me to easily send Unity datatypes in a 
Json compliant way which most unity Json serializers don't allow for.

For Example

```cs
Json data = new Json
{
    ["John"] = new Json
    {
        {"age", 30},
        {"position", new Vector2(1.025468f, 2.1f)},
        {"rotation", Quaternion.identity}
    }
};
```

would net the json

```json
{"John": {"age": 30, "position": {"x": 1.025468, "y": 2.1}, "rotation": {"x": 0, "y": 0, "z": 0, "w": 1}}}
```

Where as if you used something like FullSerializer or SimpleJson you'd get polluted with a bunch of metadata
and type information garbage like the below

```json

```

If this project interests you please get in touch as I'd like to know about your usecases and make sure
I'm covering them.  I'd like this library to grow into a well used Json library for interacting with
web-based Json API's so I have a few quality of life features i'd like to implement in the future.

## Installation

You must be using Unity >= 2018.4!

In your unity project root open Packages/manifest.json

Add the following line to the dependencies section

```
  "com.redowl.unity-json": "https://github.com/red-owl-games/unity-json.git",
```

Open Unity and the package should download automatically and you are ready to go!

```cs
using RedOwl.Engine

Json data = new Json();
```
