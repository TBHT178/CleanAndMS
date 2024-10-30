using OrderApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApi.Application.DTOs.Conversions
{
    public static class OrderConversion
    {
        public static Order ToEntity(OrderDTO dto) => new Order { 
            Id = dto.Id,
            ClientId = dto.ClientId,
            ProductId = dto.ProductId,
            OrderedDate = dto.OrderedDate,
            PurchaseQuantity = dto.PurchaseQuantity,
        };

        public static (OrderDTO?, IEnumerable<OrderDTO>?) FromEntity(Order? order, IEnumerable<Order>? orders)
        {
            // Return single
            if(order is not null || orders is null) 
            {
                var singleOrder = new OrderDTO(order!.Id, order.ProductId, order.ClientId, order.PurchaseQuantity, order.OrderedDate);
                return (singleOrder, null);
            }

            // Return list
            if(orders is not null || order is null)
            {
                var _orders = orders!.Select(o =>
                    new OrderDTO(o.Id, o.ProductId, o.ClientId, o.PurchaseQuantity, o.OrderedDate)
                );

                return (null, _orders);
            }

            return (null, null);
        } 
    }
}
