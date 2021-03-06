# Idiot Gui - For us Plebs
**This project is under active development and is not stable**

Idiot Gui is a response to the innumerable number of over-engineered "*abstraction layer built on 10 other abstraction layers with 39 layers of indirection to decouple your MVVM/MVC/... running in 8 different processes with support for all modern microwaves*" type GUIs that are taking over the world. Idiot Gui is fast, simple and (mostly) unopinionated.

## Designed to:
- Require zero changes for cross-platform support (Windows, Mac, Linux)
- Have very low input latency and be fast (I'm looking at you Chromium/Electron, you monster)
- Support applications with hundreds of simultaneously moving elements (I'm still looking at you Chromium/Electron!!)
- Support a lot of the 'good parts' of [HTML's Flexbox](https://css-tricks.com/snippets/css/a-guide-to-flexbox/) layout, without the HTML/CSS part
- Take advantage of the beautiful C# language (why write your GUI in a markup language when C# is so pretty?)

## Not designed to:
- Support everything including the kitchen sink
- Look anything like the system-GUI; just like Chromium/Electron, all rendering is custom
- Maximize the number of buzzwords in your project
- **Be used by anyone. It was built by an idiot, I mean come on...**

## Free Sample
Yea I would have probably skipped to here as well...
```csharp
// This is just normal C#, no more DSLs / Markup!
var window = new Window("The Bestest App Ever", 400, 100)
{
  ChildAlignment = ChildAlignments.Horizontal,
  Children = new Element[]
  {
    new Label
    {
      Margin = 4,
      Width = new SWeighted(1),
      Height = new SFill(),
      Background = Color.DarkRed,
      Border = new BorderStyle(1, Color.Cyan),
      Text = "Left panel"
    },
    new Label
    {
      Margin = new BorderSize(4, 4, 4, 4),
      Width = new SWeighted(3),
      Height = new SFill(),
      Background = Color.DarkGreen,
      Border = new BorderStyle(1, Color.Cyan),
      Text = "Right (bigger) panel"
    }
  }
};
```
![Output of the above](/Other/screenshot1.PNG?raw=true "Output of the above")

## Data Binding
Data Binding can be tricky, jump to [Data Binding Is Easy](#data-binding-is-easy) to understand why. IdiotGui takes a more sane approach and leverages the magic powers of the C# compiler to do all the binding **at compile time** without any `dynamic` faffing about or running expensive diffs at runtime.

### Binding Values
Values are bound via C# closures:
```csharp
var changingValue = "Hello, world";
```
```csharp
// Creates a "reactive" GUI element that will always reflect the value of 'changingValue'.
// This is done with C# closures so it is very fast and compile-time type checked.
new Label
{
  Text = Binding.Closure(() => changingValue)
}
```
```csharp
// Creates a 'static' GUI element that will NOT reflect changes to 'changingValue'
new Label
{
  Text = changingValue
}
```

### Binding IEnumerables (Children Elements)
The `Children` field can also be bound to a changing source, but that binding is done differently.

![Dank flip-desk meme](/Other/desk_flip.jpg?raw=true "Dank flip-desk meme")

Calm yourself! Because it's an IEnumerable source, what you're actually asking for is: "IdiotGui, create a new Element when a NEWLY ADDED item appears in the source list, remove old ones and handle reorders". IdiotGui simply binds to the source IEnumerable, diffs it against the previously seen elements, and calls a user-supplied factory function to generate new Elements.

This is much simpler to use than it is to explain:
```csharp
// A normal list of strings. Will always be rendered, even if it changes later (the GUI is "reactive").
var changingList = new List<string> {"  ", "The kitchen sink", "More indirection", "More confusing stuff", "CSS? lol"};
```
```csharp
var window = new Window("IdiotGui Todo List", 300, 200)
{
  ChildAlignment = ChildAlignments.Vertical,
  // Note that up until the special "SelectAndBind", this was just a normal LINQ expression.
  // "SelectAndBind" is a "Select" variant that creates a 'EnumerableChildrenBinding' for you.
  Children = changingList
    .Where(str => !string.IsNullOrWhiteSpace(str))
    .SelectAndBind(todo =>
      new Label
      {
        Border = new BorderStyle(1, Color.Cyan),
        // Works exactly like you would expect, including later updates
        Text = todo
      }
  )
};
```
![Output of the above](/Other/screenshot2.PNG?raw=true "Output of the above")

*Note: The source data types must be comparable in a meaningful way for this to work correctly (they must implement GetHashCode and Equals).*

#### Data Binding Is Easy
Data binding is a hard problem to solve in a lot of GUIs. The general description of data binding is: "If the value over there changes, it should change over here as well". In a lot of GUIs carry around massive amounts of code to solve this problem because the definition and rendering are decoupled through 18 different layers. In pure C# data binding is easy to do; C# already has data binding, it's called **Closures**.

The beauty of using closures is that they works exactly like you would expect thanks to the compiler. The example in [Binding Values](#binding-values) wouldn't normally work because `changingValue` is of `ValueType`, meaning it's passed around by value not reference (technically a lie, everything is passed by value, but work with me here okay). The compiler knows this, and will re-scope your `ValueType` so it can be accessed by the closure. On Top of that you get compile-time type-checking and all the assurances that go with that. If you were wondering why IdiotGui doesn't just use `dynamic`, that's why. Add in enough `dynamic` and you might as well just use JavaScript.

Beautiful hu? I think so too, but everyone thinks their own kids are beautiful...

## Desalination
Do I sound salty about the current state of desktop GUIs? No... not at all. That's why I wrote my own, because the existing ones are *so* good.

Okay okay, jokes aside, come sit by the fire and I'll tell you a tale.

The year was now and a lonely software developer wanted to make a visual programming language much like [Unreal Engine's Blueprinting system](https://docs.unrealengine.com/latest/INT/Engine/Blueprints/). Naive and optimistic the lonely little software developer created [NodeFlow](https://github.com/IdiotEngineering/NodeFlow) and in just a few days wrote the code that would take a visual AST and generate C# from it, then compile said C# and dynamically load and link the new assembly.

It was then, like most developers do, that he thought he should start prototyping the GUI. Having already created this EXACT project in HTML/CSS/JS before, and knowing how sluggish it was, the lonely little developer knew native rendering was the only option. "I know, I'll take a look at WPF!" he thought. Nope, not cross platform in any meaningful way. That's when the downward spiral into darkness started: "UWP then! Run with Xamarin!". After much brain-damage and frustration with how unbelievably complicated rendering a damned rectangle is, the developer slipped into a deep state of saltiness. For the C# language (the developer's primary source of sustenance after all the C at work that makes his eyes bleed and his heart cry) there was little out there that didn't make him slip further into Gui-related saltiness.

Through all the darkness came the light. Having already created 2 game engines in the past (one in C# that was fully multi-threaded and ran like a bat out of hell) the developer knew that there was a better, faster way to render rectangles than UWP/Electron. The lonely little developer started by trying to strip down his existing game engine but soon gave up on that and tossed the entire thing out. Time to create an entire GUI from scratch. To expedite the process the lonely little developer made a deal with the devil and pulled in Google Skia for rendering, with a promise to someday pull all of that junk out in favor of pure, multi-threaded GPU accelerated rendering like his game engine had. Same day... 

Born was Idiot Gui.