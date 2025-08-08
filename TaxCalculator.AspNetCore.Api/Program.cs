using TaxCalculator.Data.Interfaces;
using TaxCalculator.Data.Repositories;
using TaxCalculator.Services.Interfaces;
using TaxCalculator.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Use Newtonsoft.Json for exact compatibility

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=TaxCalculatorDB;Trusted_Connection=true;";

// Register dependencies (migrated from AutofacConfig)
builder.Services.AddSingleton<IConnectionFactory>(provider => 
    new SqlConnectionFactory(connectionString));

// Register repositories
builder.Services.AddScoped<ITaxBracketRepository, TaxBracketRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserIncomeRepository, UserIncomeRepository>();

// Register services
builder.Services.AddScoped<ITaxCalculationService, TaxCalculationService>();
builder.Services.AddScoped<IUserTaxService, UserTaxService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<TaxCalculator.Services.Interfaces.ILogger, SimpleLogger>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { } // Make Program accessible for testing
