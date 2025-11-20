using DashboardServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS設定（オフライン環境での同一ネットワーク内通信用）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// サービス登録
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

// データベース初期化
using (var scope = app.Services.CreateScope())
{
    var dashboardService = scope.ServiceProvider.GetRequiredService<DashboardService>();
    await dashboardService.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 静的ファイル提供（wwwroot）
app.UseStaticFiles();
app.UseDefaultFiles();

app.UseCors("AllowAll");

app.MapControllers();

// デフォルトルート（wwwroot/index.htmlにリダイレクト）
app.MapFallbackToFile("index.html");

app.Run();
