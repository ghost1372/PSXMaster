namespace PSXMaster.Collection;
public interface IOperation<T>
{
    bool IsEnabled { get; set; }

    IEnumerable<T> Execute(IEnumerable<T> source);
}
