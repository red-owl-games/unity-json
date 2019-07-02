using UnityEngine;
using RedOwl.Engine;

namespace RedOwl.Examples
{

    public class JsonExample : MonoBehaviour
    {
        private string sampleJson =
            "{" +
            "\"employee\":{ \"name\":\"John\", \"age\":30, \"height\":168.321, \"city\":[\"New York\", null, true, {\"x\": 1, \"y\": 2}]}" +
            "}";

        public void Start()
        {
            // Create a Json Object
            Json data = new Json();

            // Or using collection initializers
            Json collection = new Json
            {
                ["John"] = new Json
                {
                    {"age", 30},
                    {"height", 168.321}, // supports all c# primative datatypes
                    {"position", new Vector2(1.025468f, 2.1f)}, // Can use select Unity datatypes too!
                    {"rotation", Quaternion.identity},
                    {
                        "city", new Json {"New York", Json.Null, true, "Los Angeles"}
                    } // supports arrays and nested objects
                },
                ["justin"] = "test" // Allows for asymmetric datatype collections
            };
            // Or implictly cast
            Json implict = 30;

            // OR build up over time
            data["coins"] = 99;
            data["health"] = -83.2f;
            data["position"] = new Vector2(1, 2);
            data["rotation"] = Quaternion.identity;
            data["unicode"] = "€öäüß✓✓✓✓"; // you can also use unicode strings
            data["foo"]["bar"]["baz"]["value"] = 20.0; // you can easily create nested objects
            data["array"]["cities"] = new Json {"New York", "Los Angeles", "Austin"};


            // Log the Json to see what your getting
            Debug.Log(data);
            //{"coins": 99, "health": -83.2, "position": {"x": 1, "y": 2}, "rotation": {"x": 0, "y": 0, "z": 0, "w": 1}, "unicode": "€öäüß✓✓✓✓", "foo": {"bar": {"baz": {"value": 20}}}, "array": {"cities": ["New York", "Los Angeles", "Austin"]}}
            Debug.Log(collection);
            //{"John": {"age": 30, "height": 168.321, "position": {"x": 1.025468, "y": 2.1}, "rotation": {"x": 0, "y": 0, "z": 0, "w": 1}, "city": ["New York", null, true, "Los Angeles"]}, "justin": "test"}

            // Serialize the Json
            // Standard spec is the kind of Json you'd send at a web API
            string standardSpec = data.ToString();
            // Typed spec allows for deserializing back to original types for better use in unity
            string typedSpec = data.Serialize();
            // Typed spec can easily be decompressed or converted to binary to save space if needed

            Debug.Log(standardSpec);
            Debug.Log(typedSpec);

            // Deserialize the Json
            Json deserialized = Json.Deserialize(typedSpec);

            // Even random strings of data with no type information
            Json random = Json.Deserialize(sampleJson);
            Debug.Log(random);

            // Access properties of both keyed and indexed
            Debug.Log(deserialized["array"]["cities"][2]);

            // Implicit type conversion
            Debug.Log(deserialized["coins"] == 99);
            Debug.Log(deserialized["position"] == new Vector2(1, 2));
            Debug.Log(deserialized["position"] == new Vector2(2, 1));
            // or explict if you need - ** only works if deserialized from a typedSpec **
            Debug.Log((Vector2) deserialized["position"]);
            Debug.Log((Quaternion) deserialized["rotation"]);

            // Throws assert on invalid conversion
            Debug.Log(deserialized["unicode"] == Quaternion.identity);
        }
    }
}
