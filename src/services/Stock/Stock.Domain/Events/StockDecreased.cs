﻿using Core.Domain.Base;

namespace Stock.Domain.Events
{
    public class StockDecreased : IDomainEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }

        public StockDecreased(Guid correlationId, Guid userId, decimal totalAmount)
        {
            CorrelationId = correlationId;
            UserId = userId;
            TotalAmount = totalAmount;
        }
    }
}
