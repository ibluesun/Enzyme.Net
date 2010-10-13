using Enzyme;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ParticleLexer;
using Enzyme.Tokens;
using ParticleLexer.StandardTokens;
using System.Diagnostics;

namespace ValidationTestingProject
{
    
    
    /// <summary>
    ///This is a test class for RVEngineTest and is intended
    ///to contain all RVEngineTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnzymeEngineTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ParseToTokens
        ///</summary>
        [TestMethod()]
        public void ParseToTokensTest()
        {
            EnzymeEngine target = new EnzymeEngine(null); // TODO: Initialize to an appropriate value

            string codeLine = "If $21 == 001 AND $23 == 01 AND $27 == 001 THEN ($14 == 1 OR $15 == 6) AND ($12==3 OR $24==\"hello\")";

            Token actual = target.ParseToTokens(codeLine);

            Assert.AreEqual<Type>(typeof(IfWordToken), actual[0].TokenClassType);
            Assert.AreEqual<Type>(typeof(SymbolicToken), actual[1].TokenClassType);
            Assert.AreEqual<Type>(typeof(EqualityToken), actual[2].TokenClassType);
            Assert.AreEqual<Type>(typeof(NumberToken), actual[3].TokenClassType);
            Assert.AreEqual<Type>(typeof(AndWordToken), actual[4].TokenClassType);

            
        }

        /// <summary>
        ///A test for Evaluate
        ///</summary>
        [TestMethod()]
        public void EvaluateTest()
        {
            IEnzymeContext fieldsProvider = new EnzymeStaticContext(); // TODO: Initialize to an appropriate value
            EnzymeEngine target = new EnzymeEngine(fieldsProvider); // TODO: Initialize to an appropriate value
            
            string codeLine = "if $8 == 4+5 then 10 else 2*10";

            object expected = 20.0;
            object actual;
            
            actual = target.Evaluate(codeLine);


            Assert.AreEqual(expected, actual);
            
        }


        [TestMethod()]
        public void MoreEvalute()
        {
            IEnzymeContext fpr = new EnzymeDynamicContext();

            EnzymeEngine engine = new EnzymeEngine(fpr);

            Debug.Print(engine.Evaluate("$r1 = 20").ToString());
            Debug.Print(engine.Evaluate("$r2 = 12").ToString());

            double d = (double)engine.Evaluate("$R = ($r1*$r2)/($r1+$r2)");


            Assert.AreEqual(d, 7.5);

        }


    }
}
