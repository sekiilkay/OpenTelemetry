using Common.Shared.DTOs;
using Common.Shared.Events;
using MassTransit;
using OpenTelemetry.Shared.Constants;
using Order.API.Models;
using Order.API.RedisServices;
using Order.API.StockServices;
using System.Diagnostics;
using System.Net;

namespace Order.API.OrderServices
{
    public class OrderService
    {
        private readonly AppDbContext _context;
        private readonly StockService _stockService;
        private readonly RedisService _redisService;
        private readonly ILogger<OrderService> _logger;
        public OrderService(AppDbContext context, StockService stockService, RedisService redisService, ILogger<OrderService> logger)
        {
            _context = context;
            _stockService = stockService;
            _redisService = redisService;
            _logger = logger;
        }
        public async Task<ResponseDto<OrderCreateResponseDto>> CreateAsync(OrderCreateRequestDto requestDto)
        {

            using (var redisActivity = ActivitySourceProvider.Source.StartActivity("RedisStringActivity"))
            {
                await _redisService.GetDb(0).StringSetAsync("userId", requestDto.UserId);

                redisActivity.SetTag("userId", requestDto.UserId);

                var redisUserId = await _redisService.GetDb(0).StringGetAsync("UserId");
            }

            Activity.Current.SetTag("Asp.Net Core(Instrumentation) Tag", "Asp.Net Core(Instrumentation) Tag Value");

            using var activity = ActivitySourceProvider.Source.StartActivity();

            activity.AddEvent(new ActivityEvent("Sipariş Süreci Başlatıldı"));

            activity.SetBaggage("userId", requestDto.UserId.ToString());

            var newOrder = new Order()
            {
                Created = DateTime.Now,
                Code = Guid.NewGuid().ToString(),
                Status = OrderStatus.Success,
                UserId = requestDto.UserId,
                Items = requestDto.Items.Select(x => new OrderItem()
                {
                    Count = x.Count,
                    ProductId = x.ProductId,
                    UnitPrice = x.UnitPrice,
                }).ToList()
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Sipariş veritbanına kaydedildi {@userId}", requestDto.UserId);

            StockCheckAndPaymentProcessRequestDto stockRequest = new(); 

            stockRequest.OrderCode = newOrder.Code;
            stockRequest.OrderItems = requestDto.Items;

            var (isSuccess, failMessage) = await _stockService.CheckStockAndPaymentStartAsync(stockRequest);

            if (!isSuccess)
                return ResponseDto<OrderCreateResponseDto>.Fail(HttpStatusCode.InternalServerError.GetHashCode(), failMessage);

            activity.AddEvent(new ActivityEvent("Sipariş Süreci Tamamlandı"));

            return ResponseDto<OrderCreateResponseDto>.Success(HttpStatusCode.OK.GetHashCode(), new OrderCreateResponseDto() { Id = newOrder.Id });
        }
    }
}
