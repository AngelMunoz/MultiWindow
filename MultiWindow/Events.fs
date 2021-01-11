namespace MultiWindow

module Events =
    let private intEvent = Event<int>()
    let private todoEvent = Event<string list>()

    let IntEvent = intEvent.Publish
    let PublishInt number = intEvent.Trigger(number)
    
    let TodoEvent = todoEvent.Publish
    let PublishTodos number = todoEvent.Trigger(number)
