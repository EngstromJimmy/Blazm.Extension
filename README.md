I've been running Blazor in production seven days after it was released.  
I use it for work all day and then for my hobby projects in the evening.  
I clock in a lot of Blazor development hours, and I'm always looking for ways to improve my workflow.
There are a few things that I've been missing in Visual Studio, and I've been thinking about creating a Visual Studio extension for a while now.
So I looked at some of my pain points and how I could solve them with an extension.

## Features when right-clicking a component in the solution explorer
When you right-click on a component in the Solution Explorer, you will see a new context menu with the following items:

<img src="https://github.com/EngstromJimmy/Blazm.Extension/blob/main/Images/SolutionRazorContext.png?raw=true" width="400">

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


## Features when right-clicking when selecting code in a Razor file.
When you select code and right-click in a component, you will see a new context menu with the following items:

<img src="https://github.com/EngstromJimmy/Blazm.Extension/blob/main/Images/RazorMenuContext.png?raw=true" width="400">


### Move namespaces to _Imports
A common task when creating a new component is to add the namespace to the _Imports.razor file.
Simply select the namespaces you want to move and right-click and choose "Move namespaces to _Imports".

### Extract to Component
Blazor is a very component-based framework, and you often find yourself extracting parts of your code to a new component.
This task is very common, and I wanted to make it as easy as possible.
Simply select the code you want to extract and right-click and select "Extract to Component".
This will create a new component with the selected code and replace the selected code with the new component.

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


## Conclusion
I hope you find this extension useful.
I will continue to add new features to the extension and I'm open to suggestions.
You can find the extension on the Visual Studio Marketplace:
[https://marketplace.visualstudio.com/items?itemName=EngstromJimmy.BlazmExtension](https://marketplace.visualstudio.com/items?itemName=EngstromJimmy.BlazmExtension)
