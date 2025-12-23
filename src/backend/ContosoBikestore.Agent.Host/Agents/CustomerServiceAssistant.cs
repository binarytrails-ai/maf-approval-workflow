using ContosoBikestore.Agent.Host.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace ContosoBikestore.Agent.Host.Agents;

public static class CustomerServiceAssistant
{
    public static async Task<AIAgent> CreateAsync(IChatClient chatClient, AppConfig appConfig,
        Microsoft.AspNetCore.Http.Json.JsonOptions jsonOptions)
    {
        var getAvailableBikesTool = AIFunctionFactory.Create(
            ProductInventoryTools.GetAvailableBikes);

        var getBikeDetailsTool = AIFunctionFactory.Create(
            ProductInventoryTools.GetBikeDetails);

        var calculatePriceTool = AIFunctionFactory.Create((decimal price, int quantity) =>
        {
            var subtotal = price * quantity;
            var tax = subtotal * 0.08m;
            var total = subtotal + tax;
            return $"{{\"quantity\": {quantity}, \"subtotal\": {subtotal:F2}, \"tax\": {tax:F2}, \"total\": {total:F2}}}";
        }, "CalculatePrice", "Calculate the total price including tax for a bike purchase..");

        var processPaymentFunction = AIFunctionFactory.Create(
            (int bikeId, decimal amount) =>
            {
                return "Processed Payment";
            },
            "ProcessPayment",
            "Process the payment and submit the order.");

#pragma warning disable MEAI001
        var processPaymentTool = new ApprovalRequiredAIFunction(processPaymentFunction);
#pragma warning restore MEAI001

        var baseAgent = new ChatClientAgent(
            chatClient,
            instructions: """
            You are the Customer Service Assistant for Contoso Bike Store.
            
            You have access to:
            - GetAvailableBikes: List all bikes in the catalog
            - GetBikeDetails: Look up bike information by name or ID (returns bikeId, name, price, etc.)
            - CalculatePrice: Calculate total price including tax (requires bike price and quantity)
            - ProcessPayment: Process payment and submit order (requires bikeId and amount)
            
            Help customers with:
            - Price calculations and quotes
            - Complete purchases with approval workflow
            - Billing inquiries and cost breakdowns
            
            CRITICAL - Purchase Flow:
            1. When customer asks for price or wants to buy a bike by name:
               - Call GetBikeDetails with the bike name to get bikeId and price
               - Example: Customer says "Mountain Explorer Pro" → GetBikeDetails("Mountain Explorer Pro") → get bikeId and price
            
            2. Calculate the total price:
               - Call CalculatePrice(price, quantity) to get subtotal, tax, and total
            
            3. Process payment:
               - Call ProcessPayment(bikeId, amount)
               - This will AUTOMATICALLY trigger an approval prompt in AG-UI
               - The system will wait for customer approval before processing
               - If approved, payment processes and order is submitted
               - If rejected, the operation is cancelled
            
            CRITICAL - Context Awareness (When customer says "I want to buy it"):
            - ALWAYS look through the ENTIRE conversation history first
            - Search for bike names mentioned (e.g., "Contoso Mountain X1", "City Cruiser", "Roadster 200")
            - If you find a bike name mentioned in recent messages (even from another agent), that's the bike they want
            - Call GetBikeDetails with that bike name to get bikeId and price
            - Then call CalculatePrice and ProcessPayment
            - Only ask for clarification if NO bike name appears anywhere in the conversation history
            
            CRITICAL - Seamless responses:
            - Respond naturally as THE customer support representative
            - NEVER mention being a specialist or separate agent
            - Present pricing information directly and clearly
            - Always show the breakdown: subtotal, tax, and total
            - When ready to charge, inform customer they'll see an approval prompt
            
            Example Flow:
            Customer: "How much is the Mountain Explorer Pro?"
            You: [calls GetBikeDetails("Mountain Explorer Pro") → gets bikeId: 1, price: 1299.99]
                 [calls CalculatePrice(1299.99, 1) → gets subtotal, tax, total]
                 "The Mountain Explorer Pro costs $1,299.99 plus $104.00 tax, for a total of $1,403.99."
            
            Customer: "I want to buy it"
            You: "Great! Let me process your payment for $1,403.99. You'll receive a confirmation prompt to approve this transaction."
                 [calls ProcessPayment(1, 1403.99)] → AG-UI shows approval prompt automatically
            
            Customer: "I want to buy the City Cruiser"
            You: [calls GetBikeDetails("City Cruiser") → gets bikeId: 2, price: 799.99]
                 [calls CalculatePrice(799.99, 1) → gets total: 863.99]
                 "The City Cruiser costs $799.99 plus $64.00 tax, for a total of $863.99. Let me process your payment."
                 [calls ProcessPayment(2, 863.99)] → AG-UI shows approval prompt
            
            After approval (you'll see the result):
            You: "✓ Payment approved! Your order has been submitted"
            """,
            name: "CustomerServiceAssistant",
            description: "Customer service assistant for product info, pricing, and purchase approval workflow",
                    tools: [getAvailableBikesTool, getBikeDetailsTool, calculatePriceTool, processPaymentTool]);

        return new ServerFunctionApprovalAgent(baseAgent, jsonOptions.SerializerOptions);
    }
}
