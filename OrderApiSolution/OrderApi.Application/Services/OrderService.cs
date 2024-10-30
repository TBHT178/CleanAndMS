using OrderApi.Application.DTOs;
using OrderApi.Application.DTOs.Conversions;
using OrderApi.Application.Interfaces;
using Polly.Registry;
using System.Net.Http.Json;

namespace OrderApi.Application.Services
{
    public class OrderService(HttpClient httpClient, IOrder orderInterface,
        ResiliencePipelineProvider<string> resiliencePipeline) : IOrderService
    {

        // GET PRODUCT
        public async Task<ProductDTO> GetProduct(int productId)
        {
            // Call Product Api using HttpClient
            // Redirect this call to the API Gateway since Product Api is not response to outsider.
            var getProduct = await httpClient.GetAsync($"/api/Products/{productId}");
            if (!getProduct.IsSuccessStatusCode)
                return null!;

            // Deserialize response
            var product = await getProduct.Content.ReadFromJsonAsync<ProductDTO>();
            if (product == null)
            {
                Console.WriteLine("Product deserialization failed.");
                return null!;
            }

            return product;
        }


        // GET USER
        public async Task<AppUserDTO> GetUser(int userId)
        {
            // Call Product Api using HttpClient
            // Redirect this call to the API Gateway since Product Api is not response to outsider.
            var getUser = await httpClient.GetAsync($"/api/Authentication/{userId}");
            if (!getUser.IsSuccessStatusCode)
                return null!;

            var product = await getUser.Content.ReadFromJsonAsync<AppUserDTO>();
            return product!;
        }


        // GET ORDER DETAILS BY ID
        public async Task<OrderDetailsDTO> GetOrderDetails(int orderId)
        {
            // Prepare Order
            var order = await orderInterface.FindByIdAsync(orderId);
            if(order is null || order!.Id <= 0)
                return null!;

            // Get Retry pipeline
            var retryPipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");

            // Prepare product
            var productDTO = await retryPipeline.ExecuteAsync(async token => await GetProduct(order.ProductId));

            // Prepare client
            var appUserDTO = await retryPipeline.ExecuteAsync(async token => await GetUser(order.ClientId));
            
            // Populate order details
            return new OrderDetailsDTO(
                order.Id,
                productDTO.Id,
                appUserDTO.Id, 
                productDTO.Name,
                appUserDTO.Email,
                appUserDTO.Address,
                appUserDTO.TelephoneNumber,
                productDTO.Name, 
                order.PurchaseQuantity,
                productDTO.Price,
                productDTO.Quantity * order.PurchaseQuantity, 
                order.OrderedDate);
        }


        // GET ORDERS BY CLIENT ID
        public async Task<IEnumerable<OrderDTO>> GetOrdersByClientId(int clientId)
        {
            // Get all client's orders
            var orders = await orderInterface.GetOrdersAsync(o => o.ClientId == clientId);
            if (!orders.Any()) return null!;

            // Convert from entity to DTO
            var (_, _orders) = OrderConversion.FromEntity(null, orders);
            return _orders!;
        }
    }
}
