using System.Diagnostics;
using Grpc.Core;
using OrdersValidator.Diagnostics;
using OrderValidators;

namespace OrdersValidator.Services;

public sealed class OrderValidatorService : OrderValidator.OrderValidatorBase
{
    public override Task<OrderValidationReply> Validate(OrderValidationRequest request, ServerCallContext context)
    {
        var rnd = Random.Shared.Next(0, 10);

        try
        {
            if (rnd <= 1)
            {
                throw new Exception("imagine this is some valid domain exception idk");
            }
        }
        catch (Exception ex)
        {
            Activity.Current?.RecordOrderValidationException(ex);
            return Task.FromResult(new OrderValidationReply { ValidationResult = ValidationResult.Failed });
        }

        return Task.FromResult(new OrderValidationReply { ValidationResult = ValidationResult.Passed });
    }
}