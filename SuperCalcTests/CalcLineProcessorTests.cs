using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace _05_SuperCalc.Tests
{
    [TestClass()]
    public class CalcLineProcessorTests
    {
        [TestMethod()]
        public void CalcLineProcessorTestDivideByZero()
        {
            string[] lines = {
                "2/0",
                "3.7/(1.8-1.8)",
                "5.10/(7-7)"
            };
            for (int i = 0; i < lines.Length; i++)
            {
                var clp = new CalcLineProcessor(lines[i]);

                string errMsg = $"[{lines[i]}]";
                Assert.IsTrue(clp.HasErrors, errMsg);
                Assert.IsTrue(clp.HasDivideByZero, errMsg);
            }
        }


        [TestMethod()]
        public void CalcLineProcessorTestAllErrors()
        {
            string[] lines = {
                "1+(", 
                "1 + x + 4",
                ")", 
                "(",
                "5(4",
                @"4\\8", 
                "*4", 
                "*4+2",
                "4//8",
                "4 8",
                "5+4     -5*(3+ 4 4)",
                "()",
                "44.5.9+23.5.4",
                null, 
                ""
            };
            for (int i = 0; i < lines.Length; i++)
            {
                var clp = new CalcLineProcessor(lines[i]);

                string errMsg = $"[{lines[i]}]";
                Assert.IsTrue(clp.HasErrors, errMsg);
            }
        }


        [TestMethod()]
        public void CalcLineProcessorTestAllOk()
        {
            double delta = 0.001;

            string[] lines = {
                "45.9+7*4.4+(4.7/8.9)-12.1",
                "1 + 2 * (3 + 2)",
                "2+15/3+4*2",
                "2-5   +(8-40)",
                "2-15/3+(8-40)",
                "2-15/3+(4*2-40)",
                "2-15/3+(4*2-(45-5)*1)",
                "2-15/3+(4*2-(45-5)*1)-8",
                " 1 ",
                "(((2+4)*2/6)-8)+7*(3-4*2)",
                "2+2*3/4",
                "   -5+2",
                "5+(-2*3)",
                "(-2*3)",
                "-5+3+((     -6)*3)"
            };

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace(',', '.');
                var clp = new CalcLineProcessor(lines[i]);

                string errMsg = $"[{lines[i]}]";
                double exp = double.Parse(new DataTable().Compute(lines[i], null).ToString());
                Assert.IsFalse(clp.HasDivideByZero, "HasDivideByZero " + errMsg);
                Assert.IsFalse(clp.HasErrors, "HasErrors " + errMsg);
                Assert.IsTrue(clp.IsParsed, "IsParsed " + errMsg);
                Assert.AreEqual(exp, clp.ParsedResult, delta, "ParsedResult " + errMsg);
            }
        }
    }
}