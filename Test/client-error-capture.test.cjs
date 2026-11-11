const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const html = fs.readFileSync(path.join(__dirname, '..', 'Client', 'wwwroot', 'index.html'), 'utf8');
const captureSource = html.match(/window\.treacheryErrorLogging = \(\(\) => \{[\s\S]*?\}\)\(\);/)[0];
const mediaAbortSource = html.match(/function isExpectedMediaFetchAbort\(error\) \{[\s\S]*?\n        \}/)[0];
const message = "Cannot read properties of undefined (reading 'M_ID')";
const extensionUrl = 'chrome-extension://eppiocemhmnlbhjplcgkofciiegomcon/executors/200.js';
const extensionStack = `TypeError: ${message}\n    at Y (${extensionUrl}:1:761)\n    at E (${extensionUrl}:1:1442)`;

function harness() {
    const listeners = new Map(), reports = [];
    const window = {
        location: { pathname: '/game', hash: '#battle' },
        addEventListener(name, listener) { listeners.set(name, listener); },
        removeEventListener(name, listener) {
            if (listeners.get(name) === listener) listeners.delete(name);
        }
    };
    vm.runInNewContext(`${mediaAbortSource}\n${captureSource}`, {
        window, navigator: { userAgent: 'test browser' }, console
    });
    const reference = {
        invokeMethodAsync(...args) {
            reports.push(args);
            return Promise.resolve();
        }
    };
    window.treacheryErrorLogging.start(reference);
    return {
        window, reports,
        dispatch(name, event) { listeners.get(name)?.(event); }
    };
}

test('does not report the supplied Chrome extension failure from either global handler', () => {
    const h = harness();
    let prevented = false;
    h.dispatch('error', {
        message, error: { stack: extensionStack },
        preventDefault() { prevented = true; }
    });
    h.dispatch('unhandledrejection', {
        reason: { message, stack: extensionStack },
        preventDefault() { prevented = true; }
    });
    assert.equal(h.reports.length, 0);
    assert.equal(prevented, false, 'extension failures remain visible to the browser');
});

test('recognizes extension filenames and Chrome, Firefox, and Safari stack frames', () => {
    for (const scheme of ['chrome-extension', 'moz-extension', 'safari-extension', 'safari-web-extension', 'ms-browser-extension']) {
        const url = `${scheme}://extension-id/script.js`;
        const h = harness();
        h.dispatch('error', { message, filename: url });
        for (const stack of [
            `TypeError: ${message}\n    at run (${url}:1:2)`,
            `TypeError: ${message}\n    at ${url}:1:2`,
            `TypeError: ${message}\n    at async ${url}:1:2`,
            `run@${url}:1:2\ncaller@${url}:3:4`,
            `global code@${url}:1:2`,
            `@${url}:1:2`
        ]) {
            h.dispatch('error', { message, error: { stack } });
            h.dispatch('unhandledrejection', { reason: { message, stack } });
        }
        assert.equal(h.reports.length, 0, scheme);
    }
});

test('reports application failures even when an extension appears later in the stack', () => {
    const h = harness();
    const stack = `TypeError: ${message}\n    at run (https://treachery.online/app.js:10:20)\n    at E (${extensionUrl}:1:1442)`;
    h.dispatch('error', { message, error: { stack }, lineno: 10, colno: 20 });
    h.dispatch('unhandledrejection', { reason: { message, stack } });
    assert.deepEqual(h.reports, [
        ['ReportClientError', 'JavaScript', message, stack, '/game#battle', 'test browser', 10, 20],
        ['ReportClientError', 'JavaScript', message, stack, '/game#battle', 'test browser', null, null]
    ]);
});

test('uses an explicit application filename rather than attributing its error to the stack', () => {
    const h = harness();
    h.dispatch('error', {
        message, filename: 'https://treachery.online/app.js', error: { stack: extensionStack }
    });
    assert.equal(h.reports.length, 1);
    assert.equal(h.reports[0][2], message);
});

test('does not skip a relative or native first frame to find an extension caller', () => {
    for (const firstFrame of ['    at run (/app.js:10:20)', '    at run (native)', 'run@app.js:10:20']) {
        const h = harness();
        const stack = `TypeError: ${message}\n${firstFrame}\n    at E (${extensionUrl}:1:1442)`;
        h.dispatch('error', { message, error: { stack } });
        h.dispatch('unhandledrejection', { reason: { message, stack } });
        assert.equal(h.reports.length, 2, firstFrame);
    }
});

test('does not suppress an error merely because its message mentions an extension URL', () => {
    const h = harness();
    const text = `Unable to access ${extensionUrl}`;
    h.dispatch('error', { message: text, error: { stack: `Error: ${text}` } });
    h.dispatch('unhandledrejection', { reason: text });
    assert.equal(h.reports.length, 2);
    assert.ok(h.reports.every(report => report[2] === text));
});

test('preserves reporting when there is no script origin or error stack', () => {
    const h = harness();
    h.dispatch('error', { message });
    h.dispatch('error', {});
    h.dispatch('unhandledrejection', { reason: { message } });
    h.dispatch('unhandledrejection', { reason: null });
    assert.deepEqual(h.reports.map(report => report[2]), [
        message,
        'JavaScript resource failed to load',
        message,
        'Unhandled promise rejection'
    ]);
});

test('excludes detail-free Script error reports without suppressing browser diagnostics', () => {
    const h = harness();
    let prevented = false;
    h.dispatch('error', {
        message: 'Script error.', filename: '', lineno: 0, colno: 0, error: null,
        preventDefault() { prevented = true; }
    });
    h.dispatch('unhandledrejection', {
        reason: { message: 'Script error.', stack: '' },
        preventDefault() { prevented = true; }
    });
    h.dispatch('unhandledrejection', { reason: 'Script error.' });
    assert.equal(h.reports.length, 0);
    assert.equal(prevented, false);
});

test('retains Script error reports with a stack, filename, line, or column', () => {
    const h = harness();
    const stack = 'run@https://treachery.online/app.js:10:20';
    for (const details of [
        { error: { stack } },
        { filename: 'https://treachery.online/app.js' },
        { lineno: 10 },
        { colno: 20 }
    ]) {
        h.dispatch('error', { message: 'Script error.', ...details });
    }
    h.dispatch('unhandledrejection', { reason: { message: 'Script error.', stack } });
    assert.equal(h.reports.length, 5);
    assert.ok(h.reports.every(report => report[2] === 'Script error.'));
    assert.equal(h.reports[0][3], stack);
    assert.equal(h.reports[2][6], 10);
    assert.equal(h.reports[3][7], 20);
});

test('continues to exclude expected media fetch aborts', () => {
    const h = harness();
    const abort = "The fetching process for the media resource was aborted by the user agent at the user's request.";
    h.dispatch('error', { message: abort });
    h.dispatch('unhandledrejection', { reason: { message: abort } });
    assert.equal(h.reports.length, 0);
});

test('stops reporting after capture is disposed and resumes when restarted', () => {
    const h = harness();
    h.window.treacheryErrorLogging.stop();
    h.dispatch('error', { message });
    h.dispatch('unhandledrejection', { reason: { message } });
    assert.equal(h.reports.length, 0);
    h.window.treacheryErrorLogging.start({
        invokeMethodAsync(...args) {
            h.reports.push(args);
            return Promise.resolve();
        }
    });
    h.dispatch('error', { message });
    assert.equal(h.reports.length, 1);
});
