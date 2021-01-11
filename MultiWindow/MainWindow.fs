namespace MultiWindow

open System
open System.Threading.Tasks
open Avalonia.Threading
open FSharp.Control.Tasks
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts


type WindowKind =
    | Counter
    | Todos

// If you intend to share elmish code then try to other platforms
// or implementations try to use an interface so you are not tied
// to the Avalonia's Window Type
type IWindowService =
    abstract OpenDialog<'T> : WindowKind -> Task<'T>


module MainWindow =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout

    type State = { number: int; todos: string list }
    let init () = { number = 0; todos = [] }, Cmd.none

    type Msg =
        | OpenCounter
        | OpenTodos
        | WindowClosed
        | IntSent of int
        | TodoSent of string list

    // Subscribe to any events that you may want
    // this time we're using .NET events to communicate between windows/modals
    module Subs =
        let intSent _ =
            let sub dispatch =
                Events.IntEvent.Subscribe(fun value -> dispatch (IntSent value))
                |> ignore

            Cmd.ofSub sub
        let todosSent _ =
            let sub dispatch =
                Events.TodoEvent.Subscribe(fun value -> dispatch (TodoSent value))
                |> ignore

            Cmd.ofSub sub

    let update (msg: Msg) (state: State) (windowService: IWindowService): State * Cmd<Msg> =
        match msg with
        | OpenCounter -> state, Cmd.OfTask.perform windowService.OpenDialog Counter (fun () -> WindowClosed)
        | OpenTodos -> state, Cmd.OfTask.perform windowService.OpenDialog Todos (fun () -> WindowClosed)
        | WindowClosed ->
            printfn "Window closed"
            state, Cmd.none
        | IntSent number ->
            { state with number = number }, Cmd.none
        | TodoSent todos ->
            { state with todos = todos }, Cmd.none
           
            

    let view (state: State) (dispatch) =
        DockPanel.create [
            DockPanel.horizontalAlignment HorizontalAlignment.Center
            DockPanel.verticalAlignment VerticalAlignment.Center

            DockPanel.children [
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.text (sprintf "Current ChildCount %i" state.number)
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.text (sprintf "Current Todos:" )
                ]
                StackPanel.create [
                    StackPanel.dock Dock.Top
                    StackPanel.spacing 12.0
                    StackPanel.children [
                        for todo in state.todos do
                            TextBlock.create [
                                TextBlock.text todo
                            ]
                    ]
                ]
                Button.create [
                    Button.dock Dock.Left
                    Button.onClick (fun _ -> dispatch OpenCounter)
                    Button.content "Open Counter"
                ]
                Button.create [
                    Button.dock Dock.Right
                    Button.onClick (fun _ -> dispatch OpenTodos)
                    Button.content "Open Todos"
                ]
            ]
        ]

type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Main Window"
        base.Width <- 400.0
        base.Height <- 400.0

        let update state msg =
            MainWindow.update state msg (this :> IWindowService)

        Elmish.Program.mkProgram MainWindow.init update MainWindow.view
        |> Program.withHost this
        |> Program.withSubscription MainWindow.Subs.intSent
        |> Program.withSubscription MainWindow.Subs.todosSent
        |> Program.run

    // implement your IWindowService
    interface IWindowService with
        member _.OpenDialog<'T>(kind: WindowKind) =
            match kind with
            | Counter ->
                Dispatcher.UIThread.InvokeAsync<'T>(fun _ ->
                    let window = CounterWindow()
                    window.ShowDialog<'T> this)
            | Todos ->
                Dispatcher.UIThread.InvokeAsync<'T>(fun _ ->
                    let window = TodosWindow()
                    window.ShowDialog<'T> this)
