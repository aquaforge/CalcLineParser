using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace _05_SuperCalc
{
    public class CalcLineProcessor
    {
        #region Region_TOP
        static List<string> OPERATION_ELEMS_LIST = new List<string>() { "+", "-", "*", "/", "(", ")" };
        static List<string> INTERNAL_OPERATION_ELEMS_LIST = new List<string>() { "~" };
        static List<string> UNAR_OPERATION_ELEMS_LIST = new List<string>() { "~" };
        static char DECIMAL_SEPARATOR = Convert.ToChar(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
        static string NUMBER_PARTS = "1234567890" + DECIMAL_SEPARATOR.ToString();

        string textToParse = "";
        bool hasErrors = false;
        bool hasDivideByZero = false;
        bool isParsed = false;
        int positionElemsList = -1;

        double calculatedResult;

        public string TextToParse => textToParse;
        public bool HasErrors => hasErrors;
        public bool IsParsed => isParsed;
        public double ParsedResult => calculatedResult;
        public bool HasDivideByZero => hasDivideByZero;

        public int PositionElemsList => positionElemsList;
        List<string> elemsList = new List<string>();
        Stack<double> stackNumbers = new Stack<double>();
        Stack<string> stackOperations = new Stack<string>();

# if DEBUG
        private void ShowElemsList() => Console.WriteLine("elemsList: " + string.Join(", ", elemsList.ToArray()));
        private void ShowStackNumbers() => Console.WriteLine("stackNumbers: " + string.Join(", ", stackNumbers.ToArray().Reverse()));
        private void ShowStackOperations() => Console.WriteLine("stackOperations: " + string.Join(", ", stackOperations.ToArray().Reverse()));

        private void ShowAll(string txt)
        {
            Console.WriteLine();
            Console.WriteLine(txt);
            ShowElemsList();
            ShowStackNumbers();
            ShowStackOperations();
        }
#endif
        public string GetAnswer()
        {
            if (hasErrors)
            {
                string s = hasDivideByZero ? " (деление на ноль)" : "";
                return $" = ошибка в выражении{s}";
            }
            else
            {
                double exp = double.Parse(new DataTable().Compute(textToParse.Replace('~', '-'), null).ToString());
                return " = " + Math.Round(calculatedResult, 2).ToString() + (exp == calculatedResult ? "" : $" (exp={exp})");
            }
        }

        static int GetOperationPriority(string elem)
        {
            switch (elem)
            {
                case "(":
                case ")": return 4;
                case "~": return 3;
                case "*":
                case "/": return 2;
                case "+":
                case "-": return 1;
                default: return 0;
            }
        }
        #endregion

        #region Region_SplitLineToElems
        private void AddNumberIfExists(StringBuilder sb)
        {
            if (sb.Length != 0)
            {
                elemsList.Add(sb.ToString());
                sb.Clear();
            }
        }



        private void SplitLineToElems()
        {
            elemsList.Clear();
            var sb = new StringBuilder();

            for (int i = 0; i < textToParse.Length; i++)
            {
                char ch = textToParse[i];
                if (OPERATION_ELEMS_LIST.Contains(ch.ToString()))
                {
                    AddNumberIfExists(sb);
                    elemsList.Add(ch.ToString());
                }
                else if (NUMBER_PARTS.Contains(ch))
                {
                    sb.Append(ch);
                }
                else
                {
                    throw new ArgumentException($"Неизвестный символ [{ch}]");

                }
            }
            AddNumberIfExists(sb);
            //Обработка унарного минуса
            if (elemsList.Count > 0)
            {
                if (elemsList[0] == "-")
                    elemsList[0] = "~";
                for (int i = 0; i < elemsList.Count; i++)
                {
                    if (elemsList[i] == "(" && i + 1 < elemsList.Count)
                        if (elemsList[i + 1] == "-")
                            elemsList[i + 1] = "~";
                }
            }
        }
        #endregion

        #region ProcessAllElems
        private void DoUnaryStackOperation()
        {
            string elem = stackOperations.Pop();
            double d1 = stackNumbers.Pop();

            switch (elem)
            {
                case "~":
                    stackNumbers.Push(-d1);
                    break;
                default:
                    throw new ArgumentException($"Неизвестный унарный оператор [{elem}]");
            }
        }

        private void DoBinaryStackOperation()
        {
            string elem = stackOperations.Pop();
            double d2 = stackNumbers.Pop();
            double d1 = stackNumbers.Pop();

            switch (elem)
            {
                case "+":
                    stackNumbers.Push(d1 + d2);
                    break;
                case "-":
                    stackNumbers.Push(d1 - d2);
                    break;
                case "*":
                    stackNumbers.Push(d1 * d2);
                    break;
                case "/":
                    if (d2 == 0) throw new DivideByZeroException($"{d1}/{d2}");
                    stackNumbers.Push(d1 / d2);
                    break;
                default:
                    throw new ArgumentException($"Неизвестный оператор [{elem}]");
            }
        }


        private void DoStackOperation()
        {
            string elem = stackOperations.Peek();
            switch (elem)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                    DoBinaryStackOperation();
                    break;

                case "~":
                    DoUnaryStackOperation();
                    break;

                default:
                    throw new ArgumentException($"Неизвестный оператор [{elem}]");
            }
#if DEBUG
            ShowAll("DoOperation AFTER");
#endif
        }

        private void AddOneOperation(string elem)
        {

            while (stackOperations.Count > 0)
            {
                var prevElem = stackOperations.Peek();
                if (prevElem == "(") break;
                if (GetOperationPriority(prevElem) < GetOperationPriority(elem)) break;
                if (stackNumbers.Count < (UNAR_OPERATION_ELEMS_LIST.Contains(prevElem) ? 1 : 2)) break;
                DoStackOperation();
            }

            switch (elem)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "(":
                case "~":
                    stackOperations.Push(elem);
                    break;

                case ")":
                    while (stackOperations.Peek() != "(")
                        DoStackOperation();
                    if (stackOperations.Peek() == "(")
                        stackOperations.Pop();
                    else
                        throw new ArgumentException($"Ошибка в скобках");
                    break;

                default:
                    throw new ArgumentException($"Ошибочный оператор {elem}");
            }
        }

        private void ProcessAllElems()
        {
            stackNumbers.Clear();
            stackOperations.Clear();

            for (int i = 0; i < elemsList.Count; i++)
            {
                var elem = elemsList[i];

                if (OPERATION_ELEMS_LIST.Contains(elem) || INTERNAL_OPERATION_ELEMS_LIST.Contains(elem))
                {
                    AddOneOperation(elem);
                }
                else
                {
                    if (double.TryParse(elem, out double d))
                        stackNumbers.Push(d);
                    else
                        throw new ArgumentException($"Ошибочный элемент [{elem}]");
                }
#if DEBUG
                ShowAll($"From Input Line: [{elem}]");
#endif
            }

            //обработать остатки операций из стека
            while (stackOperations.Count > 0)
            {
                DoStackOperation();
            }

            if (stackOperations.Count != 0)
                throw new ArgumentException($"Ошибочный элемент [{stackOperations.Peek()}]");
            if (stackNumbers.Count > 1)
                throw new ArgumentException($"Ошибочный элемент [{stackNumbers.Peek()}]");
            if (stackNumbers.Count == 0)
                throw new ArgumentException($"Ошибка во входной строке");

            hasErrors = hasDivideByZero = false;
            isParsed = true;
            calculatedResult = stackNumbers.Peek();
        }
        #endregion

        public CalcLineProcessor(string textLine)
        {
            if (string.IsNullOrEmpty(textLine))
            {
                hasErrors = true;
                hasDivideByZero = false;
                isParsed = false;
                return;
            }
            textLine = textLine.Trim().Replace('.', DECIMAL_SEPARATOR).Replace(',', DECIMAL_SEPARATOR);
            while (textLine.Contains("  "))
                textLine = textLine.Replace("  ", " ");
            foreach (var str in OPERATION_ELEMS_LIST)
                textLine = textLine.Replace(" " + str, str).Replace(str + " ", str);

            textToParse = textLine;
            try
            {
                SplitLineToElems();
                ProcessAllElems();

            }
            catch (DivideByZeroException)
            {
                hasErrors = hasDivideByZero = true;
                isParsed = false;
                return;
            }
            catch (Exception)//InvalidOperationException, ArgumentException
            {
                hasErrors = true;
                isParsed = false;
                return;
            }
        }
    }
}
