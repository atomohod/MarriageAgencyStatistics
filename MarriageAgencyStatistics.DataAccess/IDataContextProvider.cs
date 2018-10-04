namespace MarriageAgencyStatistics.DataAccess
{
    public interface IDataContextProvider<out T> where T : IContext, new()
    {
        T Create();
    }
}