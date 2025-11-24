using System.ComponentModel;

namespace Modula.Models
{
    public class BorrowTicket
    {
        public int ID { get; set; }
        public int TotalPage { get; set; }
        public int Status { get; set; }
        public int StatusNew { get; set; }
        public int ProductRTCID { get; set; }
        public int ProductRTCQRCodeID { get; set; }
        public DateTime DateBorrow { get; set; }
        public DateTime DateReturnExpected { get; set; }
        public int PeopleID { get; set; }
        public string Project { get; set; }
        public DateTime? DateReturn { get; set; }
        public string Note { get; set; }
        public int? NumberBorrow { get; set; }
        public bool AdminConfirm { get; set; }
        public int BillExportTechnicalID { get; set; }
        public string FullName { get; set; }
        public int ProductGroupRTCID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Maker { get; set; }
        public int UnitCountID { get; set; }
        public int Number { get; set; }
        public bool StatusProduct { get; set; }
        public DateTime CreateDate { get; set; }
        public int NumberInStore { get; set; }
        public string Serial { get; set; }
        public string SerialNumber { get; set; }
        public string PartNumber { get; set; }
        public string CreatedBy { get; set; }
        public string LocationImg { get; set; }
        public string ProductCodeRTC { get; set; }
        public bool BorrowCustomer { get; set; }
        public string AddressBox { get; set; }
        public string BillExportCode { get; set; }
        public string BillTypeText { get; set; }
        public int BillStatus { get; set; }
        public bool? IsDelete { get; set; }
        public string DepartmentName { get; set; }
        public string ProductQRCode { get; set; }
        public string UnitCountName { get; set; }
        public string UserZaloID { get; set; }
        public string ModulaLocationName { get; set; }
        public string ModulaLocationCode { get; set; }
        public int AxisX { get; set; }
        public int AxisY { get; set; }
        public string StatusText { get; set; }
        public int RowNumber { get; set; }
        public int DualDate { get; set; }
        public bool IsBorrow { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}