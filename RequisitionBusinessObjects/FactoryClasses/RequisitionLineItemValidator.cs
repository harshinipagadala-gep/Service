using GEP.Cumulus.P2P.BusinessEntities;
using System.Text;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.FactoryClasses
{
    public class RequisitionLineItemValidator
    {
        private long documentCode = 0;
        private NewRequisitionManager RequisitionManger = null;
        private StringBuilder errorList = new StringBuilder();
        private RequisitionItem requisitionItem;
        public RequisitionLineItemValidator(RequisitionItem item, StringBuilder error)
        {
            requisitionItem = item;
            errorList = error;
        }

        public bool Validate()
        {
            var result = true;

            return result;
        }

        public bool ValidateAccounting()
        {
            bool result = true;

            return result;
        }

    }
}
