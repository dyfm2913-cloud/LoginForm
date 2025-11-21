using System;

namespace Common.Models
{
    public class Spending
    {
        public long ID { get; set; }
        public long TheNumber { get; set; }
        public DateTime TheDate { get; set; }
        public long TheMethod { get; set; }
        public decimal Amount { get; set; }
        public long CurrencyID { get; set; }
        public long AccountID { get; set; }
        public decimal ExchangeAmount { get; set; }
        public long ExchangeCurrencyID { get; set; }
        public long ExchangeAccountID { get; set; }
        public string Notes { get; set; }
        public string RefernceNumber { get; set; }
        public string Delivery { get; set; }
        public long? CostCenterID { get; set; }
        public long UserID { get; set; }
        public long BranchID { get; set; }
        public DateTime EnterTime { get; set; }
        public int Prints { get; set; }
        public string SpecialChequeNumber { get; set; }
        public long? CommissionerID { get; set; }
        public bool IsCommissioner { get; set; }
        public bool IsDepend { get; set; }
        public long? DependUserID { get; set; }
        public long? ChequeID { get; set; }
        public string EntryID { get; set; }
    }
}