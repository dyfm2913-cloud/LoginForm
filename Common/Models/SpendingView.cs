using System;

namespace Common.Models
{
    public class SpendingView
    {
        public long ID { get; set; }
        public long? الرقم { get; set; }
        public DateTime? التاريخ { get; set; }
        public string طريقة_الصرف { get; set; }
        public decimal المبلغ { get; set; }
        public string العملة { get; set; }
        public string الصندوق { get; set; }
        public decimal مبلغ_الحساب { get; set; }
        public string عملة_الحساب { get; set; }
        public string اسم_الحساب { get; set; }
        public string ملاحظات { get; set; }
        public string رقم_المرجع { get; set; }
        public string مناولة { get; set; }
        public string مركز_التكلفة { get; set; }
        public string المستخدم { get; set; }
        public string الفرع { get; set; }
        public DateTime? وقت_الإدخال { get; set; }
        public int الطبعات { get; set; }
        public string رقم_الشيك_الخاص { get; set; }
        public string اسم_المفوض { get; set; }
        public bool الصرف_للمفوض { get; set; }
        public bool? معتمد { get; set; }
        public string المعتمد { get; set; }
        public string الفئات { get; set; }
    }
}