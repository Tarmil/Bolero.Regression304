namespace Regression304.Server

open System
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero
open Bolero.Remoting.Server
open Bolero.Server
open Regression304
open Bolero.Templating.Server
open Regression304.Client.Main

type AppSettings =
    {
        MockSettings: {| Request: {| Methods: string list; PathMatchesRegex: string |} |} list
    }

type MockInfoService(
    ctx: IRemoteContext,
    env: IWebHostEnvironment,
    appSettings : AppSettings ) =
    inherit RemoteHandler<IMockInfoService>()
    
    // let configuredMocks gives the same error if we want to use a readonly private member instead...
    member val ConfiguredMocks =
        [
            for mockSetting in appSettings.MockSettings do
                
                let methods = 
                    (",", mockSetting.Request.Methods) 
                    |> String.Join

                { title = methods; pattern = mockSetting.Request.PathMatchesRegex }
        ]

    override this.Handler =
        {
            getMocks = fun () -> async {
                return this.ConfiguredMocks
            }
        }

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc() |> ignore
        services.AddServerSideBlazor() |> ignore
        services.AddSingleton({ MockSettings = [{| Request = {| Methods = [""]; PathMatchesRegex = "" |} |}] }) |> ignore
        services
            .AddBoleroHost()
            .AddBoleroRemoting<MockInfoService>()
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../Regression304.Client")
#endif
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment() then
            app.UseWebAssemblyDebugging()

        app
            .UseAuthentication()
            .UseStaticFiles()
            .UseRouting()
            .UseAuthorization()
            .UseBlazorFrameworkFiles()
            .UseEndpoints(fun endpoints ->
#if DEBUG
                endpoints.UseHotReload()
#endif
                endpoints.MapBoleroRemoting() |> ignore
                endpoints.MapBlazorHub() |> ignore
                endpoints.MapFallbackToBolero(Index.page) |> ignore)
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStaticWebAssets()
            .UseStartup<Startup>()
            .Build()
            .Run()
        0
