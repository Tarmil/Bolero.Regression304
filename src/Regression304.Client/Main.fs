module Regression304.Client.Main

open Bolero.Remoting
open Elmish
open Bolero
open Bolero.Html
open Bolero.Templating.Client

type Mock = { title: string; pattern: string }

type IMockInfoService =
    {
        getMocks: unit -> Async<Mock list>
    }
    interface IRemoteService with
        member this.BasePath = "/mock"

type Model =
    {
        x: string
    }

let initModel =
    {
        x = ""
    }

type Message =
    | Ping
    | GotMocks of Mock list

let update message model =
    match message with
    | Ping -> model, Cmd.none
    | GotMocks mocks ->
        printfn $"{mocks}"
        model, Cmd.none

let view model dispatch =
    p { "Hello, world!" }

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let rem = this.Remote<IMockInfoService>()
        Program.mkProgram
            (fun _ ->
                initModel, Cmd.OfAsync.perform rem.getMocks () GotMocks)
            update view
#if DEBUG
        |> Program.withHotReload
#endif
