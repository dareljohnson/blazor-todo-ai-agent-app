using BlazorAiAgentTodo.Services;
using BlazorAiAgentTodo.Services.Interfaces;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register Radzen services
builder.Services.AddRadzenComponents();

// Register application services
builder.Services.AddSingleton<ITodoService, TodoService>();
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<IAgentService, AgentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

