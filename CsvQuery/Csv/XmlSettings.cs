namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml;

    public class XmlSettings : CsvSettings
    {
        private readonly string _lineElement;

        protected class ElementFacts
        {
            public string Name;
            public int Count;
            public List<string> SubElements = new List<string>();
        }

        public static bool TryAnalyze(string text, out XmlSettings result)
        {
            var elems = new Dictionary<string, ElementFacts>(StringComparer.OrdinalIgnoreCase);
            using (var stringReader = new StringReader(text))
            using (var reader = XmlReader.Create(stringReader))
            {
                var parents = new Stack<string>();
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Depth != 0)
                                {
                                    var siblings = elems[parents.Peek()].SubElements;
                                    if (!siblings.Contains(reader.LocalName))
                                        siblings.Add(reader.LocalName);
                                }
                                if (!elems.TryGetValue(reader.LocalName, out var elem))
                                {
                                    elem = new ElementFacts { Name = reader.LocalName };
                                    elems.Add(elem.Name, elem);
                                }

                                elem.Count++;
                                if (!reader.IsEmptyElement)
                                {
                                    parents.Push(reader.LocalName);
                                }
                                break;
                            case XmlNodeType.Text:
                                break;
                            case XmlNodeType.EndElement:
                                parents.Pop();
                                break;
                        }
                    }
                }
                catch (XmlException e)
                {
                    // Stop reading :/
                }
            }

            var bestCandidate = elems.Values.Where(e =>e.Count>1 && e.SubElements.Count > 1).OrderByDescending(e => e.Count)
                .FirstOrDefault();
            if (bestCandidate == null)
            {
                result = null;
                return false;
            }
            Debug.WriteLine($"Found line type {bestCandidate.Name} {bestCandidate.Count} lines)");
            Debug.WriteLine("Found columns: " + string.Join(", ", bestCandidate.SubElements));

            result = new XmlSettings(bestCandidate.Name, bestCandidate.SubElements.ToArray());
            return true;
        }

        public override IEnumerable<string[]> Parse(TextReader textReader)
        {
            using (var reader = XmlReader.Create(textReader))
            {
                var parents = new Stack<string>();
                string[] values=null;
                int inColumn = -1, lineDepth=-1;
              
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.LocalName == this._lineElement && values==null&& !reader.IsEmptyElement)
                                {
                                    values = new string[this.FieldNames.Length];
                                    lineDepth = reader.Depth;
                                }else if (values != null && inColumn == -1)
                                {
                                    inColumn = Array.FindIndex(FieldNames,
                                        x => x.Equals(reader.LocalName, StringComparison.OrdinalIgnoreCase));
                                }
                                if (!reader.IsEmptyElement)
                                {
                                    parents.Push(reader.LocalName);
                                }
                                break;
                            case XmlNodeType.Text:
                                if (inColumn != -1)
                                {
                                    if (values[inColumn] == null)
                                        values[inColumn] = reader.Value;
                                    else values[inColumn] += reader.Value;
                                }
                                break;
                            case XmlNodeType.EndElement:
                                if (reader.LocalName == this._lineElement && reader.Depth == lineDepth)
                                {
                                    yield return values;
                                    values = null;
                                }else if (reader.Depth == lineDepth + 1)
                                {
                                    inColumn = -1;
                                }
                                parents.Pop();
                                break;
                        }
                    }
            }
        }

        public XmlSettings(string lineElement, string[] fieldNames)
        {
            this._lineElement = lineElement;
            this.FieldNames = fieldNames;
            this.Separator = '<';
        }
    }
}