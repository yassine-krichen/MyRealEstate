using MediatR;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Queries.Analytics;

/// <summary>
/// Query to get the most viewed properties
/// </summary>
public class GetMostViewedPropertiesQuery : IRequest<List<PropertyViewStats>>
{
    public int TopCount { get; set; } = 10;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
