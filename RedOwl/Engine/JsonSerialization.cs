using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace RedOwl.Engine
{
    public static class JsonSerialization
    {
        #region Reader
        
        private static StringReader _reader;

        private const string WhiteSpace = " \t\n\r";
        private const string WordBreak = " \t\n\r{}[],:\"";

        private enum TOKEN
        {
            NONE,
            CURLY_OPEN,
            CURLY_CLOSE,
            SQUARED_OPEN,
            SQUARED_CLOSE,
            COLON,
            COMMA,
            STRING,
            NUMBER,
            TRUE,
            FALSE,
            NULL
        };
        
        private static char PeekChar => Convert.ToChar(_reader.Peek());

        private static char NextChar => Convert.ToChar(_reader.Read());

        private static string NextWord
        {
            get {
                StringBuilder word = new StringBuilder();

                while (WordBreak.IndexOf(PeekChar) == -1) {
                    word.Append(NextChar);

                    if (_reader.Peek() == -1) {
                        break;
                    }
                }

                return word.ToString();
            }
        }

        private static void EatWhitespace()
        {
            while (WhiteSpace.IndexOf(PeekChar) != -1) {
                _reader.Read();

                if (_reader.Peek() == -1) {
                    break;
                }
            }
        }

        private static TOKEN NextToken
        {
            get {
                EatWhitespace();

                if (_reader.Peek() == -1) {
                    return TOKEN.NONE;
                }

                char c = PeekChar;
                switch (c) {
                    case '{':
                        return TOKEN.CURLY_OPEN;
                    case '}':
                        _reader.Read();
                        return TOKEN.CURLY_CLOSE;
                    case '[':
                        return TOKEN.SQUARED_OPEN;
                    case ']':
                        _reader.Read();
                        return TOKEN.SQUARED_CLOSE;
                    case ',':
                        _reader.Read();
                        return TOKEN.COMMA;
                    case '"':
                        return TOKEN.STRING;
                    case ':':
                        return TOKEN.COLON;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        return TOKEN.NUMBER;
                }

                string word = NextWord;

                switch (word) {
                    case "false":
                        return TOKEN.FALSE;
                    case "true":
                        return TOKEN.TRUE;
                    case "null":
                        return TOKEN.NULL;
                }

                return TOKEN.NONE;
            }
        }
        
        private static Json ParseValue()
        {
            return ParseByToken(NextToken);
        }
        
        private static Json ParseByToken(TOKEN token)
        {
            switch (token) {
                case TOKEN.STRING:
                    return ParseString();
                case TOKEN.NUMBER:
                    return ParseNumber();
                case TOKEN.CURLY_OPEN:
                    // ditch opening brace
                    _reader.Read();
                    return ParseObject();
                case TOKEN.SQUARED_OPEN:
                    // ditch opening bracket
                    _reader.Read();
                    return ParseArray();
                case TOKEN.TRUE:
                    return true;
                case TOKEN.FALSE:
                    return false;
                case TOKEN.NULL:
                    return Json.Null;
                default:
                    return Json.Null;
            }
        }
        
        private static string ParseString()
        {
            StringBuilder s = new StringBuilder();
            
            // ditch opening quote
            _reader.Read();
            
            bool parsing = true;
            while (parsing) {

                if (_reader.Peek() == -1) {
                    parsing = false;
                    break;
                }

                char c = NextChar;
                switch (c) {
                case '"':
                    parsing = false;
                    break;
                case '\\':
                    if (_reader.Peek() == -1) {
                        parsing = false;
                        break;
                    }

                    c = NextChar;
                    switch (c) {
                        case '"':
                        case '\\':
                        case '/':
                            s.Append(c);
                            break;
                        case 'b':
                            s.Append('\b');
                            break;
                        case 'f':
                            s.Append('\f');
                            break;
                        case 'n':
                            s.Append('\n');
                            break;
                        case 'r':
                            s.Append('\r');
                            break;
                        case 't':
                            s.Append('\t');
                            break;
                        case 'u':
                            StringBuilder hex = new StringBuilder();

                            for (int i=0; i< 4; i++) {
                                hex.Append(NextChar);
                            }

                            s.Append((char) Convert.ToInt32(hex.ToString(), 16));
                            break;
                    }
                    break;
                default:
                    s.Append(c);
                    break;
                }
            }

            return s.ToString();
        }
        
        private static double ParseNumber()
        {
            string number = NextWord;
            double.TryParse(number, out double parsedDouble);
            return parsedDouble;
        }
        
        private static Json ParseArray()
        {
            List<Json> array = new List<Json>();

            bool parsing = true;
            while (parsing) {
                TOKEN nextToken = NextToken;

                switch (nextToken) {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.SQUARED_CLOSE:
                        parsing = false;
                        break;
                    default:
                        array.Add(ParseByToken(nextToken));
                        break;
                }
            }

            return new Json(array);
        }
        
        private static Json ParseObject()
        {
            Dictionary<string, Json> data = new Dictionary<string, Json>();

            while (true) {
                switch (NextToken) {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.CURLY_CLOSE when data.ContainsKey("JsonType") && data.ContainsKey("DataType"):
                        return Json.FromSerialization(data["JsonType"], data["DataType"], data["Value"]);
                    case TOKEN.CURLY_CLOSE:
                        return new Json(data);
                    default:
                        // key
                        string key = ParseString();
                        if (key == null) {
                            return null;
                        }
                        // :
                        if (NextToken != TOKEN.COLON) {
                            return null;
                        }
                        // ditch the colon
                        _reader.Read();

                        // value
                        data[key] = ParseValue();
                        break;
                }
            }
        }

        #endregion

        public static string Serialize(Json data)
        {
            StringWriter writer = new StringWriter();
            data.Write(writer, true);
            return writer.ToString();
        }
        
        public static Json Deserialize(string json)
        {
            _reader = new StringReader(json);
            return ParseValue();
        }

    }
}