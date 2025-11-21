namespace Modula.Models
{
    public class ModulaHistories
    {
        public int status { get; set; }
        public string message { get; set; }
        public dynamic error { get; set; }
        public ModulaHistoriesData data { get; set; }
    }

    public class ModulaHistoriesData
    {
        public List<BorrowTicket> borrows { get; set; }
        public List<BorrowTicket> returns { get; set; }
    }
}