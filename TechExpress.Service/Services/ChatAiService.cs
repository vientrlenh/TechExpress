using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using TechExpress.Repository.Models;

namespace TechExpress.Service.Services;

public class ChatAiService(AnthropicClient client, string systemPrompt)
{
    private readonly AnthropicClient _client = client;
    private readonly string _systemPrompt = systemPrompt;

    public static IReadOnlyList<ToolUnion> GetTools() =>
    [
        new ToolUnion(new Tool
        {
            Name = "search_products",
            Description = "Search for products in the TechExpress store based on the customer's needs. Use when the customer asks what products are available, wants recommendations based on purpose/budget/brand, or asks about a product category.",
            InputSchema = new InputSchema
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["keywords"] = JsonDocument.Parse("""{"type":"string","description":"Search keywords matching product name or SKU (e.g. 'RTX 4070', 'i7')"}""").RootElement,
                    ["category"] = JsonDocument.Parse("""{"type":"string","description":"Product category name. Use exact names like: CPU, GPU, RAM, Mainboard, SSD, HDD, PSU, Case, Cooler, Laptop, Monitor, Keyboard, Mouse"}""").RootElement,
                    ["brand"] = JsonDocument.Parse("""{"type":"string","description":"Brand name to filter by (e.g. 'AMD', 'Intel', 'ASUS', 'MSI', 'Corsair', 'Samsung', 'WD')"}""").RootElement,
                    ["min_price"] = JsonDocument.Parse("""{"type":"number","description":"Minimum price in VND"}""").RootElement,
                    ["max_price"] = JsonDocument.Parse("""{"type":"number","description":"Maximum price in VND"}""").RootElement
                }
            }
        }, null),

        new ToolUnion(new Tool
        {
            Name = "get_product_detail",
            Description = "Get full details of a specific product including specs, price, stock, warranty, and description. Use when the customer wants to know more about a particular product.",
            InputSchema = new InputSchema
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["product_id"] = JsonDocument.Parse("""{"type":"string","description":"The product UUID obtained from search_products"}""").RootElement
                },
                Required = ["product_id"]
            }
        }, null),

        new ToolUnion(new Tool
        {
            Name = "check_pc_compatibility",
            Description = "Check whether a set of PC components are compatible with each other (socket, RAM type, form factor, power requirements, etc.). Use when the customer wants to build a PC or verify if selected parts work together.",
            InputSchema = new InputSchema
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["components"] = JsonDocument.Parse("""
                    {
                        "type": "array",
                        "description": "List of PC components to validate for compatibility",
                        "items": {
                            "type": "object",
                            "properties": {
                                "product_id": {"type": "string", "description": "The product UUID"},
                                "quantity": {"type": "integer", "description": "Quantity of this component (usually 1, except RAM)"}
                            },
                            "required": ["product_id", "quantity"]
                        }
                    }
                    """).RootElement
                },
                Required = ["components"]
            }
        }, null),
        
        new ToolUnion(new Tool 
        {
            Name = "escalate_to_staff",
            Description = "Escalate this chat session to a human support staff member. Use when the customer has issues with orders, returns, account problems, complaints, or anything you cannot resolve by yourself.",
            InputSchema = new InputSchema 
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["reason"] = JsonDocument.Parse("""{"type":"string","description":"Brief reason for escalation"}""").RootElement
                },
                Required = ["reason"]
            }
        }, null)
    ];

    public List<MessageParam> BuildMessagesFromHistory(List<ChatMessage> history) =>
        [.. history.Select(m => new MessageParam
        {
            Role = m.IsAiMessage ? Role.Assistant : Role.User,
            Content = m.Message
        })];

    public async Task<Message> CallApiAsync(List<MessageParam> messages) =>
        await _client.Messages.Create(new MessageCreateParams
        {
            Model = Model.ClaudeSonnet4_6,
            MaxTokens = 4096,
            System = _systemPrompt,
            Messages = messages,
            Tools = GetTools()
        });
}
