using System;

namespace Common.Models
{
    /// <summary>
    /// نموذج بيانات سند القبض
    /// </summary>
    public class Receipt
    {
        public long ID { get; set; }
        
        public long? TheNumber { get; set; }
        
        public DateTime? TheDate { get; set; }
        
        public long? TheMethod { get; set; } // 1=نقدي، 2=شيك، 3=تحويل بنكي
        
        public decimal? Amount { get; set; }
        
        public long? CurrencyID { get; set; } // 1=دينار، 2=دولار، 3=يورو
        
        public long? AccountID { get; set; }
        
        public string Notes { get; set; }
        
        public long? UserID { get; set; }
        
        public DateTime? EnterTime { get; set; }
    }
}
