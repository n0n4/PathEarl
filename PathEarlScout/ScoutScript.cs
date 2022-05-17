using PathEarlCore;
using PathEarlScout.Keywords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathEarlScout
{
    public static class ScoutScript<T> where T : ITileInfo
    {
        public static void SaveCheck(ScoutSerializer<T> serializer, ScoutCheck<T> check)
        {
            if (check.FirstKeywordFloat != null)
                SaveKeywordFloat(serializer, check.FirstKeywordFloat);
            else if (check.FirstKeywordInt != null)
                SaveKeywordInt(serializer, check.FirstKeywordInt);
            else if (check.FirstKeywordString != null)
                SaveKeywordString(serializer, check.FirstKeywordString);
            else
                throw new Exception("Expected first keyword in conditional");

            serializer.Write(" ");
            serializer.Write(check.Operation);
            serializer.Write(" ");

            if (check.SecondKeywordFloat != null)
                SaveKeywordFloat(serializer, check.SecondKeywordFloat);
            else if (check.SecondKeywordInt != null)
                SaveKeywordInt(serializer, check.SecondKeywordInt);
            else if (check.SecondKeywordString != null)
                SaveKeywordString(serializer, check.SecondKeywordString);
            else
                throw new Exception("Expected second keyword in conditional");
        }

        public static int LoadCheck(ScoutSerializer<T> serializer, string line, int pos, ScoutCheck<T> check)
        {
            pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> firstReturn);
            pos = ParseHelper.SkipSpaces(line, pos);
            check.Operation = ParseHelper.ReadToken(line, ' ', pos);
            pos += check.Operation.Length;
            pos = ParseHelper.SkipSpaces(line, pos);
            pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> secondReturn);

            check.FirstKeywordFloat = firstReturn.KeywordFloat;
            check.FirstKeywordString = firstReturn.KeywordString;
            check.FirstKeywordInt = firstReturn.KeywordInt;

            check.SecondKeywordFloat = secondReturn.KeywordFloat;
            check.SecondKeywordString = secondReturn.KeywordString;
            check.SecondKeywordInt = secondReturn.KeywordInt;

            return pos;
        }

        public static void SaveKeywordFloat(ScoutSerializer<T> serializer, KeywordFloat<T> keyword)
        {
            if (string.IsNullOrEmpty(keyword.KeywordOwner))
            {
                if (string.IsNullOrEmpty(keyword.Keyword))
                {
                    serializer.Write(keyword.Literal.ToString());
                    serializer.Write("f");
                }
                else
                {
                    serializer.Write(keyword.Keyword);
                }
            }
            else
            {
                serializer.Write(keyword.KeywordOwner);
                serializer.Write(".");
                serializer.Write(keyword.Keyword);
            }

            if (keyword.HasNext)
            {
                serializer.Write(" ");
                serializer.Write(keyword.NextOperation);
                serializer.Write(" ");

                if (keyword.NextFloat != null)
                    SaveKeywordFloat(serializer, keyword.NextFloat);
                else if (keyword.NextInt != null)
                    SaveKeywordInt(serializer, keyword.NextInt);
                else if (keyword.NextString != null)
                    SaveKeywordString(serializer, keyword.NextString);
                else
                    throw new Exception("Expected next keyword after " + keyword.ToString());
            }
        }

        public static void SaveKeywordInt(ScoutSerializer<T> serializer, KeywordInt<T> keyword)
        {
            if (string.IsNullOrEmpty(keyword.KeywordOwner))
            {
                if (string.IsNullOrEmpty(keyword.Keyword))
                {
                    serializer.Write(keyword.Literal.ToString());
                }
                else
                {
                    serializer.Write(keyword.Keyword);
                }
            }
            else
            {
                serializer.Write(keyword.KeywordOwner);
                serializer.Write(".");
                serializer.Write(keyword.Keyword);
            }

            if (keyword.HasNext)
            {
                serializer.Write(" ");
                serializer.Write(keyword.NextOperation);
                serializer.Write(" ");

                if (keyword.NextFloat != null)
                    SaveKeywordFloat(serializer, keyword.NextFloat);
                else if (keyword.NextInt != null)
                    SaveKeywordInt(serializer, keyword.NextInt);
                else if (keyword.NextString != null)
                    SaveKeywordString(serializer, keyword.NextString);
                else
                    throw new Exception("Expected next keyword after " + keyword.ToString());
            }
        }

        public static void SaveKeywordString(ScoutSerializer<T> serializer, KeywordString<T> keyword)
        {
            if (string.IsNullOrEmpty(keyword.KeywordOwner))
            {
                if (string.IsNullOrEmpty(keyword.Keyword))
                {
                    serializer.Write("\"");
                    serializer.Write(keyword.Literal);
                    serializer.Write("\"");
                }
                else
                {

                    serializer.Write(keyword.Keyword);
                }
            }
            else
            {
                serializer.Write(keyword.KeywordOwner);
                serializer.Write(".");
                serializer.Write(keyword.Keyword);
            }

            if (keyword.HasNext)
            {
                serializer.Write(" ");
                serializer.Write(keyword.NextOperation);
                serializer.Write(" ");

                if (keyword.NextFloat != null)
                    SaveKeywordFloat(serializer, keyword.NextFloat);
                else if (keyword.NextInt != null)
                    SaveKeywordInt(serializer, keyword.NextInt);
                else if (keyword.NextString != null)
                    SaveKeywordString(serializer, keyword.NextString);
                else
                    throw new Exception("Expected next keyword after " + keyword.ToString());
            }
        }

        public static int LoadKeyword(ScoutSerializer<T> serializer, string line, int pos, out KeywordReturn<T> keywordReturn)
        {
            string full = ParseHelper.ReadTokenIgnoreQuotes(line, ' ', pos);
            pos += full.Length;

            string owner = null;
            string keyword = null;
            int dotPos = full.IndexOf('.');
            if (dotPos != -1)
            {
                owner = ParseHelper.ReadToken(full, '.', 0);
                keyword = full.Substring(dotPos + 1);
            }
            else
            {
                keyword = full;
            }

            keywordReturn = new KeywordReturn<T>();
            InfoAccess<T> acc = serializer.Scout.InfoAccess;
            if (acc.GetFloats.TryGetValue(keyword, out Func<T, float> floatFunc))
            {
                KeywordFloat<T> key = serializer.Scout.Recycler.KeywordFloatPool.Request();
                key.Accessor = floatFunc;
                key.Keyword = keyword;
                key.KeywordOwner = owner;
                key.HasNext = false;
                keywordReturn.KeywordFloat = key;

                // check for a subsequent keyword
                if (line.Length > pos)
                {
                    pos = ParseHelper.SkipSpaces(line, pos);
                    string operation = ParseHelper.ReadToken(line, ' ', pos);
                    if (KeywordHelper<T>.FloatCombinations.Contains(operation))
                    {
                        key.HasNext = true;
                        key.NextOperation = operation;
                        pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                        key.NextFloat = nextReturn.KeywordFloat;
                        key.NextInt = nextReturn.KeywordInt;
                        key.NextString = nextReturn.KeywordString;
                    }
                }
            } 
            else if (acc.GetInts.TryGetValue(keyword, out Func<T, int> intFunc))
            {
                KeywordInt<T> key = serializer.Scout.Recycler.KeywordIntPool.Request();
                key.Accessor = intFunc;
                key.Keyword = keyword;
                key.KeywordOwner = owner;
                key.HasNext = false;
                keywordReturn.KeywordInt = key;

                // check for a subsequent keyword
                if (line.Length > pos)
                {
                    pos = ParseHelper.SkipSpaces(line, pos);
                    string operation = ParseHelper.ReadToken(line, ' ', pos);
                    if (KeywordHelper<T>.IntCombinations.Contains(operation))
                    {
                        key.HasNext = true;
                        key.NextOperation = operation;
                        pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                        key.NextFloat = nextReturn.KeywordFloat;
                        key.NextInt = nextReturn.KeywordInt;
                        key.NextString = nextReturn.KeywordString;
                    }
                }
            }
            else if (acc.GetStrings.TryGetValue(keyword, out Func<T, string> stringFunc))
            {
                KeywordString<T> key = serializer.Scout.Recycler.KeywordStringPool.Request();
                key.Accessor = stringFunc;
                key.Keyword = keyword;
                key.KeywordOwner = owner;
                key.HasNext = false;
                keywordReturn.KeywordString = key;

                // check for a subsequent keyword
                if (line.Length > pos)
                {
                    pos = ParseHelper.SkipSpaces(line, pos);
                    string operation = ParseHelper.ReadToken(line, ' ', pos);
                    if (KeywordHelper<T>.StringCombinations.Contains(operation))
                    {
                        key.HasNext = true;
                        key.NextOperation = operation;
                        pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                        key.NextFloat = nextReturn.KeywordFloat;
                        key.NextInt = nextReturn.KeywordInt;
                        key.NextString = nextReturn.KeywordString;
                    }
                }
            }
            else
            {
                // must try parsing as a literal
                if (keyword.StartsWith("\""))
                {
                    // string literal
                    KeywordString<T> key = serializer.Scout.Recycler.KeywordStringPool.Request();
                    key.KeywordOwner = null;
                    key.Accessor = null;
                    key.Keyword = null;
                    key.Literal = keyword.Substring(1, keyword.Length - 2);
                    key.HasNext = false;
                    keywordReturn.KeywordString = key;

                    // check for a subsequent keyword
                    if (line.Length > pos)
                    {
                        pos = ParseHelper.SkipSpaces(line, pos);
                        string operation = ParseHelper.ReadToken(line, ' ', pos);
                        if (KeywordHelper<T>.StringCombinations.Contains(operation))
                        {
                            key.HasNext = true;
                            key.NextOperation = operation;
                            pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                            key.NextFloat = nextReturn.KeywordFloat;
                            key.NextInt = nextReturn.KeywordInt;
                            key.NextString = nextReturn.KeywordString;
                        }
                    }
                }
                else if (keyword.EndsWith("f"))
                {
                    // float literal
                    KeywordFloat<T> key = serializer.Scout.Recycler.KeywordFloatPool.Request();
                    key.KeywordOwner = null;
                    key.Accessor = null;
                    key.Keyword = null;
                    if (!float.TryParse(keyword, out float keyValue))
                    {
                        throw new Exception("Could not parse '" + keyword + "' as float");
                    }
                    key.Literal = keyValue;
                    key.HasNext = false;
                    keywordReturn.KeywordFloat = key;

                    // check for a subsequent keyword
                    if (line.Length > pos)
                    {
                        pos = ParseHelper.SkipSpaces(line, pos);
                        string operation = ParseHelper.ReadToken(line, ' ', pos);
                        if (KeywordHelper<T>.FloatCombinations.Contains(operation))
                        {
                            key.HasNext = true;
                            key.NextOperation = operation;
                            pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                            key.NextFloat = nextReturn.KeywordFloat;
                            key.NextInt = nextReturn.KeywordInt;
                            key.NextString = nextReturn.KeywordString;
                        }
                    }
                }
                else
                {
                    // int literal
                    KeywordInt<T> key = serializer.Scout.Recycler.KeywordIntPool.Request();
                    key.KeywordOwner = null;
                    key.Accessor = null;
                    key.Keyword = null;
                    if (!int.TryParse(keyword, out int keyValue))
                    {
                        throw new Exception("Could not parse '" + keyword + "' as int");
                    }
                    key.Literal = keyValue;
                    key.HasNext = false;
                    keywordReturn.KeywordInt = key;

                    // check for a subsequent keyword
                    if (line.Length > pos)
                    {
                        pos = ParseHelper.SkipSpaces(line, pos);
                        string operation = ParseHelper.ReadToken(line, ' ', pos);
                        if (KeywordHelper<T>.IntCombinations.Contains(operation))
                        {
                            key.HasNext = true;
                            key.NextOperation = operation;
                            pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                            key.NextFloat = nextReturn.KeywordFloat;
                            key.NextInt = nextReturn.KeywordInt;
                            key.NextString = nextReturn.KeywordString;
                        }
                    }
                }
            }

            return pos;
        }
    }
}
