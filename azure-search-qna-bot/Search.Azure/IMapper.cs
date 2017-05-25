namespace Search.Azure
{
    public interface IMapper<T, TResult>
    {
        TResult Map(T item);
    }
}