window.persistentStorageInterop = {
    keyboardInteropElements: [],
    inputInteropElements: [],
    mouseInteropElements: [],
    clipboardInteropElements: [],
}

window.invokeInterop = async function (functionName, data) {
    try {
        return await DotNet.invokeMethodAsync("Bamboozlers", functionName, data);
    } catch (error) {
        if (error.message !== "No call dispatcher has been set.") {
            throw error;
        }
    }
}

window.keyboardInterop = {
    addListener: function (element) {
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
            await window.invokeInterop("OnKeydown", data);
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
            await window.invokeInterop("OnKeyup", data);
        });
    },
    register: async function (keyboardListenerClass) {
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
            this.addListener(newElements[i]);
        }
    }
}

window.inputInterop = {
    addListener: function (disallowedKeys, element) {
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
            await window.invokeInterop("OnInputKeydown", data);
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
            await window.invokeInterop("OnInputKeyup", data);
        });
    },
    register: async function (inputListenerClass) {
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
        
        const disallowedKeys = await window.invokeInterop("OnGetDisallowedInputs");
        
        for (let i = 0; i < newElements.length; i++) {
            this.addListener(disallowedKeys, newElements[i]);
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
    addListener: function (element) {
        console.log("Adding mouse listener for element: ", element);
        let passed = true;
        element.addEventListener("mouseover", async function (event) {
            const data = {
                elementId: element.id,
                passed: passed,
            };
            await window.invokeInterop("OnMouseOver", data);
        });

        element.addEventListener("mouseout", async function (event) {
            const data = {
                elementId: element.id,
                passed: passed,
            };
            await window.invokeInterop("OnMouseOut", data);
        });
    },
    register: async function (inputListenerClass) {
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
            this.addListener(newElements[i]);
        }
    },
}

window.clipboardInterop = {
    register: async function (clipboardListenerClass) {
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
            this.addListener(newElements[i]);
        }
    },
    addListener: function (element) {
        console.log("Adding clipboard listener for element: ", element);
        element.addEventListener("paste", async function (event) {
            event.preventDefault();
            
            let text = (event.clipboardData || window.clipboardData).getData('text');
            const data = {
                elementId: element.id,
                text: text
            };
            let result = await window.invokeInterop("OnPaste", data);
            if (result && result !== "") {
                text = result;
            }

            const sel = window.getSelection();
            const range = sel.getRangeAt(0);

            range.deleteContents();

            const textNode = document.createTextNode(text);
            range.insertNode(textNode);

            range.setStartAfter(textNode);
            range.collapse(true);

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
            const result = await window.invokeInterop("OnCopy", data);
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
            const result = await window.invokeInterop("OnCut", data);
            clipdata.setData('text', result);
        });
    }
}

window.keyboardInterop.addListener(document);