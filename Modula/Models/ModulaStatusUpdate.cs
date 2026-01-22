namespace Modula.Models
{
    public class ModulaStatusUpdate
    {
        public int ID;
        public int PeopleID;
        /// <summary>
        /// 1: Hoàn thành thao tác lấy hàng; 2: Hoàn thành thao tác trả hàng
        /// </summary>
        public int StatusPerson;
    }
}