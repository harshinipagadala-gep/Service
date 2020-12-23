using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEP.Cumulus.Requisition.Tests.DataSource.DAO
{
    [ExcludeFromCodeCoverage]
    public class RequisitionData
    {
        public long DocumentCode;
        public int DocumentStatus;
        public string DocumentNumber;
        public long Creator;
        public Decimal Tax;
    }

    [ExcludeFromCodeCoverage]
    public class ExecuteTestCases
    {
        public ExecuteTestCases(bool execute, DateTime dateTime)
        {
            var tm = dateTime.TimeOfDay;
            //TimeSpan ts = new TimeSpan(18, 30, 0);
            this.ExecutionTime = DateTime.Now.Date + tm;
            this.ExecuteTestCase = execute;
        }
        public bool ExecuteTestCase { get; }

        public DateTime ExecutionTime { get; }
    }
}
