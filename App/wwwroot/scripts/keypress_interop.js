window.keypress = {
    startListening: function (reference) {
        window.addEventListener('keydown', e => window.keypress.handleKeyDown(e, reference));
        window.addEventListener('keyup', e => window.keypress.handleKeyUp(e, reference));
    },
    stopListening: function (reference) {
        window.removeEventListener('keydown', e => window.keypress.handleKeyDown(e, reference));
        window.removeEventListener('keyup', e => window.keypress.handleKeyUp(e, reference));
    },
    handleKeyDown: function (event, reference) {
        const data = {
            key: event.key,
            code: event.code,
            ctrl: event.ctrlKey,
            shift: event.shiftKey,
            alt: event.altKey,
            meta: event.metaKey
        };
        reference.invokeMethodAsync('OnKeyPressed', data);
    },
    handleKeyUp: function (event, reference) {
        const data = {
            key: event.key,
            code: event.code,
            ctrl: event.ctrlKey,
            shift: event.shiftKey,
            alt: event.altKey,
            meta: event.metaKey
        };
        reference.invokeMethodAsync('OnKeyReleased', data);
    }
};
