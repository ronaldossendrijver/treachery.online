const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const source = fs.readFileSync(path.join(__dirname, '..', 'Client', 'wwwroot', 'js', 'fx.js'), 'utf8');

function harness() {
    let elements = [], time = 0, nextFrame = 0;
    const frames = new Map(), draws = [], canvasDraws = [], textDraws = [], textCanvases = new Map(), canvases = [];
    const reducedMotion = { matches: false };
    const rect = (left = 0, top = 0, width = 24, height = 24) =>
        ({ left, top, width, height, right: left + width, bottom: top + height });
    const stage = { appendChild(el) { el.isConnected = true; } };
    const root = {
        isConnected: true,
        querySelectorAll: () => elements,
        getBoundingClientRect: () => rect(0, 0, 600, 600)
    };
    const context = {
        drawImage(...args) { draws.push(args); canvasDraws.push({ canvas: this.canvas, args }); },
        fillText(text) { textDraws.push(text); textCanvases.set(this.canvas, text); },
        strokeText() {},
        measureText: text => ({ width: text.length * 8, fontBoundingBoxAscent: 9, fontBoundingBoxDescent: 3 }),
        getImageData: (_x, _y, w, h) => ({ data: new Uint8ClampedArray(w * h * 4).fill(255) }),
        createRadialGradient: () => ({ addColorStop() {} }),
        save() {}, restore() {}, translate() {}, rotate() {}, scale() {},
        clearRect() {}, beginPath() {}, arc() {}, fill() {}, stroke() {}, moveTo() {}, lineTo() {}
    };
    const document = {
        hidden: false,
        body: stage,
        getElementById: () => root,
        querySelector: selector => selector === '.map-stage' ? stage :
            elements.find(el => selector.includes(`"${el.dataset.fxKey}"`)),
        querySelectorAll: () => elements.filter(el => el.dataset.fxKey.startsWith('force-')),
        createElement: () => {
            const canvas = {
                width: 0, height: 0, style: {}, isConnected: false,
                setAttribute() {}, remove() { this.isConnected = false; }
            };
            const canvasContext = Object.assign(Object.create(context), { canvas });
            canvas.getContext = () => canvasContext;
            canvases.push(canvas);
            return canvas;
        }
    };
    const window = { devicePixelRatio: 1, innerWidth: 600, innerHeight: 600, matchMedia: () => reducedMotion };
    vm.runInNewContext(source, {
        window, document, console, CSS: { escape: s => s }, performance: { now: () => time },
        getComputedStyle: () => ({
            font: 'bold 12px Verdana', color: 'white', textShadow: 'rgb(0, 0, 0) 0px 0px 2px',
            webkitTextStrokeColor: 'black', webkitTextStrokeWidth: '1px'
        }),
        setInterval: () => 1, setTimeout: () => 1,
        requestAnimationFrame: callback => { frames.set(++nextFrame, callback); return nextFrame; }
    });
    function image(left = 0) {
        const listeners = {};
        return {
            tagName: 'IMG', complete: true, naturalWidth: 24, style: {}, dataset: {},
            closest: () => null, getBoundingClientRect: () => rect(left),
            addEventListener(name, callback) { listeners[name] = callback; },
            load() { this.complete = true; listeners.load?.(); }
        };
    }
    function token(key, count = 1, left = 0) {
        const imgs = Array.from({ length: count }, () => image(left));
        if (!key.startsWith('force-')) {
            const el = imgs[0];
            el.dataset.fxKey = key;
            el.dataset.fxAmount = String(count);
            return el;
        }
        const attributes = new Map();
        const label = key.endsWith('-White-s') ? null : {
            tagName: 'SPAN', textContent: String(count), offsetWidth: String(count).length * 8, offsetHeight: 12,
            closest: () => null, getBoundingClientRect: () => rect(left + 12, 8, String(count).length * 8, 12),
            setAttribute(name, value) { attributes.set(name, String(value)); },
            getAttribute(name) { return attributes.has(name) ? attributes.get(name) : null; },
            removeAttribute(name) { attributes.delete(name); }
        };
        return {
            tagName: 'DIV', dataset: { fxKey: key }, querySelectorAll: () => imgs,
            querySelector: () => label, imgs, label
        };
    }
    return {
        token, window, document, reducedMotion, frames, draws, canvasDraws, textDraws, textCanvases, canvases,
        async sync(next, scene = 'game') {
            elements = next;
            await window.FxSyncMapTokens(scene);
        },
        step(ms) {
            time += ms;
            const callbacks = [...frames.values()];
            frames.clear();
            callbacks.forEach(callback => callback(time));
        }
    };
}

