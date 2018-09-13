# Web-Api-Core-Example

Vamos a crear un proyecto de Web Api Core paso a paso.

## 1) Primer paso será crear la carpeta y luego el proyecto de Web Api Core
- mkdir WAC.WebAPI
- cd WAC.WebAPI
- dotnet new webapi -au None --no-https

## 2) Abrimos la carpeta con Visual Studio Code
- cd ..
- start .
- Open With Visual Studio Code
- Añadimos las Extensiones de C# a WAC.WebApi cuando nos pregunte

## 3) Crearemos un espacio de trabajo o workspace en VSCode
- File/Save Workspace As... WAC

## 4) Crearemos un proyecto de Test para la WebAPI
- mkdir WAC.WebAPI.Tests
- dotnet new mstest
- Agregamos la referencia desde WAC.WebAPI.Tests a WAC.WebAPI
```
    dotnet add WAC.WebAPI.Tests/WAC.WebAPI.Tests.csproj reference WAC.WebAPI/WAC.WebAPI.csproj
```

## 5) Vamos a crear un Test para crear un Recurso Usuario 
- Definir el test unitario
- Crear una implementación básica 
- Creamos un UserCreationTest y definimos un primer método de Test
```C#
	[TestMethod]
	public void CreateValidUserTest()
	{
			//Arrange
            var modelIn = new UserModelIn() { Username = "Alberto", Password = "pass", Age = 2 }; 
            var controller = new UsersController();
            var result = controller.Post(modelIn);

            //Act
            var createdResult = result as CreatedAtRouteResult;
            var modelOut = createdResult.Value as UserModelOut;

            //Assert
            Assert.IsNotNull(createdResult);
            Assert.AreEqual("GetById", createdResult.RouteName);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(modelIn.Username, modelOut.Username);
	}
```
- Agreguemos a el proyecto de tests la referencia a AspNetCore.Mvc.Core
```
	cd  WAC.WebAPI.Test
	dotnet add package  Microsoft.AspNetCore.Mvc.Core
```

## 6) Vamos a crear una implementación básica para que la prueba pase.
- Primero debemos crear un controller WebAPI, UsersController, podemos ya hacerlo con las operaciones por defecto. Luego si no usamos alguna podemos borrarlas.
- Cambiar los tipos de retorno a IActionResult
- Creamos una carpeta Models dentro del proyecto de WebAPI. Las clases dentro de está carpeta suelen llevar el namespace 
```C#
	namespace WAC.WebAPI.Models { }
```
- Estos modelos son las represetanción de nuestros recursos para los clientes de la API REST. El cliente hablará con la WebAPI REST por medio de estos modelos. 
**¿Podríamos usar las clases del dominio del negocio para esto?**
Poder podemos, pero estamos dando un motivo de cambio nuevo al dominio, esto va contra SRP. Las clases del dominio no deberían cambiar por motivos referentes a la implementación de REST ni por la comunicación cliente-servidor. Otra ventaja extra es que podemos tener distintas representaciones del recurso dependiendo de la petición (ej: in y out), o podemos ocultar cierta información (UserModelOut sin campo Password), entre otros. Sin embargo, debemos considerar que de cambiar el negocio y las clases del dominio, esto genera seguramente genere un impacto en los modelos.
- Crear los modelos UserModelIn y UserModelOut 
- Implementaremos la operación POST de forma muy básica en el controller utilizando los modelos creados.
	- Usaremos el Attribute [HttpPost] sobre el método Post (De forma similar existen HttpGet, HttpPut, HttpDelete, HttpPatch)
	- Recibiremos por parámetros desde el Body HTTP un UserModelIn. Para eso definiremos el método Post de la siguiente manera:
	```C#
		...Post([FromBody] UserModelIn user) {}
	```
	- Podemos usar [FromUri] [FromQuery] (entre otros) dependiendo de cómo queremos que el cliente envíe la información. 
	En caso de ser en la URL, específicamente en la URI, podemos usar el primero:
	```
		api/Users/{id} => api/Users/1
		//También podemos especificar condiciones
		api/Users/{id:min(18)}
	```
	- Si queremos que envíen ciertos parámetros como query "URI?param=1&param=2" podemos usar el segundo.
	- Usaremos para post un método ofrecido por ControllerBase llamado CreatedAtRoute:
	
	```C#
		return CreatedAtRoute("GetById", new { id = addedUser.Id }, addedUser);
	```
	
	- El primer parámetro referencia al Attribute del método get por id [HttpGet], como ven el mismo está definido de la siguiente forma:

	```C#
		[HttpGet("{id}", Name="GetById")]
	```
		
	- El segundo parámetro es el Id a colocar en la ruta del Get
	- El tercer parámetro es el objeto a devolver en el Body de la Response Http, UserModelOut.
	Utilizar CreatedAtRoute de esta forma, hará que la respuesta tenga un Header llamado **Location**, el cual tendrá el valor con la URL del Get referenciado. De esta forma sabemos como acceder al recurso una vez creado. 

