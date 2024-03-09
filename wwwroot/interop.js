window.messageInputInterop = {
    init: async function (dotNetReference, elementId) {
        const element = document.getElementById(elementId);

        const disallowedKeys = await dotNetReference.invokeMethodAsync("GetDisallowedKeys");
        let passed = true;
        // Impl Note: Determining if the event should be prevented from the "OnKeydown" event is too slow and causes the event to be missed. 
        element.addEventListener("keydown", async function (event) {
            passed = true;
            for(let {alt, code, ctrl, key, meta, shift} of disallowedKeys) {
                if(key === event.key && code === event.code && ctrl === event.ctrlKey && shift === event.shiftKey && alt === event.altKey && meta === event.metaKey) {
                    passed = false;
                    event.preventDefault();
                    break;
                }
            }
            const data = {
                KeyReference: {
                    Key: event.key,
                    Code: event.code,
                    Ctrl: event.ctrlKey,
                    Shift: event.shiftKey,
                    Alt: event.altKey,
                    Meta: event.metaKey,
                },
                Text: element.innerText,
                Passed: passed
            };
            await dotNetReference.invokeMethodAsync("OnKeydown", data);
        });

        element.addEventListener("keyup", async function (event) {
            const data = {
                KeyReference: {
                    Key: event.key,
                    Code: event.code,
                    Ctrl: event.ctrlKey,
                    Shift: event.shiftKey,
                    Alt: event.altKey,
                    Meta: event.metaKey,
                },
                Text: element.innerText,
                Passed: passed
            };
            await dotNetReference.invokeMethodAsync("OnKeyup", data);
        });
    },
    clear: async function (elementId) {
        const element = document.getElementById(elementId);
        element.innerHTML = "";
    }
}