test('initial state and game replacement do not animate every token', async () => {
    const h = harness();
    await h.sync([h.token('force-1-Green-f', 4), h.token('terror-1-0')]);
    assert.equal(h.frames.size, 0);
    await h.sync([h.token('force-2-Red-f', 6)], 'other-game');
    assert.equal(h.frames.size, 0);
});

test('shipping hides the force count while images load and assembles it with the forces', async () => {
    const h = harness();
    await h.sync([]);
    const shipped = h.token('force-1-Green-f', 3);
    shipped.imgs[2].complete = false;
    const sync = h.sync([shipped]);
    assert.equal(shipped.label.getAttribute('data-fx-previous'), '');
    assert.ok(shipped.imgs.every(img => img.style.visibility === 'hidden'));
    shipped.imgs[2].load();
    await sync;
    assert.ok(h.textDraws.includes('3'), 'the arriving count must be drawn into the animation');
    h.step(400);
    assert.ok(h.draws.length > 0);
    h.step(449);
    assert.equal(shipped.label.getAttribute('data-fx-previous'), '');
    h.step(1);
    assert.equal(shipped.label.getAttribute('data-fx-previous'), null);
    assert.ok(shipped.imgs.every(img => img.style.visibility === ''));
    assert.equal(h.frames.size, 0);
});

test('a newer sync replaces pending image-load holds without revealing its count early', async () => {
    const h = harness();
    await h.sync([]);
    const slow = h.token('force-1-Green-f', 3);
    slow.imgs[0].complete = false;
    const oldSync = h.sync([slow]);
    const newer = h.token('force-1-Green-f', 2);
    await h.sync([newer]);
    assert.equal(slow.label.getAttribute('data-fx-previous'), null);
    assert.ok(slow.imgs.every(img => img.style.visibility !== 'hidden'));
    assert.equal(newer.label.getAttribute('data-fx-previous'), '');
    slow.imgs[0].load();
    await oldSync;
    assert.equal(newer.label.getAttribute('data-fx-previous'), '');
    h.step(850);
    assert.equal(newer.label.getAttribute('data-fx-previous'), null);
});

test('partial movement beams only transferred forces, leaving the rest visible', async () => {
    const h = harness();
    await h.sync([h.token('force-1-Green-f', 5)]);
    const source = h.token('force-1-Green-f', 3);
    const destination = h.token('force-2-Green-f', 2, 300);
    await h.sync([source, destination]);
    assert.ok(source.imgs.every(img => !img.style.visibility));
    assert.ok(destination.imgs.every(img => img.style.visibility === 'hidden'));
    assert.equal(destination.label.getAttribute('data-fx-previous'), '');
    assert.ok(h.textDraws.filter(text => text === '2').length >= 2, 'both ends of the beam include the moved count');
    h.step(500);
    h.step(600);
    assert.ok(destination.imgs.every(img => img.style.visibility === ''));
    assert.equal(destination.label.getAttribute('data-fx-previous'), null);
    assert.equal(h.frames.size, 0);
    await h.sync([source, destination]);
    assert.equal(h.frames.size, 0, 'unchanged stacks must not replay effects');
});

test('reinforcements animate without hiding forces already at their destination', async () => {
    const h = harness();
    await h.sync([h.token('force-1-Green-f', 3), h.token('force-2-Green-f', 2, 300)]);
    const reinforced = h.token('force-2-Green-f', 5, 300);
    await h.sync([reinforced]);
    assert.ok(reinforced.imgs.slice(0, 2).every(img => !img.style.visibility));
    assert.ok(reinforced.imgs.slice(2).every(img => img.style.visibility === 'hidden'));
    assert.equal(reinforced.label.getAttribute('data-fx-previous'), '2');
    assert.ok(h.textDraws.includes('3'), 'the moving forces carry their count, not the new total');
    h.step(1100);
    assert.ok(reinforced.imgs.every(img => img.style.visibility !== 'hidden'));
    assert.equal(reinforced.label.getAttribute('data-fx-previous'), null);
});

