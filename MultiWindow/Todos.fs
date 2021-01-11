namespace MultiWindow

open Avalonia.Controls
open Avalonia.Controls
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts

module Todos =
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open Avalonia.Layout

    type State =
        { todos: string list
          currentTodo: string }

    let init = { todos = []; currentTodo = "" }

    type Msg =
        | AddTodo
        | RemoveTodo of string
        | UpdateCurrent of string


    let update (msg: Msg) (state: State): State =
        match msg with
        | UpdateCurrent current -> { state with currentTodo = current }
        | AddTodo ->
            let todos = state.currentTodo :: state.todos
            Events.PublishTodos(todos)
            { state with
                  todos = todos }
        | RemoveTodo name ->
            let todos =
                  state.todos
                  |> List.filter (fun tname -> name <> tname)
            Events.PublishTodos(todos)
            { state with
                  todos = todos }

    let view (state: State) (dispatch) =
        DockPanel.create [
            DockPanel.verticalAlignment VerticalAlignment.Center
            DockPanel.horizontalAlignment HorizontalAlignment.Center
            DockPanel.children [
                TextBox.create [
                    TextBox.dock Dock.Top
                    TextBox.onTextChanged (fun text -> dispatch (UpdateCurrent text))
                ]
                Button.create [
                    Button.dock Dock.Top
                    Button.content "Save"
                    Button.onClick (fun _ -> dispatch AddTodo)
                ]
                ListBox.create [
                    ListBox.dock Dock.Bottom
                    ListBox.viewItems [
                        for todo in state.todos do
                            StackPanel.create [
                                StackPanel.orientation Orientation.Horizontal
                                StackPanel.horizontalAlignment HorizontalAlignment.Stretch
                                StackPanel.spacing 12.
                                StackPanel.children [
                                    TextBlock.create [
                                        TextBlock.horizontalAlignment HorizontalAlignment.Left
                                        TextBlock.text todo
                                    ]
                                    Button.create [
                                        Button.horizontalAlignment HorizontalAlignment.Right
                                        Button.content "Delete 🧺"
                                        Button.onClick (fun _ -> dispatch (RemoveTodo todo))
                                    ]
                                ]
                            ]
                    ]
                ]

            ]

        ]

type TodosWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Todos Window"
        base.Width <- 400.0
        base.Height <- 400.0

        Elmish.Program.mkSimple (fun () -> Todos.init) Todos.update Todos.view
        |> Program.withHost this
        |> Program.run