## 7) Correr el test.
- Para correr los test unitarios, basta con pararnos sobre el proyecto de test y ejecutarlo:
	```
		cd WAC.WebAPI.Tests
		dotnet test
	```
- Opcionalmente pueden instalarse la extensión **.NET Core Test Explorer** o alguna similar.

## 8) Validation Attributes
- Podemos querer poner validaciones en nuestros modelos de tal forma de restringuir y determinar si una Request es válida o no. Para ello podemos hacer uso de Attributes sobre las propiedades de las clases Model. Luego necesitaremos checkear si el modelo es válido o no. Al UserModelIn le pondremos las siguientes restricciones:
	```C#
	    [Required]
        public string Username {get; set;}
        
        [Required]
        public string Password {get; set;}

        [Required]
        public int Age {get; set;}
	```
- Crearemos un test para probar obtener un BadRequest y luego crearemos la implementación. Veremos que de esta forma no podemos probar realmente los Attributes pero si como se comporta el controller.

## 9) Creación de Application Layer // Business Layer
Vamos a reescribir nuestro post para que delegue a la lógica de negocio el resto del procesamiento de la petición. Terminaremos teniendo:
- Un proyecto classlib que contendrá las clases del dominio, que representaran a las entidades del negocio y sus restricciones.
- Un proyecto classlib con clases servicio que actuarán como fachada para el Application Layer y coordinará la ejecución de la petición/funcionalidad.
- Creamos entonces los siguientes proyectos:
	```
		mkdir WAC.Application.Users.Tests
		cd WAC.Application.Users.Tests
		dotnet new mstest
		cd ..
	```
- Pasaremos a escribir una primer prueba unitaria para crear un Usuario. Algo que nos puede ayudar es pensar esta prueba como si la misma fuera el método Post del UsersController. Diseñaremos esta prueba de la forma que conocemos hasta ahora, y luego la diseñaremos utilizando MOQ.
	```C#
		private IUserService userService;

        [TestInitialize]
        public void SetUp()
        {
            user = GetUser();
            userService = new UserService(t);
        }

        private User GetUser()
        {
            var user = new User("User1", "Pass", 23);
            return user;
        }

        [TestMethod]
        public void SignUpTest()
        {
            userService.SignUp(user);
            Assert.AreEquals(user, userService.Get(user));
        }
	```
- Tomaremos un shortcut en el proceso y crearemos 3 proyectos para poder implementar la prueba. 
	- El primero será **WAC.Contracts.Application.Users** donde colocaremos las interfaces como IUserService, las cuales son requeridas por quien quiera usar el Application Layer y serán implementadas por dicha capa.
	- El segundo proyecto será **WAC.Application.Users** donde colocaremos las implementaciones de los contratos anteriores.
	- Por último crearemos un proyecto para el dominio de usuarios **WAC.Domain.Users**. Aquí pondremos las entidades del negocio.
	- Las implementaciones son básicas, solo buscamos pasar el Test. Luego la idea es repetir la línea de pensamiento hasta que cumplamos con la funcionalidad solicitada.
	- Las clases del dominio deberían haber sido creadas con pruebas específicas, en este momento nuestra prueba está probando tanto la implementación de UserService como la implementación de User. 

## 10) Conectando la WebAPI REST con Application Layer

- Primero cambiaremos la implementación del método POST para que use **UserService/SignUp**.
- Luego veremos como conectar el controller de WebApi con el UserService. Lo que queremos conseguir es evitar la dependecia generada por el new UserService en el controller. Para esto usaremos la Invesión del Control provista por .NET Core, la cual funciona gracias a la inyección de dependencias. Cuando el Controller se instancie para atender una petición, sus dependencias serán inyectadas por .NET sin que tengamos que hacer new. 
- Este mismo principio puede ser utilizado para instanciar un único contexto de EntityFramework por cada petición. Trabajando de manera conectada durante el tiempo que demora en procesarse una petición. 
- En el siguiente paso veremos como configurar IOC.

## 11) Configurando IOC

- Sin hacerlo explícito, ya hemos desarrollado este ejemplo para que el IOC y la inyección de dependencias sea muy sencilla de hacer.
- El primer paso será cambiar el constructor del controller a:
	
	```C#
		
        public UsersController(IUserService aUserService) {
            userService = aUserService;
        }
	```