test('arriving reinforcement counts do not cover the previous count', async () => {
    async function labelOffset(existing) {
        const h = harness();
        await h.sync(existing ? [h.token('force-1-Green-f', 2)] : []);
        await h.sync([h.token('force-1-Green-f', existing ? 5 : 3)]);
        const label = h.canvasDraws.find(draw => h.textCanvases.get(draw.args[0]) === '3');
        const image = h.canvasDraws.find(draw => draw.canvas === label.canvas && draw !== label);
        return label.args[2] - image.args[2];
    }

    assert.equal(await labelOffset(true), (await labelOffset(false)) - 14);
});

test('rerendering a stack keeps incoming forces and the prior count in sync', async () => {
    const h = harness();
    await h.sync([h.token('force-1-Green-f', 2)]);
    await h.sync([h.token('force-1-Green-f', 5)]);
    h.step(200);
    const rerendered = h.token('force-1-Green-f', 5);
    await h.sync([rerendered]);
    assert.equal(rerendered.label.getAttribute('data-fx-previous'), '2');
    assert.ok(rerendered.imgs.slice(0, 2).every(img => !img.style.visibility));
    assert.ok(rerendered.imgs.slice(2).every(img => img.style.visibility === 'hidden'));
    h.step(650);
    assert.equal(rerendered.label.getAttribute('data-fx-previous'), null);
    assert.ok(rerendered.imgs.every(img => img.style.visibility !== 'hidden'));
});

test('overlapping shipments update the existing count only as each shipment arrives', async () => {
    const h = harness();
    await h.sync([h.token('force-1-Green-f', 2)]);
    const first = h.token('force-1-Green-f', 5);
    await h.sync([first]);
    assert.equal(first.label.getAttribute('data-fx-previous'), '2');
    h.step(200);
    const second = h.token('force-1-Green-f', 7);
    await h.sync([second]);
    assert.equal(first.label.getAttribute('data-fx-previous'), null);
    assert.equal(second.label.getAttribute('data-fx-previous'), '2');
    assert.ok(second.imgs.slice(0, 2).every(img => !img.style.visibility));
    assert.ok(second.imgs.slice(2).every(img => img.style.visibility === 'hidden'));
    h.step(650);
    assert.equal(second.label.getAttribute('data-fx-previous'), '5');
    assert.ok(second.imgs.slice(0, 5).every(img => img.style.visibility !== 'hidden'));
    assert.ok(second.imgs.slice(5).every(img => img.style.visibility === 'hidden'));
    h.step(200);
    assert.equal(second.label.getAttribute('data-fx-previous'), null);
    assert.ok(second.imgs.every(img => img.style.visibility !== 'hidden'));
});

test('mixed movements and shipments keep the previous count until their respective arrivals', async () => {
    const h = harness();
    await h.sync([h.token('force-1-Green-f', 3), h.token('force-2-Green-f', 2, 300)]);
    const destination = h.token('force-2-Green-f', 7, 300);
    await h.sync([destination]);
    assert.equal(destination.label.getAttribute('data-fx-previous'), '2');
    assert.ok(h.textDraws.includes('3'));
    assert.ok(h.textDraws.includes('2'));
    h.step(850);
    assert.equal(destination.label.getAttribute('data-fx-previous'), '4');
    h.step(150);
    assert.equal(destination.label.getAttribute('data-fx-previous'), null);
});

test('resetting the map cancels a held force count', async () => {
    const h = harness();
    await h.sync([]);
    const shipped = h.token('force-1-Green-f', 3);
    await h.sync([shipped]);
    assert.equal(shipped.label.getAttribute('data-fx-previous'), '');
    await h.sync([shipped], 'other-game');
    assert.equal(shipped.label.getAttribute('data-fx-previous'), null);
    assert.ok(shipped.imgs.every(img => img.style.visibility !== 'hidden'));
});

test('a removed shipment releases its count before the same nodes are reused', async () => {
    const h = harness();
    await h.sync([]);
    const shipped = h.token('force-1-Green-f', 3);
    await h.sync([shipped]);
    h.step(100);
    shipped.dataset.fxKey = 'force-2-Green-f';
    await h.sync([shipped]);
    assert.equal(shipped.label.getAttribute('data-fx-previous'), '');
    assert.ok(shipped.imgs.every(img => img.style.visibility === 'hidden'));
    h.step(1000);
    assert.equal(shipped.label.getAttribute('data-fx-previous'), null);
    assert.ok(shipped.imgs.every(img => img.style.visibility !== 'hidden'));
});

