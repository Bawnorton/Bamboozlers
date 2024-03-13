window.persitentStorageInterop = {
    keyboardInteropElements: [],
    inputInteropElements: [],
}

window.keyboardInterop = {
    init: function (dotNetReference) {
        this.addListener(dotNetReference, document);
    },
    addListener: function (dotNetReference, element) {
        console.log("Adding keyboard listener for element: ", element);
        element.addEventListener("keydown", async function (event) {
            const data = {
                key: event.key,
                code: event.code,
                ctrl: event.ctrlKey,
                shift: event.shiftKey,
                alt: event.altKey,
                meta: event.metaKey
            };
            await dotNetReference.invokeMethodAsync("OnKeydown", data);
        });
        
        element.addEventListener("keyup", async function (event) {
            const data = {
                key: event.key,
                code: event.code,
                ctrl: event.ctrlKey,
                shift: event.shiftKey,
                alt: event.altKey,
                meta: event.metaKey
            };
            await dotNetReference.invokeMethodAsync("OnKeyup", data);
        });
    },
    register: async function (dotNetReference, keyboardListenerClass) {
        let elements = window.persitentStorageInterop.keyboardInteropElements;
        let newElements = [];
        newElements.push(...document.getElementsByClassName(keyboardListenerClass));
        newElements = newElements.filter((v, i, a) => a.indexOf(v) === i && !elements.includes(v));
        
        if (newElements.length !== 0) {
            elements.push(...newElements);
            window.persitentStorageInterop.keyboardInteropElements = elements;
        } else {
            return;
        }
        
        for (let i = 0; i < newElements.length; i++) {
            this.addListener(dotNetReference, newElements[i]);
        }
    }
}

window.inputInterop = {
    addListener: function (dotNetReference, disallowedKeys, element) {
        console.log("Adding input listener for element: ", element);
        let passed = true;
        element.addEventListener("keydown", async function (event) {
            const data = {
                key: event.key,
                code: event.code,
                ctrl: event.ctrlKey,
                shift: event.shiftKey,
                alt: event.altKey,
                meta: event.metaKey,
                content: element.innerText,
                passed: passed
            };
            for ({ key, code, ctrl, shift, alt, meta } of disallowedKeys) {
                if (event.key === key && event.code === code && event.ctrlKey === ctrl && event.shiftKey === shift && event.altKey === alt && event.metaKey === meta) {
                    passed = false;
                    event.preventDefault();
                    event.stopPropagation();
                    break;
                } else {
                    passed = true;
                }
            }
            await dotNetReference.invokeMethodAsync("OnInputKeydown", data);
        });
        
        element.addEventListener("keyup", async function (event) {
            const data = {
                key: event.key,
                code: event.code,
                ctrl: event.ctrlKey,
                shift: event.shiftKey,
                alt: event.altKey,
                meta: event.metaKey,
                content: element.innerText,
                passed: passed
            };
            await dotNetReference.invokeMethodAsync("OnInputKeyup", data);
        });
    },
    register: async function (dotNetReference, inputListenerClass) {
        let elements = window.persitentStorageInterop.inputInteropElements;
        let newElements = [];
        newElements.push(...document.getElementsByClassName(inputListenerClass));
        newElements = newElements.filter((v, i, a) => a.indexOf(v) === i && !elements.includes(v));
        
        if (newElements.length !== 0) {
            elements.push(...newElements);
            window.persitentStorageInterop.inputInteropElements = elements;
        } else {
            return;
        }
        
        const disallowedKeys = await dotNetReference.invokeMethodAsync("OnGetDisallowedInputs");
        
        for (let i = 0; i < newElements.length; i++) {
            this.addListener(dotNetReference, disallowedKeys, newElements[i]);
        }
    },
    clear: async function (elementId) {
        const element = document.getElementById(elementId);
        element.innerHTML = "";
    },
    scrollToBottom: async function (elementId) {
        const element = document.getElementById(elementId);
        element.scrollTop = element.scrollHeight;
    }
}