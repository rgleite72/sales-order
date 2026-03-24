using Microsoft.Extensions.Logging;
using SalesOrder.Domain.Orders.Entities;
using SalesOrder.Application.Contracts;
using SalesOrder.Application.Contracts.Integration;
using SalesOrder.Application.Contracts.Persistence;
using SalesOrder.Application.DTOs.Order;
using SalesOrder.Application.Exceptions;

namespace SalesOrder.Application.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly IProductCatalogGateway _productCatalogGateway;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IUnitOfWork uow,
        ISalesOrderRepository salesOrderRepository,
        IProductCatalogGateway productCatalogGateway,
        ILogger<OrderService> logger)
    {
        _uow = uow;
        _salesOrderRepository = salesOrderRepository;
        _productCatalogGateway = productCatalogGateway;
        _logger = logger;
    }

    public async Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderRequestDto dto, CancellationToken ct)
    {
        _logger.LogInformation(
            "Starting order creation. CustomerId={CustomerId}, ItemsCount={ItemsCount}",
            dto.CustomerId,
            dto.Items?.Count ?? 0);

        if (dto.CustomerId == Guid.Empty)
            throw ValidationException.Required(nameof(dto.CustomerId));

        if (dto.Items is null || dto.Items.Count == 0)
            throw ValidationException.Required(nameof(dto.Items));

        if (dto.Items.Any(x => x.Quantity <= 0))
            throw ValidationException.ForField(nameof(dto.Items), "Must be greater than zero.");

        if (string.IsNullOrWhiteSpace(dto.Currency))
            throw ValidationException.Required(nameof(dto.Currency));

        await using var tx = await _uow.BeginTransactionAsync(ct);

        try
        {
            var now = DateTime.UtcNow;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"SO-{now:yyyyMMddHHmmss}",
                CustomerId = dto.CustomerId,
                OrderDate = now,
                Currency = dto.Currency.Trim().ToUpper(),
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                Status = "Pending",
                TotalAmount = 0m,
                CreatedAt = now,
                UpdatedAt = now
            };

            _logger.LogInformation(
                "Transaction started for order creation. OrderId={OrderId}, OrderNumber={OrderNumber}",
                order.Id,
                order.OrderNumber);

            var orderItems = new List<OrderItem>();

            foreach (var itemDto in dto.Items)
            {
                _logger.LogInformation(
                    "Validating product in ProductCatalog. OrderId={OrderId}, ProductId={ProductId}, Quantity={Quantity}",
                    order.Id,
                    itemDto.ProductId,
                    itemDto.Quantity);

                var product = await _productCatalogGateway.GetProductByIdAsync(itemDto.ProductId, ct);

                if (product is null)
                {
                    _logger.LogWarning(
                        "Product not found in ProductCatalog. OrderId={OrderId}, ProductId={ProductId}",
                        order.Id,
                        itemDto.ProductId);

                    throw new NotFoundException($"Product with id '{itemDto.ProductId}' was not found.");
                }

                if (!string.Equals(product.Currency, order.Currency, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Product currency does not match order currency. OrderId={OrderId}, ProductId={ProductId}, ProductCurrency={ProductCurrency}, OrderCurrency={OrderCurrency}",
                        order.Id,
                        product.Id,
                        product.Currency,
                        order.Currency);

                    throw ValidationException.ForField(
                        nameof(dto.Currency),
                        "Product currency does not match order currency.");
                }

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.InitialPrice,
                    TotalPrice = product.InitialPrice * itemDto.Quantity
                };

                orderItems.Add(orderItem);

                _logger.LogInformation(
                    "Order item added successfully. OrderId={OrderId}, ProductId={ProductId}, UnitPrice={UnitPrice}, TotalPrice={TotalPrice}",
                    order.Id,
                    product.Id,
                    orderItem.UnitPrice,
                    orderItem.TotalPrice);
            }

            order.TotalAmount = orderItems.Sum(x => x.TotalPrice);
            order.OrderItems = orderItems;

            _logger.LogInformation(
                "Order calculated successfully. OrderId={OrderId}, TotalAmount={TotalAmount}, ItemsCount={ItemsCount}",
                order.Id,
                order.TotalAmount,
                orderItems.Count);

            await _salesOrderRepository.AddAsync(order, ct);
            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "Order created successfully. OrderId={OrderId}, OrderNumber={OrderNumber}, TotalAmount={TotalAmount}",
                order.Id,
                order.OrderNumber,
                order.TotalAmount);

            return new SalesOrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Currency = order.Currency,
                Status = order.Status,
                TotalAmount = order.TotalAmount
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);

            _logger.LogError(
                ex,
                "Order creation failed and transaction was rolled back. CustomerId={CustomerId}",
                dto.CustomerId);

            throw;
        }
    }

    public async Task<SalesOrderResponseDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Getting order by id. OrderId={OrderId}", id);

        var order = await _salesOrderRepository.GetByIdAsync(id, ct);

        if (order is null)
        {
            _logger.LogWarning("Order not found. OrderId={OrderId}", id);
            throw new NotFoundException("Order not found");
        }

        _logger.LogInformation("Order retrieved successfully. OrderId={OrderId}, Status={Status}", order.Id, order.Status);

        return new SalesOrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            Currency = order.Currency,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.OrderItems.Select(item => new SalesOrderItemResponseDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }

    public async Task<PagedResponseDto<SalesOrderResponseDto>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken ct)
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        _logger.LogInformation(
            "Listing orders. Page={Page}, PageSize={PageSize}, Search={Search}, Status={Status}",
            page,
            pageSize,
            search,
            status);

        var (orders, totalCount) = await _salesOrderRepository.ListAsync(page, pageSize, search, status, ct);

        _logger.LogInformation(
            "Orders listed successfully. Page={Page}, PageSize={PageSize}, Total={Total}",
            page,
            pageSize,
            totalCount);

        var items = orders.Select(order => new SalesOrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            Currency = order.Currency,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.OrderItems.Select(item => new SalesOrderItemResponseDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        }).ToList();

        return new PagedResponseDto<SalesOrderResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = totalCount
        };
    }

    public async Task CancelAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Starting order cancellation. OrderId={OrderId}", id);

        var order = await _salesOrderRepository.GetByIdAsync(id, ct);

        if (order is null)
        {
            _logger.LogWarning("Order not found for cancellation. OrderId={OrderId}", id);
            throw new NotFoundException("Order not found");
        }

        if (order.Status == "Canceled")
        {
            _logger.LogWarning("Order is already canceled. OrderId={OrderId}", id);
            throw ValidationException.ForField(nameof(order.Status), "Order is already canceled.");
        }

        order.Status = "Canceled";
        order.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Order canceled successfully. OrderId={OrderId}", id);
    }

    public async Task ConfirmAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Starting order confirmation. OrderId={OrderId}", id);

        var order = await _salesOrderRepository.GetByIdAsync(id, ct);

        if (order is null)
        {
            _logger.LogWarning("Order not found for confirmation. OrderId={OrderId}", id);
            throw new NotFoundException("Order not found");
        }

        if (order.Status == "Confirmed")
        {
            _logger.LogWarning("Order is already confirmed. OrderId={OrderId}", id);
            throw ValidationException.ForField(nameof(order.Status), "Order is already confirmed.");
        }

        if (order.Status == "Canceled")
        {
            _logger.LogWarning("Canceled order cannot be confirmed. OrderId={OrderId}", id);
            throw ValidationException.ForField(nameof(order.Status), "Canceled order cannot be confirmed.");
        }

        order.Status = "Confirmed";
        order.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Order confirmed successfully. OrderId={OrderId}", id);
    }
}
