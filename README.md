# MultiWindow

This is a sample on how you can show dialog windows in Avalonia.FuncUI and have them communicate via .NET events and Elmish Subscriptions


## MainWindow

Inside Main window we declare an IWindowInterface, this is to avoid coupling the service with an Avalonia window, in case you want to share the elmish code with another implementation or code base

```fsharp
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
```
in our update function we add as a third parameter the service we want to work with (IWindowService in this case)
```fsharp
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
```

### Inter-Window communication
> ***DISCLAIMER***: I'm not sure this is a good approach, or if there are concurrency issues with this but that's *A* way I could think of resolving this problem, feel free to let me know if there are better approaches.

Let's say you want to communicate between windows, there's no built-in way to do that via elmish, since Elmish was made for the browser where the `window` concept doesn't exist, for that we'll go for a PubSub approach using .NET events


```fsharp
namespace MultiWindow

module Events =
    let private intEvent = Event<int>()
    let private todoEvent = Event<string list>()

    let IntEvent = intEvent.Publish
    let PublishInt number = intEvent.Trigger(number)
    
    let TodoEvent = todoEvent.Publish
    let PublishTodos number = todoEvent.Trigger(number)

```
In one of the child windows we just need to call the Publish function
```fsharp
let update (msg: Msg) (state: State): State =
    match msg with
    | UpdateCurrent current -> { state with currentTodo = current }
    | AddTodo ->
        let todos = state.currentTodo :: state.todos
        Events.PublishTodos(todos) // <-- this one right here
        { state with
              todos = todos }
    | RemoveTodo name ->
        let todos =
              state.todos
              |> List.filter (fun tname -> name <> tname)
        Events.PublishTodos(todos) // <-- this one right here
        { state with
              todos = todos }

```

```fsharp
let update (msg: Msg) (state: State): State =
    match msg with
    | Increment ->
        let count = state.count + 1
        Events.PublishInt count // <- this one right here
        { state with count = count }
    | Decrement ->
        let count = state.count - 1
        Events.PublishInt count // <- this one right here
        { state with count = count }
    | Reset ->
        Events.PublishInt init.count // <- this one right here
        init
```

In the `MainWindow` we can create a `Subscriptions` module that handles the subscriptions

```fsharp
module MainWindow = 
    (* ... *)
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
```
In our MainWindow we just need to subscibe to those

```fsharp
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

```
