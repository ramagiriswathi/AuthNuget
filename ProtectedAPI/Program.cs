using AuthPackage.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace ProtectedAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //     .AddFdcAuth("https://localhost:44369/signing-keys");
            builder.Services.AddAuthentication("FdcCustomerAuth").AddFdcJwtBearer(options =>
            {
                options.SigningKeysUri = new Uri("https://localhost:44369/signing-keys");            
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }            
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
