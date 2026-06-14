using System.Collections.Generic;

public class OrderViewData
{
    public int Id { get; set; }

    public string CustomerFullName { get; set; }
    public OrderStatus Status { get; set; }
    public string CreatedAt { get; set; }

    public bool CanChangeStatus { get; set; }
    public bool CanDelete { get; set; }

    public List<string> Items { get; set; }

    public Order SourceOrder { get; set; }
}