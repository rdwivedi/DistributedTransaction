using AutoMapper;
using MassTransit;
using MediatR;
using Messages.IntegrationEvents;
using Order.Application.Features.Commands;

namespace Order.API.Consumers
{
    public class StockFailedConsumer : IConsumer<StockFailedIE>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public StockFailedConsumer(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<StockFailedIE> context)
        {
            await _mediator.Send(_mapper.Map<StockFailed.Command>(context.Message));
        }
    }
}
