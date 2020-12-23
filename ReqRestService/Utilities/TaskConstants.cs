namespace GEP.Cumulus.P2P.Req.RestService
{
    public static class TaskConstants
    {
        #region My Task //Note : Need to remove once Localization is implemented
        public const string SENT_FOR_APPROVAl = "Send for Approval";
        public const string APPROVE = "Approve";
        public const string REJECT = "Reject";
        public const string SUBMIT_TO_PARTNER = "Submit to Supplier";
        public const string REQ_SENT_FOR_APPROVAl_MESSAGE = "Requisition Sent for Approval successfully.";
        public const string APPROVED_MESSAGE = "Approved successfully.";
        public const string REJECTED_MESSAGE = "Rejected successfully.";
        public const string APPROVED_ERROR_MESSAGE = "Error occurred while Approving";
        public const string REJECTED_ERROR_MESSAGE = "Error occurred while Rejecting";
        public const string REQ_FILL_ALL_MANDATORY_INFORMATION = "Fill All Mandatory information.";
        public const string APPROVAL_VALIDATION_FAILED_MESSAGE = "Approval Validation Failed.";
        public const string RECEIPT_FINALIZE_SUCCESS_MESSAGE = "Receipt Finalize successfully.";
        public const string RECEIPT_FINALIZE_ERROR_MESSAGE = "Error occurred while Finalize Receipt.";
        public const string RECEIPT_PLEASEENTERRECEIVEDBY = "Please Enter Received By.";
        public const string P2P_ORDER_ORDERSENTTOPARTNERSUCCESSFULLY = "Order Sent to Supplier Successfully.";
        public const string P2P_ORDER_ERROROCCUREDWHILESENDINGORDERTOPARTNER = "Error occured while Sending Order to Supplier.";
        public const string P2P_INV_ERROROICCUREDWHILESUBMITTINGINVOICETOBUYER = "Error occured while submitting Invoice to Buyer.";
        public const string P2P_ERROROCCUREDWHILEVALIDATINGBUDGET = "Allocated funds are exceeded. Please visit the document detail page and click on approve for more information.";
        public const string P2P_INV_INVOICESUBMITTEDTOBUYERSUCCESSFULLY = "Invoice Submitted to Buyer Successfully.";
        public const string EDIT = "Edit";
        public const string ACCEPT = "Accept";
        public const string ACCEPT_ERR_MESSAGE = "Error occured while Accepting.";
        public const string REJECT_ERR_MESSAGE = "Error occured while Rejected.";
        public const string ACCEPT_MESSAGE = "Accepted Successfully.";
        public const string REJECT_MESSAGE = "Rejected Successfully.";
        public const string REQUISITION = "Requisition";
        public const string ORDER = "Order";
        public const string IR = "IR";
        public const string PAYMENTREQUEST = "PaymentRequest";
        public const string P2P_ORDER_ORDERACKNOWLEDGEDSUCCESSFULLY = "Order Acknowledged Successfully.";
        public const string P2P_ORDER_ERROROCCUREDWHILEACKNOWLEDGED = "Error occured while Acknowldging Order.";
        public const string P2P_ERROROCCUREDWHILEVALIDATINGBUDGETOVERALL = "Overall budget is not available for selected period.";
        public const string P2P_ERROROCCUREDWHILEVALIDATINGBUDGETPERIOD = "There is no available period for the budget allocation.";
        public const string UPDATEUSERDEFINEDAPPROVALMESSAGE = "Below list of user(s) are removed due to lack of authority for the value of this document. Go to 'Manage Transactional Approvals' to update approver list";
        public const string ACCEPTOR_AS_APPROVER_VALIDATION_MESSAGE = "You cannot select following person(s) as approver since he/she is also an accepter of this document. This violates segregation of duty compliance.";
        public const string SENT_FOR_REVIEW = "Send for Review";
        public const string P2P_REQ_SUBMIT_ERROR = "The selected action is not valid for the current state of the document and hence it cannot be performed. The document will get refreshed to reflect its current state.";
        #endregion
    }
}
