using System;

namespace Common.Models
{
    /// <summary>
    /// نموذج القيود البسيطة
    /// </summary>
    public class SimpleEntry
    {
        public long ID { get; set; }
        
        public long? TheNumber { get; set; }
        
        public DateTime? TheDate { get; set; }
        
        public string Description { get; set; }
        
        public string AccountFrom { get; set; } // الحساب المدين
        
        public string AccountTo { get; set; } // الحساب الدائن
        
        public decimal? Amount { get; set; }
        
        public string Currency { get; set; }
        
        public string Notes { get; set; }
        
        public long? UserID { get; set; }
        
        public DateTime? EnterTime { get; set; }
    }
}
