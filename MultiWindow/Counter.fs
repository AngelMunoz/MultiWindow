namespace MultiWindow

open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts

module Counter =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout

    type State = { count: int }
    let init = { count = 0 }

    type Msg =
        | Increment
        | Decrement
        | Reset

    let update (msg: Msg) (state: State): State =
        match msg with
        | Increment ->
            let count = state.count + 1
            Events.PublishInt count
            { state with count = count }
        | Decrement ->
            let count = state.count - 1
            Events.PublishInt count
            { state with count = count }
        | Reset ->
            Events.PublishInt init.count
            init

    let view (state: State) (dispatch) =
        DockPanel.create [
            DockPanel.verticalAlignment VerticalAlignment.Center
            DockPanel.horizontalAlignment HorizontalAlignment.Center
            DockPanel.children [
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> dispatch Reset)
                    Button.content "reset"
                ]
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> dispatch Decrement)
                    Button.content "-"
                ]
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> dispatch Increment)
                    Button.content "+"
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.fontSize 48.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text (string state.count)
                ]
            ]
        ]

type CounterWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Counter WIndow"
        base.Width <- 400.0
        base.Height <- 400.0

        Elmish.Program.mkSimple (fun () -> Counter.init) Counter.update Counter.view
        |> Program.withHost this
        |> Program.run
