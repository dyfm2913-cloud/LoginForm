using System;

namespace Common.Models
{
    /// <summary>
    /// نموذج عرض القيود البسيطة
    /// </summary>
    public class SimpleEntryView
    {
        public long ID { get; set; }
        
        public long? الرقم { get; set; }
        
        public DateTime? التاريخ { get; set; }
        
        public string الوصف { get; set; }
        
        public string الحساب_المدين { get; set; }
        
        public string الحساب_الدائن { get; set; }
        
        public decimal? المبلغ { get; set; }
        
        public string العملة { get; set; }
        
        public string المستخدم { get; set; }
        
        public string ملاحظات { get; set; }
    }
}
