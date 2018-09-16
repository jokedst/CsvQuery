namespace CsvQuery.Tools
{

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Simple streaming JSON parser
    /// </summary>
    public class JsonParser
    {
        private readonly StringBuilder _sb = new StringBuilder(100, 100000);

        private void ReadToNewline(TextReader reader)
        {
            int ch;
            while ((ch = reader.Read()) != -1)
            {
                if (ch == '\n') break;
                if (ch == '\r')
                {
                    if (reader.Peek() == '\n')
                        reader.Read();
                    break;
                }
            }
        }

        private string ReadString(TextReader reader, char quoteChar = '"')
        {
            this._sb.Length = 0;
            int ch;
            var buffer = new char[4];
            while ((ch = reader.Read()) != -1)
            {
                var c = (char)ch;
                if (c == '\\')
                {
                    ch = reader.Read();
                    if (ch == -1) break;
                    c = (char)ch;
                    switch (c)
                    {
                        case 'b':
                            this._sb.Append('\b');
                            break;
                        case 'n':
                            this._sb.Append('\n');
                            break;
                        case 'r':
                            this._sb.Append('\r');
                            break;
                        case 't':
                            this._sb.Append('\t');
                            break;
                        case 'u':
                            var read = reader.ReadBlock(buffer, 0, 4);
                            if (read != 4) throw new JsonException("Unexpected EOF in unicode sequence");
                            // Fuck unicode
                            this._sb.Append((char)Convert.ToUInt32(new string(buffer), 16));
                            break;
                        default:
                            this._sb.Append(c);
                            break;
                    }
                }

                if (c == quoteChar) break;
                this._sb.Append(c);
            }

            return this._sb.ToString();
        }

        private decimal ReadNumber(TextReader reader, char alreadyReadChar)
        {
            const string allowed = "0123456789.eE";
            this._sb.Length = 0;
            this._sb.Append(alreadyReadChar);
            while (allowed.IndexOf((char)reader.Peek()) != -1) this._sb.Append((char)reader.Read());

            var numberString = this._sb.ToString();
            if (decimal.TryParse(numberString, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var result))
                return result;
            throw new JsonException($"Could not parse '{numberString}' as number");
        }

        private int ReadSkippingWhitespace(TextReader reader)
        {
            int ch;
            do
            {
                ch = reader.Read();
                if (ch == '/' && reader.Peek() == '/')
                {
                    do
                    {
                        ch = reader.Read();
                    } while (ch != -1 && ch != '\r' && ch != '\n');
                }

                if (ch == '/' && reader.Peek() == '*')
                {
                    reader.Read();
                    do
                    {
                        ch = reader.Read();
                    } while (ch != -1 && ch != '*' && reader.Peek() != '/');
                }
            } while (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n');

            return ch;
        }

        public IEnumerable<(JsonToken type, object value)> ParseTokens(TextReader reader)
        {
            int ch;
            var level = new Stack<JsonToken>();
            level.Push(JsonToken.None);

            while ((ch = this.ReadSkippingWhitespace(reader)) != -1)
            {
                var c = (char)ch;
                if (c == ' ' || c == '\t') continue;

                if (c == '/' && reader.Peek() == '/')
                    this.ReadToNewline(reader);

                switch (c)
                {
                    case '\'':
                    case '"':
                        yield return (JsonToken.String, this.ReadString(reader, c));
                        break;
                    case '[':
                        level.Push(JsonToken.StartArray);
                        yield return (JsonToken.StartArray, null);
                        break;
                    case '{':
                        level.Push(JsonToken.StartObject);
                        yield return (JsonToken.StartObject, null);
                        yield return (JsonToken.PropertyName, this.ReadPropertyName(reader));
                        break;
                    case ']':
                        if (level.Pop() != JsonToken.StartArray)
                            throw new JsonException("Unexpected end of array");
                        yield return (JsonToken.EndArray, null);
                        break;
                    case '}':
                        if (level.Pop() != JsonToken.StartObject)
                            throw new JsonException("Unexpected end of object");
                        yield return (JsonToken.EndObject, null);
                        break;
                    case ',':
                        if (level.Peek() == JsonToken.None)
                            throw new JsonException("Unexpected comma");
                        //yield return (JsonToken.Comma, null); // Maybe unneccesary?
                        if (level.Peek() == JsonToken.StartObject)
                            yield return (JsonToken.PropertyName, this.ReadPropertyName(reader));
                        break;
                    default:
                        if (c == '-' || c >= '0' && c <= '9')
                        {
                            var number = this.ReadNumber(reader, c);
                            yield return (JsonToken.Float, number);
                        }
                        else
                        {
                            // keywords: true, false, null
                            this._sb.Length = 0;
                            this._sb.Append(c);
                            while ("truefalsnTRUEFALSN".IndexOf((char)reader.Peek()) != -1)
                                this._sb.Append((char)reader.Read());

                            switch (this._sb.ToString().ToLower())
                            {
                                case "true":
                                    yield return (JsonToken.Boolean, true);
                                    break;
                                case "false":
                                    yield return (JsonToken.Boolean, false);
                                    break;
                                case "null":
                                    yield return (JsonToken.Null, null);
                                    break;
                                default:
                                    throw new JsonException($"Unexpected characters in JSON: '{this._sb}'");
                            }
                        }

                        break;
                }
            }

            if (level.Count != 1) yield return (JsonToken.UnexpectedEnd, null);
        }

        /// <summary>
        /// Parses JSON into Dictionary&lt;string,object&gt;, List&lt;object&gt;, bools, decimals and null:s
        /// </summary>
        public object Parse(TextReader reader)
        {
            var tokens = this.ParseTokens(reader);
            using (IEnumerator<(JsonToken type, object value)> tok = tokens.GetEnumerator())
            {
                if (!tok.MoveNext()) return null;
                return this.ParseInternal(tok);
            }
        }

        private object ParseInternal(IEnumerator<(JsonToken type, object value)> tok)
        {
            var token = tok.Current;
            switch (token.type)
            {
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean: return token.value;
                case JsonToken.Null: return null;
                case JsonToken.StartObject:
                    var jsonObject = new Dictionary<string, object>();
                    if (!tok.MoveNext()) throw new JsonUnexpectedEndException();
                    var nextProperty = tok.Current;
                    while (nextProperty.type == JsonToken.PropertyName)
                    {
                        if (!tok.MoveNext()) throw new JsonUnexpectedEndException();
                        var value = this.ParseInternal(tok);
                        jsonObject[(string)nextProperty.value] = value;
                        if (!tok.MoveNext()) throw new JsonUnexpectedEndException();
                        nextProperty = tok.Current;
                    }
                    if (nextProperty.type != JsonToken.EndObject) throw new JsonException($"Unexpected token {nextProperty.type} in object");
                    return jsonObject;
                case JsonToken.StartArray:
                    var jsonArray = new List<object>();
                    if (!tok.MoveNext()) throw new JsonUnexpectedEndException();
                    while (tok.Current.type != JsonToken.EndArray)
                    {
                        jsonArray.Add(this.ParseInternal(tok));
                        if (!tok.MoveNext()) throw new JsonUnexpectedEndException();
                    }

                    return jsonArray;
                case JsonToken.UnexpectedEnd:
                    throw new JsonUnexpectedEndException();
                default:
                    throw new JsonException($"Unexpected token '{token.type}'");
            }
        }

        private string ReadPropertyName(TextReader reader)
        {
            var ch = this.ReadSkippingWhitespace(reader);
            if (ch == '"' || ch == '\'')
            {
                var propertyName = this.ReadString(reader, (char)ch);
                var next = this.ReadSkippingWhitespace(reader);
                if (next != ':') throw new JsonException($"Unexpected character '{next}' after propertyname");
                return propertyName;
            }

            if (ch == '\'') return this.ReadString(reader, '\'');
            // technically not allowed with unquoted prop-names, but wtf
            if ((ch < 'a' || ch > 'z') && (ch < 'A' || ch > 'Z') && ch != '_' && ch != '$')
                throw new JsonException($"Unexpected character '{ch}' starting propertyname");

            this._sb.Length = 0;
            this._sb.Append((char)ch);
            const string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            while (allowed.IndexOf((char)reader.Peek()) != -1) this._sb.Append((char)reader.Read());

            var next2 = this.ReadSkippingWhitespace(reader);
            if (next2 != ':') throw new JsonException($"Unexpected character '{next2}' after propertyname");
            return this._sb.ToString();
        }

        public class JsonException : Exception
        {
            public JsonException(string message) : base(message) { }
        }

        public class JsonUnexpectedEndException : Exception
        {
            public JsonUnexpectedEndException() : base("Premature end") { }
        }
    }

    public enum JsonToken
    {
        None,
        StartObject,
        StartArray,
        PropertyName,
        Float,
        String,
        Boolean,
        Null,
        EndObject,
        EndArray,
        UnexpectedEnd
    }

}
