using chatApp.Hubs;
using chatApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddSingleton<IKafkaTopicInitializer, KafkaTopicInitializer>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var topicInit = scope.ServiceProvider.GetRequiredService<IKafkaTopicInitializer>();
    await topicInit.EnsureTopicsExistAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.Run();
