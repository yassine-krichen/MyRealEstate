using MediatR;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Application.Queries.Analytics;

public class GetMostViewedPropertiesQueryHandler : IRequestHandler<GetMostViewedPropertiesQuery, List<PropertyViewStats>>
{
    private readonly IPropertyViewRepository _propertyViewRepository;

    public GetMostViewedPropertiesQueryHandler(IPropertyViewRepository propertyViewRepository)
    {
        _propertyViewRepository = propertyViewRepository;
    }

    public async Task<List<PropertyViewStats>> Handle(GetMostViewedPropertiesQuery request, CancellationToken cancellationToken)
    {
        return await _propertyViewRepository.GetMostViewedPropertiesAsync(
            request.TopCount,
            request.FromDate,
            request.ToDate,
            cancellationToken);
    }
}
