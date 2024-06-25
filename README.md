I've been running Blazor in production seven days after it was released.  
I use it for work all day and then for my hobby projects in the evening.  
I clock in a lot of Blazor development hours, and I'm always looking for ways to improve my workflow.
There are a few things that I've been missing in Visual Studio, and I've been thinking about creating a Visual Studio extension for a while now.
So I looked at some of my pain points and how I could solve them with an extension.

## Features when right-clicking a component in the solution explorer
When you right-click on a component in the Solution Explorer, you will see a new context menu with the following items:

<img src="https://github.com/biegehydra/Blazm.Extension/blob/main/Images/SolutionRazorContext.png?raw=true" width="400">

### Creating a new Isolated CSS file
I wanted to be able to create a new Isolated CSS file directly from the Solution Explorer, and I didn't want to have to type the name of the file.  
So I created a new Visual Studio extension called BlazmExtension that adds a new context menu item to the Solution Explorer.  
Just right-click on the component and select "Create Isolated CSS .  
This will create a new CSS file with the appropriate name for that component.  

### Creating a new Isolated JavaScript file
I also wanted to be able to create a new Isolated JavaScript file directly from the Solution Explorer.  
Just right-click on the component and select "Create Isolated JavaScript".  
This will create a new JavaScript file with the same name as the component.  

### Creating a new Codebehind file
This feature will add a code-behind file directly from the Solution Explorer.  
Just right-click on the component and select "Create Codebehind".  
This will create a new code-behind file with the same name as the component.  

### Find component usages (Solution Explorer)
Clicking this will search your entire solution for usages of the selected component. 

See [Find component usages (Window)](#find-component-usages-window)

## Features when right-clicking when selecting code in a Razor file.
When you select code and right-click in a component, you will see a new context menu with the following items:

<img src="https://github.com/biegehydra/Blazm.Extension/blob/main/Images/RazorMenuContext.png?raw=true" width="400">


### Move namespaces to _Imports
A common task when creating a new component is to add the namespace to the _Imports.razor file.
Simply select the namespaces you want to move and right-click and choose "Move namespaces to _Imports".

### Extract to Component
Blazor is a very component-based framework, and you often find yourself extracting parts of your code to a new component.
This task is very common, and I wanted to make it as easy as possible.
Simply select the code you want to extract and right-click and select "Extract to Component".
This will create a new component with the selected code and replace the selected code with the new component.

### Find component usages (Right click in razor file)
The behaviour of this dependends on the cursor position when you right click. If you are anywhere
within an opening, closing, or self-enclosing tag. It will open the component usages window and search for that component.
(Note: Works for standard html elements as well)

If you are not in an opening, closing, or self-enclosing tag - be it an empty line, csharp code space, or the body of a component - 
it will search for the razor component of the file you are in. For example, if you are in NavMenu.razor and right click in an 
`@code` block, it will search for `NavMenu` usages.

See [Find component usages (Window)](#find-component-usages-window)

## Features when right-clicking a code-behind file of a Razor component.
When you right-click on a component's codebehind file in the Solution Explorer you will see a new context menu with the following items:

<img src="https://github.com/EngstromJimmy/Blazm.Extension/blob/main/Images/SolutionRazorCsContext.png?raw=true" width="400">

### Move code-behind to razor
This is my favorite feature of the extension.
Visual Studio makes moving code from the razor file to the code-behind file easy.
But it doesn't have a feature to move code from the code-behind file to the razor file.
I prefer to have all my code in the razor file, and I often move code from the code-behind file to the razor file.
Right-click on the component.razor.cs file and select "Move code-behind to razor".
This feature is in beta. I hope I have managed to cover all the edge cases, but if you find any bugs, please let me know.

## Generic features
### Switching files
Switching files between razor and code-behind is common, and I wanted to make it as easy as possible.
By pressing CTRL + ALT + N  (N for Next file), you can switch between the razor file and the code-behind file (or any nested files).

### Blazor Routes
In a large project, it can be hard to keep track of all the routes.
I wanted to be able to see all the routes in one place, so I created a new window called Blazor Routes.
You can open the window by going to Extensions -> Blazm-> Show Blazor Routes.
The window will show all the routes in your project, and you can double-click on a route to open the file.

<img src="https://github.com/EngstromJimmy/Blazm.Extension/blob/main/Images/Routes.png?raw=true" width="400">

### Quick save
This is an experimental feature that I'm testing.
I noticed that using Hot Reload from Notepad was alot faster that using it from Visual Studio. I am not sure if it is related but Visual Studio saves the file as a temporary file, deletes the original and renames the temp file. My theory is that this is why Hot Reload is slower in Visual Studio.
So I created a new feature called Quick Save. This feature will save the file without creating a temp file.
There is probably a very good reaon why Visual Studio saves the file as a temp file, so take into consideration that this feature is experimental.

### Run dotnet watch
While creating my Blazor course on Dometrain (shameless plug), I noticed that Hot Reload running from PowerShell (dotnet watch) worked better than from Visual Studio, it is also quite nice to have it running in the background. I prefer not to have to write things in powershell or the console in general. So I added "Run dotnet watch" menu item for projects.
So now we can just right-click on the project and select "Run dotnet watch" and it will start dotnet watch in a PowerShell window.

### bUnit test generation
Think writing tests is tedious? Well, this new feature promises to eliminate much of that monotony, letting us zero in on the exciting parts!
Currently, this feature is still undergoing refinement and is in its beta phase. Your feedback and suggestions would be invaluable as we refine this tool.

This feature will automatically produce test code:

```csharp
@inherits TestContext
@using Bunit
@using BlazorApp1.Pages;
@code
{
    [Fact]
    public void Test1()
    {
        //Arrange
        //Services.AddSingleton<WeatherForecastService,/*Add implementation for WeatherForecastService*/>();

        var cut = Render(@<FetchData></FetchData>);
        
        //Act

        //Assert
    }
}
```
For those who lean towards a pure C# syntax, the generator will provide:

```csharp

public class FetchDataTests : TestContext
{
    [Fact]
    public void Test1()
    {
        //Arrange
        //Services.AddSingleton<WeatherForecastService,/*Add implementation for WeatherForecastService*/>();
        var cut = RenderComponent<FetchData>();
        
        //Act

        //Assert
    }
}
```
This generator is designed to seamlessly incorporate dependency injected properties, render fragments, parameters, and much more. While I've tried to cover the most typical scenarios, there may be edge cases I've missed. Bear with me as I continue to tweak it.

There's an ongoing challenge with line break placements, but I'm optimistic about finding a resolution. To leverage this feature, simply right-click on a component and choose the desired command.

<img src="https://github.com/EngstromJimmy/Blazm.Extension/blob/main/Images/bUnit.png?raw=true" width="400">

### Find component usages (Window)
The find component usages feature opens up a window that shows you the file name, file path, line number, and a preview, of all usages for a particular razor component in your solution. In
this window there is also a search box to let you input a different component to find - this search box has auto complete but only for components in your solution. 

<img src="https://github.com/engstromjimmy/Blazm.Extension/blob/main/Images/ComponentUsagesExample.png?raw=true" width="400">

## Conclusion
I hope you find this extension useful.
I will continue to add new features to the extension and I'm open to suggestions.
You can find the extension on the Visual Studio Marketplace:
[https://marketplace.visualstudio.com/items?itemName=EngstromJimmy.BlazmExtension](https://marketplace.visualstudio.com/items?itemName=EngstromJimmy.BlazmExtension)
