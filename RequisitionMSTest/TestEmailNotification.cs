using System;
using GEP.Cumulus.P2P.BusinessObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequisitionMSTest.DataSource;

namespace RequisitionMSTest
{
    [TestClass]
    public class TestEmailNotification
    {
        EmailNotificationManager emailManager;

        public TestEmailNotification()
        {
            emailManager = new EmailNotificationManager();
            emailManager.UserContext = UserContextHelper.GetExecutionContext;
            emailManager.GepConfiguration = Helper.InitMultiRegion();
        }

        [TestMethod]
        public void OBONotification()
        {
            var result = emailManager.SendNotificationForRequisitionReview(270822, null, null, "OnSubmit", GEP.Cumulus.Documents.Entities.DocumentStatus.ReviewPending, ""); // Review Pending req where OBOID!= RequesterID

            var result1 = emailManager.SendNotificationForRequisitionReview(269271, null, null, "OnSubmit", GEP.Cumulus.Documents.Entities.DocumentStatus.ReviewPending, "");// Review Pending req where OBOID == RequesterID

            Assert.IsNotNull(result);
            Assert.IsTrue(result);

            Assert.IsNotNull(result1);
            Assert.IsFalse(result1);
        }
    }
}
