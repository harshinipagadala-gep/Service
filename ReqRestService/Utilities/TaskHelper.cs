using GEP.Cumulus.Documents.Entities;
using System.Collections.Generic;

namespace GEP.Cumulus.P2P.Req.RestService
{
    public static class TaskHelper
    {
        public static TaskInformation CreateTaskObject(long documentCode, long contactCode, List<TaskActionDetails> lstTaskActionDetails, bool isDeleted, bool isSupplier, long buyerPartnerCode, string companyName)
        {
            TaskInformation taskInformation = new TaskInformation();
            taskInformation.DocumentCode = documentCode;
            taskInformation.CompanyName = companyName;
            taskInformation.DeleteBuyerAdditionalDetails = false;
            taskInformation.DeleteSupplierAdditionalDetails = false;

            TaskDetails taskDetailsAdd = new TaskDetails();
            taskDetailsAdd.ContactCode = contactCode;
            taskDetailsAdd.BuyerPartnerCode = buyerPartnerCode;
            taskDetailsAdd.IsSupplier = isSupplier;
            taskDetailsAdd.IsDeleted = isDeleted;
            if (lstTaskActionDetails != null)
            {
                foreach (var item in lstTaskActionDetails)
                    taskDetailsAdd.TaskActionDetailsList.Add(item);
                if (isSupplier)
                    taskInformation.SupplierTaskDetailsList.Add(taskDetailsAdd);
                else
                    taskInformation.BuyerTaskDetailsList.Add(taskDetailsAdd);
            }
            return taskInformation;
        }

        public static TaskActionDetails CreateActionDetails(ActionKey actionKey, string actionText, string additionalDetails = "")
        {
            TaskActionDetails taskActionDetails = new TaskActionDetails();
            taskActionDetails.ActionKey = actionKey;
            taskActionDetails.ActionText = actionText;
            taskActionDetails.AdditionalDetails = additionalDetails;
            return taskActionDetails;
        }

    }
}
