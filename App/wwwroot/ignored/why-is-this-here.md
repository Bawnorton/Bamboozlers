Due to how playwright works and doesn't work because nothing in this god damn tech stack wants to
cooperate, I have to capture the `_content` dir provided by blazor and shove it manually into the
`wwwroot` so that when playwright runs the site via unit testing it doesn't give me a
```
Failed to fetch dynamically imported module: 
http://localhost:5001/_content/Blazorise/button.js?v=1.5.0.0
```
It is a right pain in the rear and I have been trouble shooting this for 6 hours now. I have no idea 
why the `_content` dir isn't being automatically copied over to the site when it is run and there are 
2 google results if you search this, both of which don't resolve to a website. So this is what I came
up with. Feel free to give it a go if you want this to be done properly but I am so not spending another
minute on this.

When testing with playwright, rename the dir to `_content`, otherwise keep it as `ignored`. And double check
you're not renaming the stuff in `App.razor`, stuff breaks otherwise.