- El siguiente paso es arreglar la prueba unitaria, cómo no hemos visto Mocking aún. Debemos instanciar manualmente la dependencia para la prueba.
	
	```C#
		 var controller = new UsersController(new UserService());
	```
	
- El último paso será configurar el IOC en el Startup.cs del proyecto de WebAPI, en el método ConfigureServices:
	
	```C#
		public void ConfigureServices(IServiceCollection services)
        	{
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped<IUserService, UserService>();
		}
	```
- De forma similar podemos configurar el Contexto de EF. Ver en Startup.cs

## 12) Entity Framework Core
Antes de proceder vamos a detenernos a ver como utilizar EF Core en conjunto con IoC.
**NOTA: Recomendamos invesitar Unit of Work y Repository Pattern, sin embargo no lo implementaremos en este ejemplo**
- Primero tenemos que crearnos un proyecto classlib para el acceso a datos. 
	```
		mkdir WAC.DataAccess
		dotnet new classlib
	```
- Agregaremos al proyecto nuevo EFCore
	```
		cd WAC.DataAccess
		dotnet add package Microsoft.EntityFrameworkCore
	```
- Agregamos la dependencia WAC.Domain.Users
	```
		dotnet add WAC.DataAccess/WAC.DataAccess reference WAC.Domain.Users/WAC.Domain.Users
	```
 - Luego crearemos la clase Contexto. 
	La misma va a cambiar un poco respecto a EF6 de .NET Framework. El contructor recibirá un objecto **DbContextOptions** que es el objeto que crearemos en el Startup.cs de la WebAPI. De esta forma podremos pasarle el ConnectionString.
	Tambíen podemos usar FluentAPI para configurar las relaciones, claves, mapeos etc. La sintaxis difiere un poco de EF6 de .NET Framework.
 	```C#
		public class DomainContext : DbContext
    	{
        	public DbSet<User> Users { get; set; }

        	public DomainContext(DbContextOptions<DomainContext> options) : base(options)
       		{
        	}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd();
		}
	```
- Ahora vamos a la WebAPI a configurar el Startup.cs. De esta forma por cada request, el framework nos proveerá de una instancia del contexto.
Usaremos EF en memoria para el desarrollo.
	```C#
		services.AddDbContext<DomainContext>(options => options.UseInMemoryDatabase(Configuration.GetConnectionString("WACDatabase")));
	```
- Para que esta línea funcione tenemos que agregar la referencia al WAC.DataAccess desde WAC.WebAPI
	```
		dotnet add WAC.WebAPI/WAC.WebAPI.csproj reference WAC.DataAccess/WAC.DataAccess.csproj
	```
	También debemos agregar el using al namespace del Context nuestro.
- Por último debemos agregar el paquete de EFCore.InMemory para simular una base de datos no relacional en memoria. Parados sobre la carpeta de WAC.WebAPI:
	```
		dotnet add package Microsoft.EntityFrameworkCore.InMemory
	```
	También agregamos el using al namespace del paquete.
- Ahora tenemos que configurar nuestro ConnectionString. Para eso vamos a los archivos de WebAPI **appsettings.Development.json** y **appsettings.json** y agregamos la siguientes entradas:
En caso de usar EF InMemory, en el primer archivo:

	```json
		"ConnectionStrings": {
			"WACDatabase":"Server=(localdb)\\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0;"
		}
	```
En caso de usar una base de datos real, por ejemplo en producción o en la defensa:
	
		"ConnectionStrings": {
			"WACDatabase": "Server=./SQLServer_R14;Database=WACDatabase;Trusted_Connection=True;Integrated Security=True;"
		}
- Buildeamos el proyecto
	```
		dotnet build
	```
- EF Core no cuenta con migraciones automáticas. **Si usamos una base de datos realacional SQL** (!InMemory) entonces debemos crear las migraciones. Hay varias formas de hacer esto, pararnos sobre el proyecto del Contexto y referenciar al de arranque con los .json de configuración. O lo inverso.
	```
		cd WAC.DataAccess
		dotnet ef migrations add MyFirstMigration --startup-project=../WAC.WebAPI/WAC.WebAPI.csproj
	```
	Luego de generar la migración debemos impactarla en la base usando
	```
		dotnet ef database update
	```
 - Y por último veremos como usarlo. Para ello diseñaremos a partir de UserService un objeto que nos sirva de acceso a datos.

## Próximamente:
- En los próximos commits veremos como seguir con nuestra aplicación.
	- Mocking con MOQ
	- Autenticación con JWT
	

	
