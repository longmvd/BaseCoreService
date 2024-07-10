using BaseCoreService.Extensions;
using BaseCoreService.DL;
using BaseCoreService.BL;
using BaseCoreService.Authen;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCustomJwtAuthentication(builder.Configuration);
DLStartupImport.Intit(builder.Services, builder.Configuration);
BLStartupImport.Intit(builder.Services, builder.Configuration);
AuthenStartupImport.Init(builder.Services, builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
