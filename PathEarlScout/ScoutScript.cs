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
            if (keyword.KeywordOwner == null)
            {
                if (keyword.Keyword == null)
                {
                    serializer.Write(keyword.Literal.ToString());
                    serializer.Write("f");
                }
                else
                {
                    if (string.IsNullOrEmpty(keyword.Keyword.Literal) || keyword.Keyword.HasNext)
                    {
                        serializer.Write("<float:");
                        SaveKeywordString(serializer, keyword.Keyword);
                        serializer.Write(">");
                    } 
                    else
                    {
                        SaveKeywordString(serializer, keyword.Keyword, true);
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(keyword.KeywordOwner.Literal) || keyword.KeywordOwner.HasNext)
                {
                    serializer.Write("<float:");
                    SaveKeywordString(serializer, keyword.KeywordOwner);
                    serializer.Write(">");
                }
                else
                {
                    SaveKeywordString(serializer, keyword.KeywordOwner, true);
                }

                serializer.Write(".");

                if (string.IsNullOrEmpty(keyword.Keyword.Literal) || keyword.Keyword.HasNext)
                {
                    serializer.Write("<float:");
                    SaveKeywordString(serializer, keyword.Keyword);
                    serializer.Write(">");
                }
                else
                {
                    SaveKeywordString(serializer, keyword.Keyword, true);
                }
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
            if (keyword.KeywordOwner == null)
            {
                if (keyword.Keyword == null)
                {
                    serializer.Write(keyword.Literal.ToString());
                }
                else
                {
                    if (string.IsNullOrEmpty(keyword.Keyword.Literal) || keyword.Keyword.HasNext)
                    {
                        serializer.Write("<int:");
                        SaveKeywordString(serializer, keyword.Keyword);
                        serializer.Write(">");
                    }
                    else
                    {
                        SaveKeywordString(serializer, keyword.Keyword, true);
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(keyword.KeywordOwner.Literal) || keyword.KeywordOwner.HasNext)
                {
                    serializer.Write("<int:");
                    SaveKeywordString(serializer, keyword.KeywordOwner);
                    serializer.Write(">");
                }
                else
                {
                    SaveKeywordString(serializer, keyword.KeywordOwner, true);
                }

                serializer.Write(".");

                if (string.IsNullOrEmpty(keyword.Keyword.Literal) || keyword.Keyword.HasNext)
                {
                    serializer.Write("<int:");
                    SaveKeywordString(serializer, keyword.Keyword);
                    serializer.Write(">");
                }
                else
                {
                    SaveKeywordString(serializer, keyword.Keyword, true);
                }
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

        public static void SaveKeywordString(ScoutSerializer<T> serializer, KeywordString<T> keyword, bool noQuotes = false)
        {
            if (keyword.KeywordOwner == null)
            {
                if (keyword.Keyword == null)
                {
                    if (!noQuotes)
                        serializer.Write("\"");
                    serializer.Write(keyword.Literal);
                    if (!noQuotes)
                        serializer.Write("\"");
                }
                else
                {
                    if (string.IsNullOrEmpty(keyword.Keyword.Literal) || keyword.Keyword.HasNext)
                    {
                        serializer.Write("<string:");
                        SaveKeywordString(serializer, keyword.Keyword);
                        serializer.Write(">");
                    }
                    else
                    {
                        SaveKeywordString(serializer, keyword.Keyword, true);
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(keyword.KeywordOwner.Literal) || keyword.KeywordOwner.HasNext)
                {
                    serializer.Write("<string:");
                    SaveKeywordString(serializer, keyword.KeywordOwner);
                    serializer.Write(">");
                }
                else
                {
                    SaveKeywordString(serializer, keyword.KeywordOwner, true);
                }

                serializer.Write(".");

                if (string.IsNullOrEmpty(keyword.Keyword.Literal) || keyword.Keyword.HasNext)
                {
                    serializer.Write("<string:");
                    SaveKeywordString(serializer, keyword.Keyword);
                    serializer.Write(">");
                }
                else
                {
                    SaveKeywordString(serializer, keyword.Keyword, true);
                }
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

        public static KeywordString<T> LoadKeywordName(ScoutSerializer<T> serializer, string line)
        {
            int pos = ParseHelper.SkipSpaces(line, 0);
            if (line[pos] == '<')
            {
                // load bracketed token
                int lastPos = ParseHelper.FindLastNonSpace(line);
                if (line[lastPos] != '>')
                    throw new Exception("Expected ending > on line " + line);
                string bracketed = line.Substring(pos + 1, lastPos - (1 + pos));

                // read the type prefix
                int colonPos = bracketed.IndexOf(':');
                if (colonPos == -1)
                    throw new Exception("Expected type prefix on line " + line);

                LoadKeyword(serializer, bracketed, colonPos + 1, out KeywordReturn<T> keywordReturn);
                if (keywordReturn.KeywordString == null)
                    throw new Exception("Failed to parse " + bracketed + " as keyword string");

                if (bracketed.StartsWith("int"))
                    keywordReturn.KeywordString.PrefixType = EKeywordType.Int;
                else if (bracketed.StartsWith("string"))
                    keywordReturn.KeywordString.PrefixType = EKeywordType.String;
                else if (bracketed.StartsWith("float"))
                    keywordReturn.KeywordString.PrefixType = EKeywordType.Float;
                else
                    throw new Exception("Unrecognized type prefix on line " + line);

                return keywordReturn.KeywordString;
            }
            else
            {
                // load as a literal
                KeywordString<T> key = serializer.Scout.Recycler.KeywordStringPool.Request();
                
                key.Literal = line;
                key.KeywordOwner = null;
                key.Accessor = null;
                key.Keyword = null;
                key.HasNext = false;

                return key;
            }
        }

        public static int LoadKeyword(ScoutSerializer<T> serializer, string line, int pos, out KeywordReturn<T> keywordReturn)
        {
            string full = ParseHelper.ReadTokenIgnoreBrackets(line, ' ', pos);
            pos += full.Length;

            string owner = null;
            string keyword = null;
            KeywordString<T> ownerKeyword = null;
            KeywordString<T> keywordKeyword = null;
            int dotPos = ParseHelper.IndexOfIgnoreBrackets(full, 0, '.');
            int firstPos = ParseHelper.SkipSpaces(full, 0);
            if (dotPos != -1 && !char.IsDigit(full[firstPos]) && full[firstPos] != '-' && full[firstPos] != '.')
            {
                owner = full.Substring(0, dotPos);
                keyword = full.Substring(dotPos + 1);

                ownerKeyword = LoadKeywordName(serializer, owner);
                keywordKeyword = LoadKeywordName(serializer, keyword);
            }
            else
            {
                keyword = full;

                keywordKeyword = LoadKeywordName(serializer, keyword);
            }

            keywordReturn = new KeywordReturn<T>();
            InfoAccess<T> acc = serializer.Scout.InfoAccess;
            if (keywordKeyword != null && keywordKeyword.PrefixType != EKeywordType.None)
            {
                // in this case, it's a bracketed dynamic lookup
                // we use the prefix to determine what kind of keyword we are constructing
                if (keywordKeyword.PrefixType == EKeywordType.Int)
                {
                    KeywordInt<T> key = serializer.Scout.Recycler.KeywordIntPool.Request();
                    key.Accessor = null;
                    key.Keyword = keywordKeyword;
                    key.KeywordOwner = ownerKeyword;
                    key.HasNext = false;
                    keywordReturn.KeywordInt = key;

                    // check for a subsequent keyword
                    pos = CheckIntHasNext(key, serializer, line, pos);
                }
                else if (keywordKeyword.PrefixType == EKeywordType.String)
                {
                    KeywordString<T> key = serializer.Scout.Recycler.KeywordStringPool.Request();
                    key.Accessor = null;
                    key.Keyword = keywordKeyword;
                    key.KeywordOwner = ownerKeyword;
                    key.HasNext = false;
                    keywordReturn.KeywordString = key;

                    // check for a subsequent keyword
                    pos = CheckStringHasNext(key, serializer, line, pos);
                }
                else if (keywordKeyword.PrefixType == EKeywordType.Float)
                {
                    KeywordFloat<T> key = serializer.Scout.Recycler.KeywordFloatPool.Request();
                    key.Accessor = null;
                    key.Keyword = keywordKeyword;
                    key.KeywordOwner = ownerKeyword;
                    key.HasNext = false;
                    keywordReturn.KeywordFloat = key;

                    // check for a subsequent keyword
                    pos = CheckFloatHasNext(key, serializer, line, pos);
                }
                else
                    throw new Exception("Unrecognized type prefix in keyword load");
            }
            else if (acc.TryGetFloatGet(owner, keyword, out Func<Tile<T>, float> floatFunc))
            {
                KeywordFloat<T> key = serializer.Scout.Recycler.KeywordFloatPool.Request();
                key.Accessor = floatFunc;
                key.Keyword = keywordKeyword;
                key.KeywordOwner = ownerKeyword;
                key.HasNext = false;
                keywordReturn.KeywordFloat = key;

                // check for a subsequent keyword
                pos = CheckFloatHasNext(key, serializer, line, pos);
            } 
            else if (acc.TryGetIntGet(owner, keyword, out Func<Tile<T>, int> intFunc))
            {
                KeywordInt<T> key = serializer.Scout.Recycler.KeywordIntPool.Request();
                key.Accessor = intFunc;
                key.Keyword = keywordKeyword;
                key.KeywordOwner = ownerKeyword;
                key.HasNext = false;
                keywordReturn.KeywordInt = key;

                // check for a subsequent keyword
                pos = CheckIntHasNext(key, serializer, line, pos);
            }
            else if (acc.TryGetStringGet(owner, keyword, out Func<Tile<T>, string> stringFunc))
            {
                KeywordString<T> key = serializer.Scout.Recycler.KeywordStringPool.Request();
                key.Accessor = stringFunc;
                key.Keyword = keywordKeyword;
                key.KeywordOwner = ownerKeyword;
                key.HasNext = false;
                keywordReturn.KeywordString = key;

                // check for a subsequent keyword
                pos = CheckStringHasNext(key, serializer, line, pos);
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
                    pos = CheckStringHasNext(key, serializer, line, pos);
                }
                else if (keyword.EndsWith("f"))
                {
                    // float literal
                    KeywordFloat<T> key = serializer.Scout.Recycler.KeywordFloatPool.Request();
                    key.KeywordOwner = null;
                    key.Accessor = null;
                    key.Keyword = null;
                    if (!float.TryParse(keyword.Substring(0, keyword.Length - 1), out float keyValue))
                    {
                        throw new Exception("Could not parse '" + keyword + "' as float");
                    }
                    key.Literal = keyValue;
                    key.HasNext = false;
                    keywordReturn.KeywordFloat = key;

                    // check for a subsequent keyword
                    pos = CheckFloatHasNext(key, serializer, line, pos);
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
                    pos = CheckIntHasNext(key, serializer, line, pos);
                }
            }

            return pos;
        }

        private static int CheckIntHasNext(KeywordInt<T> key, ScoutSerializer<T> serializer, string line, int pos)
        {
            if (line.Length > pos)
            {
                pos = ParseHelper.SkipSpaces(line, pos);
                string operation = ParseHelper.ReadToken(line, ' ', pos);
                if (KeywordHelper<T>.IntCombinations.Contains(operation))
                {
                    pos += operation.Length;
                    pos = ParseHelper.SkipSpaces(line, pos);

                    key.HasNext = true;
                    key.NextOperation = operation;
                    pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                    key.NextFloat = nextReturn.KeywordFloat;
                    key.NextInt = nextReturn.KeywordInt;
                    key.NextString = nextReturn.KeywordString;
                }
            }
            return pos;
        }

        private static int CheckStringHasNext(KeywordString<T> key, ScoutSerializer<T> serializer, string line, int pos)
        {
            if (line.Length > pos)
            {
                pos = ParseHelper.SkipSpaces(line, pos);
                string operation = ParseHelper.ReadToken(line, ' ', pos);
                if (KeywordHelper<T>.StringCombinations.Contains(operation))
                {
                    pos += operation.Length;
                    pos = ParseHelper.SkipSpaces(line, pos);

                    key.HasNext = true;
                    key.NextOperation = operation;
                    pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                    key.NextFloat = nextReturn.KeywordFloat;
                    key.NextInt = nextReturn.KeywordInt;
                    key.NextString = nextReturn.KeywordString;
                }
            }
            return pos;
        }

        private static int CheckFloatHasNext(KeywordFloat<T> key, ScoutSerializer<T> serializer, string line, int pos)
        {
            if (line.Length > pos)
            {
                pos = ParseHelper.SkipSpaces(line, pos);
                string operation = ParseHelper.ReadToken(line, ' ', pos);
                if (KeywordHelper<T>.FloatCombinations.Contains(operation))
                {
                    pos += operation.Length;
                    pos = ParseHelper.SkipSpaces(line, pos);

                    key.HasNext = true;
                    key.NextOperation = operation;
                    pos = LoadKeyword(serializer, line, pos, out KeywordReturn<T> nextReturn);
                    key.NextFloat = nextReturn.KeywordFloat;
                    key.NextInt = nextReturn.KeywordInt;
                    key.NextString = nextReturn.KeywordString;
                }
            }
            return pos;
        }
    }
}
