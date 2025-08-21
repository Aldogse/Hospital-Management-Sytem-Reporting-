using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.Repository;
using Serilog;
Log.Logger = new LoggerConfiguration()
      .WriteTo.Console()
      .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
      .Enrich.FromLogContext()
      .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<ReportDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("MainDb"),
        new MySqlServerVersion(new Version(8, 0, 36)));
});
builder.Services.AddScoped<IhrLeaveRepository,hrLeaveRepository>();
builder.Services.AddScoped<IhrPayrollRepository, hrPayrollRepository>();
builder.Services.AddScoped<IhrEmployeeInformation, hrEmployeeInformation>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//CORS Configuration

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();

