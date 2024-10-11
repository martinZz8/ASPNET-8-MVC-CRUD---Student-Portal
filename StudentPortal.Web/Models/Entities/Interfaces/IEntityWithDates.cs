namespace StudentPortal.Web.Models.Entities.Interfaces
{
    public interface IEntityWithDates
    {
        DateTime CreateDate { get; set; }
        DateTime UpdateDate { get; set; }
    }
}
