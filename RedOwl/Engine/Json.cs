

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace RedOwl.Engine
{
    /// <summary>
    /// We need to merge the Json and JsonArray classes
    /// Both can use a dictionary to store their data
    /// We need an Enum to track if the Json class is : NotSet, IsValue, IsContainer
    /// We might also need move all the implict value conversions into this class
    /// The class will not inherit from dictionary and will need to store a dictionray for IsContainer and IJsonValue for IsValue
    /// Maybe we actually have an Enum for JsonValueType : Null, String, Bool, Number, Array, Object ?
    ///
    /// Lastly we need to implement json string parsing and probably convert to Serialize/Deserialize methods where ToString calls Serialize
    /// Well need to look at the msgPack jsonReader/jsonWriter classes to understand string parsing tokenization
    /// </summary>

    public class Json : IEnumerable
    {
        public enum JsonTypes
        {
            Undefined,
            Null,
            String,
            Boolean,
            Number,
            Array,
            Object
        }

        public enum DataTypes
        {
            Undefined,
            Primitive,
            Container,
            Uri,
            Guid,
            DateTime,
            DateTimeOffset,
            TimeSpan,
            Color,
            Vector,
            Quaternion,
            Bounds,
            Rect
        }

        internal JsonTypes JsonType = JsonTypes.Undefined;
        internal DataTypes DataType = DataTypes.Undefined;
        
        private string _string;
        private bool _bool;
        private double _number;
        
        private List<Json> _array = new List<Json>();
        private Dictionary<string, Json> _container = new Dictionary<string, Json>();

        public Json this[int index]
        {
            get => _array[index];
            set
            {
                Assert.IsTrue(JsonType == JsonTypes.Array || JsonType == JsonTypes.Undefined);
                JsonType = JsonTypes.Array;
                DataType = DataTypes.Container;
                _array[index] = value;
            }
        }

        public Json this[string key]
        {
            get
            {
                if (!_container.ContainsKey(key)) _container[key] = Json.Object;
                return _container[key];
            }
            set
            {
                Assert.IsTrue(JsonType == JsonTypes.Object || JsonType == JsonTypes.Undefined);
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Container;
                _container[key] = value;
            }
        }

        public override string ToString()
        {
            switch (JsonType)
            {
                case JsonTypes.Undefined:
                case JsonTypes.Null:
                    return "null";
                case JsonTypes.String:
                    return _string;
                case JsonTypes.Boolean:
                    return _bool.ToString();
                case JsonTypes.Number:
                    return _number.ToString(CultureInfo.CurrentCulture);
                case JsonTypes.Array:
                    StringWriter arrayWriter = new StringWriter();
                    Write(arrayWriter);
                    return arrayWriter.ToString();
                case JsonTypes.Object:
                    StringWriter containerWriter = new StringWriter();
                    Write(containerWriter);
                    return containerWriter.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerator GetEnumerator()
        {
            switch (JsonType)
            {
                case JsonTypes.Array:
                    return _array.GetEnumerator();
                case JsonTypes.Object:
                    return _container.GetEnumerator();
            }
            throw new ArgumentOutOfRangeException();
        }
        
        public void Add(Json value)
        {
            Assert.IsTrue(JsonType == JsonTypes.Array || JsonType == JsonTypes.Undefined);
            JsonType = JsonTypes.Array;
            DataType = DataTypes.Container;
            _array.Add(value);
        }

        public void Add(string key, Json value)
        {
            Assert.IsTrue(JsonType == JsonTypes.Object || JsonType == JsonTypes.Undefined);
            JsonType = JsonTypes.Object;
            DataType = DataTypes.Container;
            _container.Add(key, value);
        }

        #region Types

        private string String
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.String && DataType == DataTypes.Primitive);
                return _string;
            }
            set
            {
                JsonType = JsonTypes.String;
                DataType = DataTypes.Primitive;
                _string = value;
            }
        }
        
        private bool Bool
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Boolean && DataType == DataTypes.Primitive);
                return _bool;
            }
            set
            {
                JsonType = JsonTypes.Boolean;
                DataType = DataTypes.Primitive;
                _bool = value;
            }
        }
        
        private double Number
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Number && DataType == DataTypes.Primitive);
                return _number;
            }
            set
            {
                JsonType = JsonTypes.Number;
                DataType = DataTypes.Primitive;
                _number = value;
            }
        }
        
        private Uri Uri
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.String && DataType == DataTypes.Uri);
                return new Uri(_string);
            }
            set
            {
                JsonType = JsonTypes.String;
                DataType = DataTypes.Uri;
                _string = value.OriginalString;
            }
        }
        
        private Guid Guid
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.String && DataType == DataTypes.Guid);
                return Guid.Parse(_string);
            }
            set
            {
                JsonType = JsonTypes.String;
                DataType = DataTypes.Guid;
                _string = value.ToString();
            }
        }
        
        private DateTime DateTime
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.String && DataType == DataTypes.DateTime);
                return DateTime.SpecifyKind(DateTime.Parse(_string), DateTimeKind.Utc);
            }
            set
            {
                JsonType = JsonTypes.String;
                DataType = DataTypes.DateTime;
                _string = DateTime.SpecifyKind(value, DateTimeKind.Utc).ToString(CultureInfo.CurrentCulture);
            }
        }
        
        private DateTimeOffset DateTimeOffset
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.String && DataType == DataTypes.DateTimeOffset);
                return DateTimeOffset.Parse(_string);
            }
            set
            {
                JsonType = JsonTypes.String;
                DataType = DataTypes.DateTimeOffset;
                _string = value.ToString(CultureInfo.CurrentCulture);
            }
        }
        
        private TimeSpan TimeSpan
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && DataType == DataTypes.TimeSpan);
                return new TimeSpan((long)_number);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.TimeSpan;
                _number = value.Ticks;
            }
        }
        
        private Color Color
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && DataType == DataTypes.Color);
                return new Color(this["r"], this["g"], this["b"], this["a"]);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Color;
                _container = new Dictionary<string, Json> { {"r", value.r}, {"g", value.g}, {"b", value.b}, {"a", value.a}};
            }
        }
        
        private Color32 Color32
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && DataType == DataTypes.Color);
                return new Color(this["r"], this["g"], this["b"], this["a"]);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Color;
                _container = new Dictionary<string, Json> { {"r", value.r}, {"g", value.g}, {"b", value.b}, {"a", value.a}};
            }
        }
        
        private Vector2 Vector2
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && DataType == DataTypes.Vector);
                return new Vector2(this["x"], this["y"]);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Vector;
                _container = new Dictionary<string, Json> { {"x", value.x}, {"y", value.y}};
            }
        }
        
        private Vector3 Vector3
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && DataType == DataTypes.Vector);
                return new Vector3(this["x"], this["y"], this["z"]);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Vector;
                _container = new Dictionary<string, Json> { {"x", value.x}, {"y", value.y}, {"z", value.z}};
            }
        }
        
        private Vector4 Vector4
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && (DataType == DataTypes.Vector || DataType == DataTypes.Quaternion));
                return new Vector4(this["x"], this["y"], this["z"], this["w"]);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Vector;
                _container = new Dictionary<string, Json> { {"x", value.x}, {"y", value.y}, {"z", value.z}, {"w", value.w}};
            }
        }
        
        private Quaternion Quaternion
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && (DataType == DataTypes.Vector || DataType == DataTypes.Quaternion));
                return new Quaternion(this["x"], this["y"], this["z"], this["w"]);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Quaternion;
                _container = new Dictionary<string, Json> { {"x", value.x}, {"y", value.y}, {"z", value.z}, {"w", value.w}};
            }
        }
        
        private Bounds Bounds
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && DataType == DataTypes.Bounds);
                return new Bounds(this["center"], this["size"]);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Bounds;
                _container = new Dictionary<string, Json> { {"center", value.center}, {"size", value.size}};
            }
        }
        
        private Rect Rect
        {
            get
            {
                Assert.IsTrue(JsonType == JsonTypes.Object && DataType == DataTypes.Rect);
                return new Rect(this["x"], this["y"], this["width"], this["height"]);
            }
            set
            {
                JsonType = JsonTypes.Object;
                DataType = DataTypes.Rect;
                _container = new Dictionary<string, Json> { {"x", value.x}, {"y", value.y}, {"width", value.width}, {"height", value.height}};
            }
        }

        #endregion
        
        #region Constructors

        public Json() {}
        public Json(string value) { String = value; }
        public Json(bool value) { Bool = value; }
        public Json(double value) { Number = value; }
        public Json(Uri value) { Uri = value; }
        public Json(Guid value) { Guid = value; }
        public Json(DateTime value) { DateTime = value; }
        public Json(DateTimeOffset value) { DateTimeOffset = value; }
        public Json(TimeSpan value) { TimeSpan = value; }
        public Json(Color value) { Color = value; }
        public Json(Color32 value) { Color32 = value; }
        public Json(Vector2 value) { Vector2 = value; }
        public Json(Vector3 value) { Vector3 = value; }
        public Json(Vector4 value) { Vector4 = value; }
        public Json(Quaternion value) { Quaternion = value; }
        public Json(Bounds value) { Bounds = value; }
        public Json(Rect value) { Rect = value; }

        public Json(IEnumerable<Json> data)
        {
            JsonType = JsonTypes.Array;
            DataType = DataTypes.Container;
            _array = new List<Json>(data);
        }
        
        public Json(IDictionary<string, Json> data)
        {
            JsonType = JsonTypes.Object;
            DataType = DataTypes.Container;
            _container = new Dictionary<string, Json>(data);
        }

        public static Json Null => new Json { JsonType = JsonTypes.Null, DataType = DataTypes.Primitive };
        public static Json Array => new Json { JsonType = JsonTypes.Array, DataType = DataTypes.Container };
        public static Json Object => new Json { JsonType = JsonTypes.Object, DataType = DataTypes.Container };

        #endregion

        #region ToJsonType
        
        public static implicit operator Json(string value) => new Json(value);
        public static implicit operator Json(string[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<string> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(bool value) => new Json(value);
        public static implicit operator Json(bool[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<bool> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(float value) => new Json((double)(decimal)value);
        public static implicit operator Json(float[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<float> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(double value) => new Json(value);
        public static implicit operator Json(double[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<double> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(decimal value) => new Json((double)value);
        public static implicit operator Json(decimal[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<decimal> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(short value) => new Json(value);
        public static implicit operator Json(short[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<short> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(int value) => new Json(value);
        public static implicit operator Json(int[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<int> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(long value) => new Json(value);
        public static implicit operator Json(long[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<long> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(ushort value) => new Json(value);
        public static implicit operator Json(ushort[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<ushort> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(uint value) => new Json(value);
        public static implicit operator Json(uint[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<uint> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(ulong value) => new Json(value);
        public static implicit operator Json(ulong[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<ulong> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(sbyte value) => new Json(value);
        public static implicit operator Json(sbyte[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<sbyte> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(byte value) => new Json(value);
        public static implicit operator Json(byte[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<byte> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        
        public static implicit operator Json(Uri value) => new Json(value);
        public static implicit operator Json(Uri[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Uri> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Guid value) => new Json(value);
        public static implicit operator Json(Guid[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Guid> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(DateTime value) => new Json(value);
        public static implicit operator Json(DateTime[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<DateTime> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(DateTimeOffset value) => new Json(value);
        public static implicit operator Json(DateTimeOffset[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<DateTimeOffset> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(TimeSpan value) => new Json(value);
        public static implicit operator Json(TimeSpan[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<TimeSpan> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Color value) => new Json(value);
        public static implicit operator Json(Color[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Color> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Color32 value) => new Json(value);
        public static implicit operator Json(Color32[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Color32> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Vector2 value) => new Json(value);
        public static implicit operator Json(Vector2[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Vector2> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Vector3 value) => new Json(value);
        public static implicit operator Json(Vector3[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Vector3> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Vector4 value) => new Json(value);
        public static implicit operator Json(Vector4[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Vector4> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Quaternion value) => new Json(value);
        public static implicit operator Json(Quaternion[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Quaternion> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Bounds value) => new Json(value);
        public static implicit operator Json(Bounds[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Bounds> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        public static implicit operator Json(Rect value) => new Json(value);
        public static implicit operator Json(Rect[] value) => new Json(System.Array.ConvertAll(value, x => (Json)x));
        public static implicit operator Json(List<Rect> value) => new Json(System.Array.ConvertAll(value.ToArray(), x => (Json)x));
        
        #endregion

        #region ToOtherTypes
        
        public static implicit operator string(Json value) => value.String;
        public static implicit operator string[](Json value) => value._array.ConvertAll(x => (string)x).ToArray();
        public static implicit operator List<string>(Json value) => value._array.ConvertAll(x => (string)x);
        public static implicit operator bool(Json value) => value.Bool;
        public static implicit operator bool[](Json value) => value._array.ConvertAll(x => (bool)x).ToArray();
        public static implicit operator List<bool>(Json value) => value._array.ConvertAll(x => (bool)x);
        public static implicit operator float(Json value) => (float)value.Number;
        public static implicit operator float[](Json value) => value._array.ConvertAll(x => (float)x).ToArray();
        public static implicit operator List<float>(Json value) => value._array.ConvertAll(x => (float)x);
        public static implicit operator double(Json value) => value.Number;
        public static implicit operator double[](Json value) => value._array.ConvertAll(x => (double)x).ToArray();
        public static implicit operator List<double>(Json value) => value._array.ConvertAll(x => (double)x);
        public static implicit operator decimal(Json value) => (decimal)value.Number;
        public static implicit operator decimal[](Json value) => value._array.ConvertAll(x => (decimal)x).ToArray();
        public static implicit operator List<decimal>(Json value) => value._array.ConvertAll(x => (decimal)x);
        public static implicit operator short(Json value) => (short)value.Number;
        public static implicit operator short[](Json value) => value._array.ConvertAll(x => (short)x).ToArray();
        public static implicit operator List<short>(Json value) => value._array.ConvertAll(x => (short)x);
        public static implicit operator int(Json value) => (int)value.Number;
        public static implicit operator int[](Json value) => value._array.ConvertAll(x => (int)x).ToArray();
        public static implicit operator List<int>(Json value) => value._array.ConvertAll(x => (int)x);
        public static implicit operator long(Json value) => (long)value.Number;
        public static implicit operator long[](Json value) => value._array.ConvertAll(x => (long)x).ToArray();
        public static implicit operator List<long>(Json value) => value._array.ConvertAll(x => (long)x);
        public static implicit operator ushort(Json value) => (ushort)value.Number;
        public static implicit operator ushort[](Json value) => value._array.ConvertAll(x => (ushort)x).ToArray();
        public static implicit operator List<ushort>(Json value) => value._array.ConvertAll(x => (ushort)x);
        public static implicit operator uint(Json value) => (uint)value.Number;
        public static implicit operator uint[](Json value) => value._array.ConvertAll(x => (uint)x).ToArray();
        public static implicit operator List<uint>(Json value) => value._array.ConvertAll(x => (uint)x);
        public static implicit operator ulong(Json value) => (ulong)value.Number;
        public static implicit operator ulong[](Json value) => value._array.ConvertAll(x => (ulong)x).ToArray();
        public static implicit operator List<ulong>(Json value) => value._array.ConvertAll(x => (ulong)x);
        public static implicit operator sbyte(Json value) => (sbyte)value.Number;
        public static implicit operator sbyte[](Json value) => value._array.ConvertAll(x => (sbyte)x).ToArray();
        public static implicit operator List<sbyte>(Json value) => value._array.ConvertAll(x => (sbyte)x);
        public static implicit operator byte(Json value) => (byte)value.Number;
        public static implicit operator byte[](Json value) => value._array.ConvertAll(x => (byte)x).ToArray();
        public static implicit operator List<byte>(Json value) => value._array.ConvertAll(x => (byte)x);
        
        public static implicit operator Uri(Json value) => value.Uri;
        public static implicit operator Uri[](Json value) => value._array.ConvertAll(x => (Uri)x).ToArray();
        public static implicit operator List<Uri>(Json value) => value._array.ConvertAll(x => (Uri)x);
        public static implicit operator Guid(Json value) => value.Guid;
        public static implicit operator Guid[](Json value) => value._array.ConvertAll(x => (Guid)x).ToArray();
        public static implicit operator List<Guid>(Json value) => value._array.ConvertAll(x => (Guid)x);
        public static implicit operator DateTime(Json value) => value.DateTime;
        public static implicit operator DateTime[](Json value) => value._array.ConvertAll(x => (DateTime)x).ToArray();
        public static implicit operator List<DateTime>(Json value) => value._array.ConvertAll(x => (DateTime)x);
        public static implicit operator DateTimeOffset(Json value) => value.DateTimeOffset;
        public static implicit operator DateTimeOffset[](Json value) => value._array.ConvertAll(x => (DateTimeOffset)x).ToArray();
        public static implicit operator List<DateTimeOffset>(Json value) => value._array.ConvertAll(x => (DateTimeOffset)x);
        public static implicit operator TimeSpan(Json value) => value.TimeSpan;
        public static implicit operator TimeSpan[](Json value) => value._array.ConvertAll(x => (TimeSpan)x).ToArray();
        public static implicit operator List<TimeSpan>(Json value) => value._array.ConvertAll(x => (TimeSpan)x);
        public static implicit operator Color(Json value) => value.Color;
        public static implicit operator Color[](Json value) => value._array.ConvertAll(x => (Color)x).ToArray();
        public static implicit operator List<Color>(Json value) => value._array.ConvertAll(x => (Color)x);
        public static implicit operator Color32(Json value) => value.Color32;
        public static implicit operator Color32[](Json value) => value._array.ConvertAll(x => (Color32)x).ToArray();
        public static implicit operator List<Color32>(Json value) => value._array.ConvertAll(x => (Color32)x);
        public static implicit operator Vector2(Json value) => value.Vector2;
        public static implicit operator Vector2[](Json value) => value._array.ConvertAll(x => (Vector2)x).ToArray();
        public static implicit operator List<Vector2>(Json value) => value._array.ConvertAll(x => (Vector2)x);
        public static implicit operator Vector3(Json value) => value.Vector3;
        public static implicit operator Vector3[](Json value) => value._array.ConvertAll(x => (Vector3)x).ToArray();
        public static implicit operator List<Vector3>(Json value) => value._array.ConvertAll(x => (Vector3)x);
        public static implicit operator Vector4(Json value) => value.Vector4;
        public static implicit operator Vector4[](Json value) => value._array.ConvertAll(x => (Vector4)x).ToArray();
        public static implicit operator List<Vector4>(Json value) => value._array.ConvertAll(x => (Vector4)x);
        public static implicit operator Quaternion(Json value) => value.Quaternion;
        public static implicit operator Quaternion[](Json value) => value._array.ConvertAll(x => (Quaternion)x).ToArray();
        public static implicit operator List<Quaternion>(Json value) => value._array.ConvertAll(x => (Quaternion)x);
        public static implicit operator Bounds(Json value) => value.Bounds;
        public static implicit operator Bounds[](Json value) => value._array.ConvertAll(x => (Bounds)x).ToArray();
        public static implicit operator List<Bounds>(Json value) => value._array.ConvertAll(x => (Bounds)x);
        public static implicit operator Rect(Json value) => value.Rect;
        public static implicit operator Rect[](Json value) => value._array.ConvertAll(x => (Rect)x).ToArray();
        public static implicit operator List<Rect>(Json value) => value._array.ConvertAll(x => (Rect)x);
        
        #endregion

        #region Serialization
        public string Serialize()
        {
            return JsonSerialization.Serialize(this);
        }

        public static Json Deserialize(string json)
        {
            return JsonSerialization.Deserialize(json);
        }

        public void Write(StringWriter writer, bool includeTypeInfo = false)
        {
            if (includeTypeInfo)
            {
                writer.Write("{");
                writer.Write($"\"JsonType\": \"{JsonType}\", \"DataType\": \"{DataType}\", \"Value\": ");
            }
            switch (JsonType)
            {
                case JsonTypes.Undefined:
                case JsonTypes.Null:
                    writer.Write("null");
                    break;
                case JsonTypes.String:
                    writer.Write("\"");
                    writer.Write(_string);
                    writer.Write("\"");
                    break;
                case JsonTypes.Boolean:
                    writer.Write(_bool ? "true" : "false");
                    break;
                case JsonTypes.Number:
                    writer.Write(_number);
                    break;
                case JsonTypes.Array:
                    writer.Write("[");
                    int arrayIndex = 0;
                    int arrayCount = _array.Count - 1;
                    foreach (Json item in _array)
                    {
                        item.Write(writer, includeTypeInfo);
                        if (arrayIndex < arrayCount) writer.Write(", ");
                        arrayIndex++;
                    }
                    writer.Write("]");
                    break;
                case JsonTypes.Object:
                    writer.Write("{");
                    if (DataType == DataTypes.Container)
                    {
                        int containerIndex = 0;
                        int containerCount = _container.Count - 1;
                        foreach (KeyValuePair<string, Json> kvp in _container)
                        {
                            writer.Write($"\"{kvp.Key}\": ");
                            kvp.Value.Write(writer, includeTypeInfo);
                            if (containerIndex < containerCount) writer.Write(", ");
                            containerIndex++;
                        }
                    }
                    else
                    {
                        int containerIndex = 0;
                        int containerCount = _container.Count - 1;
                        foreach (KeyValuePair<string, Json> kvp in _container)
                        {
                            writer.Write($"\"{kvp.Key}\": {kvp.Value}");
                            if (containerIndex < containerCount) writer.Write(", ");
                            containerIndex++;
                        }
                    }
                    writer.Write("}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (includeTypeInfo)
            {
                writer.Write("}");
            }
        }

        public static Json FromSerialization(Json jsonType, Json dataType, Json value)
        {
            value.JsonType = (JsonTypes) Enum.Parse(typeof(JsonTypes), jsonType);
            value.DataType = (DataTypes) Enum.Parse(typeof(DataTypes), dataType);
            return value;
        }
        
        #endregion
    }
}