test('enabling reduced motion releases an in-progress shipment', async () => {
    const h = harness();
    await h.sync([]);
    const shipped = h.token('force-1-Green-f', 3);
    await h.sync([shipped]);
    h.reducedMotion.matches = true;
    await h.sync([shipped]);
    assert.equal(shipped.label.getAttribute('data-fx-previous'), null);
    assert.ok(shipped.imgs.every(img => img.style.visibility !== 'hidden'));
    h.step(1100);
    assert.equal(h.frames.size, 0);
});

test('forces without a displayed count still animate normally', async () => {
    const h = harness();
    await h.sync([]);
    const shipped = h.token('force-1-White-s', 2);
    await h.sync([shipped]);
    assert.equal(shipped.label, null);
    assert.ok(shipped.imgs.every(img => img.style.visibility === 'hidden'));
    const rerendered = h.token('force-1-White-s', 2);
    await h.sync([rerendered]);
    assert.ok(rerendered.imgs.every(img => img.style.visibility === 'hidden'));
    h.step(850);
    assert.ok(rerendered.imgs.every(img => img.style.visibility !== 'hidden'));
    assert.equal(h.textDraws.length, 0);
});

test('spice loss animates both partial collection and complete removal', async () => {
    const h = harness();
    await h.sync([h.token('spice-1', 10)]);
    await h.sync([h.token('spice-1', 4)]);
    assert.ok(h.frames.size > 0);
    h.step(1000);
    await h.sync([]);
    assert.ok(h.frames.size > 0);
    h.step(1000);
    assert.equal(h.frames.size, 0);
});

test('spice blow runs at half speed for exactly 5800 milliseconds', async () => {
    const h = harness();
    await h.sync([h.token('spice-1', 10)]);
    h.window.FxSpiceBlow(['spice-1'], 0);
    const canvas = h.canvases[h.canvases.length - 1];
    assert.ok(canvas.isConnected);
    h.step(2900);
    assert.ok(canvas.isConnected, 'the blow must still be running at the old end time');
    h.step(2899);
    assert.ok(canvas.isConnected);
    h.step(1);
    assert.equal(canvas.isConnected, false);
    assert.equal(h.frames.size, 0);
});

test('spice blow respects reduced motion', async () => {
    const h = harness();
    await h.sync([h.token('spice-1', 10)]);
    h.reducedMotion.matches = true;
    const canvasCount = h.canvases.length;
    h.window.FxSpiceBlow(['spice-1'], 0);
    assert.equal(h.canvases.length, canvasCount);
    assert.equal(h.frames.size, 0);
});

for (const type of ['terror', 'ambassador', 'discovery']) {
    test(`${type} tokens animate in and out`, async () => {
        const h = harness();
        await h.sync([]);
        const token = h.token(`${type}-1-0`);
        await h.sync([token]);
        assert.equal(token.style.visibility, 'hidden');
        h.step(800);
        assert.equal(token.style.visibility, '');
        await h.sync([]);
        assert.ok(h.frames.size > 0);
        h.step(1000);
        assert.equal(h.frames.size, 0);
    });
}

test('reduced motion and background updates stay instantaneous', async () => {
    for (const setting of ['reducedMotion', 'hidden']) {
        const h = harness();
        await h.sync([]);
        if (setting === 'reducedMotion') h.reducedMotion.matches = true;
        else h.document.hidden = true;
        const shipped = h.token('force-1-Green-f', 2);
        await h.sync([shipped, h.token('terror-1-0')]);
        assert.equal(h.frames.size, 0);
        assert.ok(shipped.imgs.every(img => !img.style.visibility));
        assert.equal(shipped.label.getAttribute('data-fx-previous'), null);
        assert.equal(h.textDraws.length, 0);
    }
});

test('killed stacks use the existing tanks effect, not a second map departure', async () => {
    const h = harness();
    await h.sync([h.token('force-1-Green-f', 3)]);
    h.window.FxDisintegrate([{ key: 'force-1-Green-f', from: 3, to: 0 }]);
    // The existing tanks effect owns the sole scheduled animation.
    h.step(1600);
    await h.sync([]);
    assert.equal(h.frames.size, 0);
});
