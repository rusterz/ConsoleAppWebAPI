using WebAPI.DI_container;

var services = new DiServiceCollection();
// services.RegisterTransient<IUserService, UserService>();  
// services.RegisterSingleton<IUserRepository, UserRepository>(); 

var container = services.GenerateContainer();
