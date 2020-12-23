using GEP.Cumulus.Requisition.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequisitionMSTest
{
    [ExcludeFromCodeCoverage]
    public class TestHelper
    {
        public TestHelper()
        {
            TestCaseSourceFactory.SetExecutionFlag();
        }

        public static bool CheckToExecute
        {
            get
            {
                return TestCaseSourceFactory.ExecuteTestCases != null &&
               TestCaseSourceFactory.ExecuteTestCases.ExecuteTestCase &&
               DateTime.Now > TestCaseSourceFactory.ExecuteTestCases.ExecutionTime;
            }
        }
        public static long LongRandom(long min, long max)
        {
            Random rand = new Random();
            long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)rand.Next((Int32)min, (Int32)max);
            return result;
        }
    }
}
