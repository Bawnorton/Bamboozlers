window.persistentStorageInterop = {
    keyboardInteropElements: [],
    inputInteropElements: [],
    mouseInteropElements: [],
    clipboardInteropElements: [],
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
        let elements = window.persistentStorageInterop.keyboardInteropElements;
        let newElements = [];
        newElements.push(...document.getElementsByClassName(keyboardListenerClass));
        newElements = newElements.filter((v, i, a) => a.indexOf(v) === i && !elements.includes(v));
        
        if (newElements.length !== 0) {
            elements.push(...newElements);
            window.persistentStorageInterop.keyboardInteropElements = elements;
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
                elementId: element.id,
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
                elementId: element.id,
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
        let elements = window.persistentStorageInterop.inputInteropElements;
        let newElements = [];
        newElements.push(...document.getElementsByClassName(inputListenerClass));
        newElements = newElements.filter((v, i, a) => a.indexOf(v) === i && !elements.includes(v));
        
        if (newElements.length !== 0) {
            elements.push(...newElements);
            window.persistentStorageInterop.inputInteropElements = elements;
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

window.mouseInterop = {
    addListener: function (dotNetReference, element) {
        console.log("Adding mouse listener for element: ", element);
        let passed = true;
        element.addEventListener("mouseover", async function (event) {
            const data = {
                elementId: element.id,
                passed: passed,
            };
            await dotNetReference.invokeMethodAsync("OnMouseOver", data);
        });

        element.addEventListener("mouseout", async function (event) {
            const data = {
                elementId: element.id,
                passed: passed,
            };
            await dotNetReference.invokeMethodAsync("OnMouseOut", data);
        });
    },
    register: async function (dotNetReference, inputListenerClass) {
        let elements = window.persistentStorageInterop.mouseInteropElements;
        let newElements = [];
        newElements.push(...document.getElementsByClassName(inputListenerClass));
        newElements = newElements.filter((v, i, a) => a.indexOf(v) === i && !elements.includes(v));

        if (newElements.length !== 0) {
            elements.push(...newElements);
            window.persistentStorageInterop.mouseInteropElements = elements;
        } else {
            return;
        }

        for (let i = 0; i < newElements.length; i++) {
            this.addListener(dotNetReference, newElements[i]);
        }
    },
}

window.clipboardInterop = {
    register: async function (dotNetReference, clipboardListenerClass) {
        let elements = window.persistentStorageInterop.clipboardInteropElements;
        let newElements = [];
        newElements.push(...document.getElementsByClassName(clipboardListenerClass));
        newElements = newElements.filter((v, i, a) => a.indexOf(v) === i && !elements.includes(v));
        
        if (newElements.length !== 0) {
            elements.push(...newElements);
            window.persistentStorageInterop.clipboardInteropElements = elements;
        } else {
            return;
        }
        
        for (let i = 0; i < newElements.length; i++) {
            this.addListener(dotNetReference, newElements[i]);
        }
    },
    addListener: function (dotNetReference, element) {
        console.log("Adding clipboard listener for element: ", element);
        element.addEventListener("paste", async function (event) {
            event.preventDefault();
            
            let text = (event.clipboardData || window.clipboardData).getData('text');
            const data = {
                elementId: element.id,
                text: text
            };
            element.innerHTML += await dotNetReference.invokeMethodAsync("OnPaste", data);
        
            const range = document.createRange();
            const sel = window.getSelection();
            range.selectNodeContents(element);
            range.collapse(false);
            sel.removeAllRanges();
            sel.addRange(range);
            element.focus();
        });
        
        element.addEventListener("copy", async function (event) {
            event.preventDefault();
            
            const clipdata = event.clipboardData || window.clipboardData;
            let text = clipdata.getData('text');
            const data = {
                elementId: element.id,
                text: text
            };
            const result = await dotNetReference.invokeMethodAsync("OnCopy", data);
            clipdata.setData('text', result);
        });
        
        element.addEventListener("cut", async function (event) {
            event.preventDefault();
            
            const clipdata = event.clipboardData || window.clipboardData;
            let text = clipdata.getData('text');
            const data = {
                elementId: element.id,
                text: text
            };
            const result = await dotNetReference.invokeMethodAsync("OnCut", data);
            clipdata.setData('text', result);
        });
    }
}