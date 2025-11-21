using System;

namespace Common.Models
{
    /// <summary>
    /// نموذج عرض سندات القبض
    /// </summary>
    public class ReceiptView
    {
        public long ID { get; set; }
        
        public long? الرقم { get; set; }
        
        public DateTime? التاريخ { get; set; }
        
        public string طريقة_القبض { get; set; }
        
        public decimal? المبلغ { get; set; }
        
        public string العملة { get; set; }
        
        public string اسم_الحساب { get; set; }
        
        public string المستخدم { get; set; }
        
        public bool? معتمد { get; set; }
        
        public string ملاحظات { get; set; }
    }
}
