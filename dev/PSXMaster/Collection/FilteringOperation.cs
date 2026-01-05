namespace PSXMaster.Collection;
public partial class FilteringOperation<T> : ObservableObject, IOperation<T>
{
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    /// <summary>
    /// Custom filtering logic (e.g., search conditions).
    /// </summary>
    public Func<T, bool>? CustomFilter { get; set; }

    /// <summary>
    /// Optional custom ordering logic (e.g., OrderBy or OrderByDescending).
    /// </summary>
    public Func<IEnumerable<T>, IEnumerable<T>>? OrderByFunc { get; set; }

    public IEnumerable<T> Execute(IEnumerable<T> sourceItems)
    {
        if (!IsEnabled)
            return sourceItems;

        var result = CustomFilter is not null
            ? sourceItems.Where(CustomFilter)
            : sourceItems;

        if (OrderByFunc is not null)
        {
            result = OrderByFunc(result);
        }

        return result;
    }
}

