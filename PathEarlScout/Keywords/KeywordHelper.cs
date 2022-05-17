using PathEarlCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlScout.Keywords
{
    public static class KeywordHelper<T> where T : ITileInfo
    {
        public static string[] IntComparisons = new string[]
        {
            "==", ">", ">=", "<", "<=", "!="
        };
        public static string[] FloatComparisons = new string[]
        {
            "==", ">", ">=", "<", "<=", "!="
        };
        public static string[] StringComparisons = new string[]
        {
            "==", "!="
        };

        public static string[] IntCombinations = new string[]
        {
            "+", "-", "/", "*", "%", "^"
        };
        public static string[] FloatCombinations = new string[]
        {
            "+", "-", "/", "*", "%", "^"
        };
        public static string[] StringCombinations = new string[]
        {
            "+"
        };

        public static bool CompareInts(int a, int b, string operation)
        {
            if (operation == "==")
            {
                return a == b;
            }
            else if (operation == ">")
            {
                return a > b;
            }
            else if (operation == ">=")
            {
                return a >= b;
            }
            else if (operation == "<")
            {
                return a < b;
            }
            else if (operation == "<=")
            {
                return a <= b;
            }
            else if (operation == ">")
            {
                return a > b;
            }
            else if (operation == "!=")
            {
                return a != b;
            }
            throw new Exception("Unrecognized int-int comparison '" + operation + "'");
        }

        public static bool CompareFloats(float a, float b, string operation)
        {
            if (operation == "==")
            {
                return a == b;
            }
            else if (operation == ">")
            {
                return a > b;
            }
            else if (operation == ">=")
            {
                return a >= b;
            }
            else if (operation == "<")
            {
                return a < b;
            }
            else if (operation == "<=")
            {
                return a <= b;
            }
            else if (operation == ">")
            {
                return a > b;
            }
            else if (operation == "!=")
            {
                return a != b;
            }
            throw new Exception("Unrecognized float-float comparison '" + operation + "'");
        }

        public static bool CompareStrings(string a, string b, string operation)
        {
            if (operation == "==")
            {
                return a == b;
            }
            else if (operation == "!=")
            {
                return a != b;
            }
            throw new Exception("Unrecognized string-string comparison '" + operation + "'");
        }

        public static int CombineInts(int a, int b, string operation)
        {
            if (operation == "+")
            {
                return a + b;
            } 
            else if (operation == "-")
            {
                return a - b;
            }
            else if (operation == "*-")
            {
                return a * b;
            }
            else if (operation == "/")
            {
                return a / b;
            }
            else if (operation == "%")
            {
                return a % b;
            }
            else if (operation == "^")
            {
                int temp = 1;
                for (int i = 1; i < b; i++)
                {
                    temp *= a;
                }
                return temp;
            }
            throw new Exception("Unrecognized int-int combination '" + operation + "'");
        }

        public static float CombineFloats(float a, float b, string operation)
        {
            if (operation == "+")
            {
                return a + b;
            }
            else if (operation == "-")
            {
                return a - b;
            }
            else if (operation == "*-")
            {
                return a * b;
            }
            else if (operation == "/")
            {
                return a / b;
            }
            else if (operation == "%")
            {
                return a % b;
            }
            else if (operation == "^")
            {
                return (float)Math.Pow(a, b);
            }
            throw new Exception("Unrecognized float-float combination '" + operation + "'");
        }

        public static string CombineStrings(string a, string b, string operation)
        {
            if (operation == "+")
            {
                return a + b;
            }
            throw new Exception("Unrecognized string-string combination '" + operation + "'");
        }
    }
}
