// Shared map-token transitions, including the pixel beams between killed forces and the tanks.
(function () {
    'use strict';

    const DURATION = 1100;
    const active = new Set();
    let overlay = null;
    let frame = 0;
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');

    function ensureOverlay() {
        if (overlay && overlay.isConnected) return overlay;
        overlay = document.createElement('canvas');
        overlay.className = 'fx-overlay';
        overlay.setAttribute('aria-hidden', 'true');
        (document.querySelector('.map-stage') || document.body).appendChild(overlay);
        return overlay;
    }

    function resizeOverlay() {
        const dpr = window.devicePixelRatio || 1;
        const width = Math.round(window.innerWidth * dpr);
        const height = Math.round(window.innerHeight * dpr);
        if (overlay.width !== width || overlay.height !== height) {
            overlay.width = width;
            overlay.height = height;
        }
        return dpr;
    }

    function rotationOf(el) {
        const host = el.closest('[style*="--rot"]');
        if (!host) return 0;
        const value = parseFloat(getComputedStyle(host).getPropertyValue('--rot'));
        return Number.isFinite(value) ? value * Math.PI / 180 : 0;
    }

    // Renders the visible images of an element into an offscreen canvas, in viewport coordinates.
    function snapshot(el) {
        const images = (el.tagName === 'IMG' ? [el] : [...el.querySelectorAll('img')])
            .filter(img => img.complete && img.naturalWidth > 0);
        if (images.length === 0) return null;

        const rects = images.map(img => img.getBoundingClientRect()).filter(r => r.width > 0 && r.height > 0);
        if (rects.length === 0) return null;

        const left = Math.min(...rects.map(r => r.left));
        const top = Math.min(...rects.map(r => r.top));
        const right = Math.max(...rects.map(r => r.right));
        const bottom = Math.max(...rects.map(r => r.bottom));
        const dpr = window.devicePixelRatio || 1;
        const canvas = document.createElement('canvas');
        canvas.width = Math.max(1, Math.ceil((right - left) * dpr));
        canvas.height = Math.max(1, Math.ceil((bottom - top) * dpr));
        const ctx = canvas.getContext('2d');

        images.forEach((img, i) => {
            const r = rects[i];
            if (!r) return;
            const rotation = rotationOf(img);
            // An axis-aligned bounding box of a rotated square image is larger than the image itself.
            const shrink = Math.abs(Math.cos(rotation)) + Math.abs(Math.sin(rotation));
            const w = r.width / shrink;
            const h = r.height / shrink;
            ctx.save();
            ctx.translate((r.left - left + r.width / 2) * dpr, (r.top - top + r.height / 2) * dpr);
            ctx.rotate(rotation);
            ctx.drawImage(img, -w / 2 * dpr, -h / 2 * dpr, w * dpr, h * dpr);
            ctx.restore();
        });

        return { canvas, left, top, width: right - left, height: bottom - top };
    }

    function particlesFor(shot, inward) {
        const dpr = window.devicePixelRatio || 1;
        const size = Math.min(shot.width, shot.height);
        const ctx = shot.canvas.getContext('2d');
        let data;
        try {
            data = ctx.getImageData(0, 0, shot.canvas.width, shot.canvas.height).data;
        } catch {
            return [];
        }

        const particles = [];
        const block = Math.max(2, Math.round(Math.max(2, size / 9) * dpr));
        const angle =  -Math.PI / 4 + (Math.random() - 0.5) * 0.6;
        for (let y = 0; y < shot.canvas.height; y += block) {
            for (let x = 0; x < shot.canvas.width; x += block) {
                const cx = Math.min(shot.canvas.width - 1, x + (block >> 1));
                const cy = Math.min(shot.canvas.height - 1, y + (block >> 1));
                if (data[(cy * shot.canvas.width + cx) * 4 + 3] < 40) continue;

                const sweep = (x / shot.canvas.width) * 0.45 + (1 - y / shot.canvas.height) * 0.15;
                const distance = (inward ? 18 + Math.random() * 46 : size * (1 + Math.random() * 2.2) + 14 + Math.random() * 30) * dpr;
                const spread = angle + (Math.random() - 0.5) * 1.2;
                particles.push({
                    sx: x, sy: y,
                    x: shot.left * dpr + x, y: shot.top * dpr + y,
                    dx: Math.cos(spread) * distance, dy: Math.sin(spread) * distance - (inward ? 10 : 24) * dpr * Math.random(),
                    delay: inward ? (1 - sweep) * 0.45 + Math.random() * 0.12 : sweep + Math.random() * 0.12,
                    spin: (Math.random() - 0.5) * 2
                });
            }
        }

        return particles.map(p => ({ ...p, size: block }));
    }

    // A brief ember glow where the tokens stood, so the loss is noticeable even for small tokens.
    function drawFlash(ctx, effect, elapsed) {
        if (elapsed > 0.55) return;
        const dpr = window.devicePixelRatio || 1;
        const shot = effect.shot;
        const x = (shot.left + shot.width / 2) * dpr;
        const y = (shot.top + shot.height / 2) * dpr;
        const t = elapsed / 0.55;
        const radius = Math.max(shot.width, shot.height) * (0.7 + t * 1.3) * dpr;
        const glow = ctx.createRadialGradient(x, y, 0, x, y, radius);
        glow.addColorStop(0, `rgba(255, 196, 120, ${0.75 * (1 - t)})`);
        glow.addColorStop(0.45, `rgba(240, 120, 40, ${0.45 * (1 - t)})`);
        glow.addColorStop(1, 'rgba(200, 70, 20, 0)');
        ctx.globalAlpha = 1;
        ctx.fillStyle = glow;
        ctx.beginPath();
        ctx.arc(x, y, radius, 0, Math.PI * 2);
        ctx.fill();
    }
    function tick(now) {
        const canvas = ensureOverlay();
        resizeOverlay();
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        for (const effect of [...active]) {
            const elapsed = (now - effect.start) / (effect.duration || DURATION);
            if (elapsed < 0) continue;

            if (effect.kind === 'token-in' || effect.kind === 'token-out') {
                drawToken(ctx, effect, Math.min(1, elapsed));
                ctx.globalAlpha = 1;
                if (elapsed >= 1) {
                    active.delete(effect);
                    effect.done?.();
                }
                continue;
            }

            if (effect.kind === 'hold' || effect.kind === 'beam') {
                if (effect.kind === 'hold') drawHold(ctx, effect, now);
                else drawBeam(ctx, effect, elapsed);
                ctx.globalAlpha = 1;
                if (elapsed >= 1) {
                    active.delete(effect);
                    effect.done?.();
                }
                continue;
            }

            if (effect.flash) drawFlash(ctx, effect, elapsed);

            for (const p of effect.particles) {
                // Each particle animates over 55% of the effect, starting at its own delay.
                let t = Math.min(1, Math.max(0, (elapsed - p.delay * 0.45) / 0.55));
                if (effect.inward) t = 1 - t;
                if (t >= 1) continue;

                const ease = t * t * (3 - 2 * t);
                ctx.globalAlpha = 1 - ease;
                ctx.drawImage(effect.shot.canvas, p.sx, p.sy, p.size, p.size,
                    p.x + p.dx * ease, p.y + p.dy * ease, p.size * (1 - 0.35 * ease), p.size * (1 - 0.35 * ease));
            }

            if (elapsed >= 1) {
                active.delete(effect);
                effect.done?.();
            }
        }

        ctx.globalAlpha = 1;
        frame = active.size > 0 ? requestAnimationFrame(tick) : 0;
        if (!frame) ctx.clearRect(0, 0, canvas.width, canvas.height);
    }

    function run(effect) {
        active.add(effect);
        if (!frame) frame = requestAnimationFrame(tick);
    }

    function byKey(key) {
        return document.querySelector(`[data-fx-key="${CSS.escape(key)}"]`);
    }

    function drawToken(ctx, effect, t) {
        const dpr = window.devicePixelRatio || 1;
        const { shot, type } = effect;
        const inward = effect.kind === 'token-in';
        const progress = inward ? 1 - t : t;
        const size = Math.max(shot.width, shot.height) * dpr;
        const x = (shot.left + shot.width / 2) * dpr;
        const y = (shot.top + shot.height / 2) * dpr;
        const color = type === 'terror' ? '110, 220, 220' : type === 'discovery' ? '185, 165, 255' : '235, 178, 85';

        ctx.save();
        ctx.translate(x, y);
        ctx.rotate((type === 'discovery' ? 1 : -0.3) * progress);
        const scale = inward ? 1 + 0.7 * progress : Math.max(0.05, 1 - 0.8 * progress);
        ctx.scale(scale, scale);
        ctx.globalAlpha = (1 - progress) * (effect.partial ? 0.5 : 1);
        ctx.drawImage(shot.canvas, -shot.width * dpr / 2, -shot.height * dpr / 2, shot.width * dpr, shot.height * dpr);
        ctx.restore();

        ctx.strokeStyle = `rgba(${color},${0.7 * Math.sin(Math.PI * t)})`;
        ctx.lineWidth = Math.max(1, 1.5 * dpr);
        ctx.beginPath();
        ctx.arc(x, y, size * (0.25 + 0.9 * (inward ? 1 - t : t)), 0, Math.PI * 2);
        ctx.stroke();
        for (let i = 0; i < 28; i++) {
            const angle = i * 2.39996 + progress * (type === 'spice' ? 3 : -1.5);
            const distance = size * (0.2 + progress * (0.6 + (i % 5) * 0.17));
            const lift = type === 'spice' ? progress * size * 1.3 : 0;
            ctx.globalAlpha = (1 - progress) * Math.sin(Math.PI * t);
            ctx.fillStyle = `rgb(${color})`;
            ctx.beginPath();
            ctx.arc(x + Math.cos(angle) * distance, y + Math.sin(angle) * distance - lift,
                Math.max(1, size * 0.035 * (1 - progress)), 0, Math.PI * 2);
            ctx.fill();
        }
    }

    function combineShots(shots) {
        const left = Math.min(...shots.map(s => s.left));
        const top = Math.min(...shots.map(s => s.top));
        const right = Math.max(...shots.map(s => s.left + s.width));
        const bottom = Math.max(...shots.map(s => s.top + s.height));
        const dpr = window.devicePixelRatio || 1;
        const canvas = document.createElement('canvas');
        canvas.width = Math.max(1, Math.ceil((right - left) * dpr));
        canvas.height = Math.max(1, Math.ceil((bottom - top) * dpr));
        const ctx = canvas.getContext('2d');
        for (const shot of shots) {
            ctx.drawImage(shot.canvas, (shot.left - left) * dpr, (shot.top - top) * dpr, shot.width * dpr, shot.height * dpr);
        }
        return { canvas, left, top, width: right - left, height: bottom - top };
    }

    function snapshotCount(el) {
        const rect = el.getBoundingClientRect();
        if (!rect.width || !rect.height || !el.offsetWidth || !el.offsetHeight) return null;
        const style = getComputedStyle(el);
        const shadow = (style.textShadow.match(/-?[\d.]+px/g) || []).map(parseFloat);
        return {
            rect, width: el.offsetWidth, height: el.offsetHeight, rotation: rotationOf(el),
            font: style.font, color: style.color, strokeColor: style.webkitTextStrokeColor,
            strokeWidth: parseFloat(style.webkitTextStrokeWidth) || 0,
            shadowX: shadow[0] || 0, shadowY: shadow[1] || 0, shadowBlur: shadow[2] || 0
        };
    }

    function countShot(count, amount, offsetX = 0, offsetY = 0) {
        if (!count) return null;
        const { rect, width, height, rotation } = count;
        const cos = Math.abs(Math.cos(rotation)), sin = Math.abs(Math.sin(rotation));
        const scaleX = rect.width / (width * cos + height * sin);
        const scaleY = rect.height / (width * sin + height * cos);
        const padding = Math.max(3, 2 * (count.shadowBlur + count.strokeWidth)) * Math.max(scaleX, scaleY);
        const dpr = window.devicePixelRatio || 1;
        const canvas = document.createElement('canvas');
        canvas.width = Math.max(1, Math.ceil((rect.width + 2 * padding) * dpr));
        canvas.height = Math.max(1, Math.ceil((rect.height + 2 * padding) * dpr));
        const ctx = canvas.getContext('2d');
        ctx.translate((rect.width / 2 + padding) * dpr, (rect.height / 2 + padding) * dpr);
        ctx.rotate(rotation);
        ctx.scale(scaleX * dpr, scaleY * dpr);
        ctx.font = count.font;
        ctx.textAlign = 'left';
        ctx.textBaseline = 'alphabetic';
        ctx.fillStyle = count.color;
        ctx.strokeStyle = count.strokeColor;
        ctx.lineWidth = count.strokeWidth;
        ctx.shadowColor = count.strokeColor;
        ctx.shadowOffsetX = count.shadowX;
        ctx.shadowOffsetY = count.shadowY;
        ctx.shadowBlur = count.shadowBlur;
        const text = String(amount);
        const metrics = ctx.measureText(text);
        const ascent = metrics.fontBoundingBoxAscent ?? metrics.actualBoundingBoxAscent ?? height * 0.8;
        const descent = metrics.fontBoundingBoxDescent ?? metrics.actualBoundingBoxDescent ?? height * 0.2;
        const baseline = (ascent - descent) / 2;
        if (count.strokeWidth > 0) ctx.strokeText(text, -width / 2, baseline);
        ctx.fillText(text, -width / 2, baseline);
        return {
            canvas, left: rect.left + offsetX - padding, top: rect.top + offsetY - padding,
            width: rect.width + 2 * padding, height: rect.height + 2 * padding
        };
    }

    function forceShot(tokens, count, amount, offsetX = 0, offsetY = 0) {
        const shots = tokens.map(t => t.shot);
        const label = countShot(count, amount, offsetX, offsetY);
        if (label) shots.push(label);
        return combineShots(shots);
    }

    let mapScene = null;
    let mapRoot = null;
    let mapLayout = '';
    let mapTokens = new Map();
    let syncRevision = 0;
    let pendingMapSync = null;
    const killedKeys = new Set();
    const hiddenTokens = new WeakMap();
    // Keep incoming images and the displayed count in sync even if Blazor replaces their DOM nodes.
    const forceArrivals = new Map();
    const heldCountElements = new WeakMap();
    const mapSelector = '[data-fx-key^="force-"], [data-fx-key^="spice-"], [data-fx-key^="terror-"], [data-fx-key^="ambassador-"], [data-fx-key^="discovery-"]';

    function hideTokens(tokens) {
        const owner = {};
        for (const { el } of tokens) {
            let owners = hiddenTokens.get(el);
            if (!owners) hiddenTokens.set(el, owners = new Set());
            owners.add(owner);
            el.style.visibility = 'hidden';
        }
        return () => {
            for (const { el } of tokens) {
                const owners = hiddenTokens.get(el);
                if (!owners?.delete(owner) || owners.size) continue;
                hiddenTokens.delete(el);
                el.style.visibility = '';
            }
        };
    }

    function forceCountElement(el) {
        return el.tagName === 'IMG' ? null : el.querySelector('[data-fx-count] > span');
    }

    function attachForceCount(state, el) {
        if (state.el !== el) {
            if (state.el && heldCountElements.get(state.el) === state) {
                heldCountElements.delete(state.el);
                state.el.removeAttribute('data-fx-previous');
            }
            const previous = el && heldCountElements.get(el);
            if (previous && previous !== state) previous.el = null;
            state.el = el;
            if (el) heldCountElements.set(el, state);
        }
        if (el) el.setAttribute('data-fx-previous', state.shown > 0 ? String(state.shown) : '');
    }

    function releaseForceArrival(state) {
        attachForceCount(state, null);
        for (const hold of state.pending) hold.releaseImages?.();
        state.pending.clear();
        if (forceArrivals.get(state.key) === state) forceArrivals.delete(state.key);
    }

    function reserveForceArrival(key, el, previous, current) {
        let state = forceArrivals.get(key);
        if (!state) {
            state = { key, el: null, shown: previous, expected: current, pending: new Set() };
            forceArrivals.set(key, state);
        }
        state.expected = current;
        attachForceCount(state, el);
        return state;
    }

    function bindArrivalImages(hold, imgs) {
        if (!hold.indices) return;
        const elements = hold.indices.map(index => imgs[index]).filter(Boolean);
        if (elements.length === hold.elements.length && elements.every((el, i) => el === hold.elements[i])) return;
        const release = hideTokens(elements.map(el => ({ el })));
        hold.releaseImages?.();
        hold.elements = elements;
        hold.releaseImages = release;
    }

    function holdForceArrival(state, amount, indices = null, imgs = []) {
        const hold = { indices, elements: [], releaseImages: null };
        state.pending.add(hold);
        bindArrivalImages(hold, imgs);
        return arrived => {
            if (!state.pending.delete(hold)) return;
            hold.releaseImages?.();
            if (arrived) state.shown = Math.min(state.expected, state.shown + amount);
            if (state.pending.size) attachForceCount(state, state.el);
            else releaseForceArrival(state);
        };
    }

    function reconcileForceArrivals(elements) {
        const byKey = new Map(elements.filter(el => el.dataset.fxKey.startsWith('force-')).map(el => [el.dataset.fxKey, el]));
        for (const state of [...forceArrivals.values()]) {
            const el = byKey.get(state.key);
            const imgs = el ? [...el.querySelectorAll('img')] : [];
            const count = imgs.length;
            if (count < state.expected) {
                for (const effect of [...active]) {
                    if (effect.mapTransition && effect.forceKey === state.key) {
                        active.delete(effect);
                        effect.done?.();
                    }
                }
                if (forceArrivals.get(state.key) !== state) continue;
                state.shown = Math.max(0, state.shown - (state.expected - count));
            }
            state.expected = count;
            attachForceCount(state, el ? forceCountElement(el) : null);
            for (const hold of state.pending) bindArrivalImages(hold, imgs);
        }
    }

    function cancelMapTransitions() {
        for (const effect of [...active]) {
            if (!effect.mapTransition) continue;
            active.delete(effect);
            effect.done?.();
        }
        for (const state of [...forceArrivals.values()]) releaseForceArrival(state);
    }

    function releasePendingSync(pending) {
        if (!pending) return;
        for (const release of pending.releases.splice(0)) release();
        if (pendingMapSync === pending) pendingMapSync = null;
    }

    function layoutOf(root) {
        const rect = root.getBoundingClientRect();
        return [rect.left, rect.top, rect.width, rect.height, window.devicePixelRatio].map(n => Math.round(n * 10)).join(',');
    }

    // Capture visual state before a later Blazor render can reuse a token's DOM nodes.
    window.FxSyncMapTokens = async function (scene) {
        const revision = ++syncRevision;
        releasePendingSync(pendingMapSync);
        const root = document.getElementById('planetmap_inner');
        if (!root) {
            cancelMapTransitions();
            return;
        }
        const elements = [...root.querySelectorAll(mapSelector)];
        const resetAtStart = scene !== mapScene || root !== mapRoot || layoutOf(root) !== mapLayout || document.hidden || reducedMotion.matches;
        const pending = { releases: [] };
        pendingMapSync = pending;
        if (resetAtStart) {
            cancelMapTransitions();
        } else {
            reconcileForceArrivals(elements);
            for (const el of elements) {
                const key = el.dataset.fxKey;
                if (!key.startsWith('force-')) continue;
                const imgs = [...el.querySelectorAll('img')];
                const before = mapTokens.get(key)?.tokens.length || 0;
                if (imgs.length <= before) continue;
                pending.releases.push(hideTokens(imgs.slice(before).map(img => ({ el: img }))));
                const state = reserveForceArrival(key, forceCountElement(el), before, imgs.length);
                const release = holdForceArrival(state, 0);
                pending.releases.push(() => release(false));
            }
        }
        const images = elements.flatMap(el => el.tagName === 'IMG' ? [el] : [...el.querySelectorAll('img')]);
        await Promise.all(images.filter(img => !img.complete).map(img => new Promise(resolve => {
            img.addEventListener('load', resolve, { once: true });
            img.addEventListener('error', resolve, { once: true });
        })));
        if (revision !== syncRevision || !root.isConnected) {
            releasePendingSync(pending);
            return;
        }

        const layout = layoutOf(root);
        const reset = resetAtStart || layout !== mapLayout || document.hidden || reducedMotion.matches;
        const next = new Map();
        for (const el of elements) {
            const key = el.dataset.fxKey;
            const imgs = el.tagName === 'IMG' ? [el] : [...el.querySelectorAll('img')];
            const tokens = imgs.map((img, index) => ({ el: img, index, shot: snapshot(img) })).filter(t => t.shot);
            if (!tokens.length) continue;
            const label = key.startsWith('force-') ? forceCountElement(el) : null;
            next.set(key, {
                key, type: key.split('-')[0], tokens, imgs, label, count: label ? snapshotCount(label) : null,
                amount: Number(el.dataset.fxAmount || tokens.length),
                group: /^force-/.test(key) ? key.replace(/^force-[^-]+-/, '') : null
            });
        }

        if (reset) {
            releasePendingSync(pending);
            if (!resetAtStart) cancelMapTransitions();
        } else {
            const losses = [];
            const gains = [];
            for (const [key, before] of mapTokens) {
                const after = next.get(key);
                if (before.type === 'force') {
                    const count = before.tokens.length - (after?.tokens.length || 0);
                    if (count > 0 && !killedKeys.has(key)) losses.push({ ...before, tokens: before.tokens.slice(-count) });
                } else if (!after || after.amount < before.amount) {
                    run({
                        kind: 'token-out', type: before.type, shot: combineShots(before.tokens.map(t => t.shot)),
                        partial: !!after, start: performance.now(), duration: 950, mapTransition: true
                    });
                }
            }
            for (const [key, after] of next) {
                const before = mapTokens.get(key);
                if (after.type === 'force') {
                    const count = after.tokens.length - (before?.tokens.length || 0);
                    if (count > 0) gains.push({ ...after, previous: before?.tokens.length || 0, tokens: after.tokens.slice(-count) });
                } else if (!before && after.type !== 'spice') {
                    tokenArrival(after);
                }
            }
            for (const gain of gains) {
                const arrivals = [];
                while (gain.tokens.length) {
                    const loss = losses.find(l => l.group === gain.group && l.tokens.length);
                    const count = loss ? Math.min(loss.tokens.length, gain.tokens.length) : gain.tokens.length;
                    arrivals.push({
                        arriving: gain.tokens.splice(0, count), loss,
                        departing: loss ? loss.tokens.splice(0, count) : null
                    });
                }
                const state = reserveForceArrival(gain.key, gain.label, gain.previous, gain.amount);
                for (const arrival of arrivals) {
                    arrival.done = holdForceArrival(state, arrival.arriving.length, arrival.arriving.map(t => t.index), gain.imgs);
                }
                for (const [index, { arriving, loss, departing, done: finish }] of arrivals.entries()) {
                    const count = gain.count;
                    const gap = count ? 2 * count.rect.height / count.height : 0;
                    const offsetY = count && (gain.previous > 0 || arrivals.length > 1) ? -count.rect.height - gap : 0;
                    const offsetX = count && arrivals.length > 1 ? (index - (arrivals.length - 1) / 2) * (count.rect.width + gap) : 0;
                    const target = forceShot(arriving, count, arriving.length, offsetX, offsetY);
                    const done = () => finish(true);
                    if (loss) {
                        const shot = forceShot(departing, loss.count, departing.length);
                        const effect = { kind: 'hold', shot, start: performance.now(), mapTransition: true, forceKey: gain.key, done };
                        if (beam({ shot, block: blockSize(shot), effect }, target, done)) {
                            effect.duration = 1000;
                            run(effect);
                        } else done();
                    } else {
                        run({
                            kind: 'scatter', shot: target, particles: particlesFor(target, true), inward: true,
                            start: performance.now(), duration: 850, mapTransition: true, forceKey: gain.key, done
                        });
                    }
                }
            }
            for (const loss of losses.filter(l => l.tokens.length)) {
                const shot = forceShot(loss.tokens, loss.count, loss.tokens.length);
                run({ kind: 'scatter', shot, particles: particlesFor(shot, false), flash: true,
                    start: performance.now(), duration: 850, mapTransition: true });
            }
            releasePendingSync(pending);
        }
        mapScene = scene;
        mapRoot = root;
        mapLayout = layout;
        mapTokens = next;
        killedKeys.clear();
    };

    function tokenArrival(token) {
        run({
            kind: 'token-in', type: token.type, shot: combineShots(token.tokens.map(t => t.shot)),
            start: performance.now(), duration: 750, mapTransition: true, done: hideTokens(token.tokens)
        });
    }

    // Map force stacks are remembered for a few seconds, so a killed stack can still dissolve where it stood
    // even when the DOM was already updated by the time the kill is detected.
    const HISTORY_MS = 4000;
    const memory = new Map();
    let memoryTimer = 0;

    function remember() {
        const now = performance.now();
        for (const el of document.querySelectorAll('[data-fx-key^="force-"]')) {
            const tokens = [...el.querySelectorAll('img')]
                .filter(img => img.complete && img.naturalWidth > 0)
                .map(img => ({ img, rect: img.getBoundingClientRect(), rotation: rotationOf(img) }))
                .filter(t => t.rect.width > 0);
            if (tokens.length === 0) continue;
            const key = el.dataset.fxKey;
            const history = memory.get(key) || [];
            const last = history[history.length - 1];
            const same = last && last.tokens.length === tokens.length
                && Math.abs(last.tokens[0].rect.left - tokens[0].rect.left) < 1 && Math.abs(last.tokens[0].rect.top - tokens[0].rect.top) < 1;
            if (same) last.time = now;
            else history.push({ time: now, tokens });
            memory.set(key, history);
        }

        for (const [key, history] of memory) {
            while (history.length > 1 && now - history[1].time > HISTORY_MS) history.shift();
            if (history.length === 0 || now - history[history.length - 1].time > HISTORY_MS) memory.delete(key);
        }
    }

    function ensureMemory() {
        if (!memoryTimer) memoryTimer = setInterval(() => { if (!document.hidden) remember(); }, 350);
    }

    function shotOf(tokens) {
        const rects = tokens.map(t => t.rect);
        const left = Math.min(...rects.map(r => r.left));
        const top = Math.min(...rects.map(r => r.top));
        const right = Math.max(...rects.map(r => r.right));
        const bottom = Math.max(...rects.map(r => r.bottom));
        const dpr = window.devicePixelRatio || 1;
        const canvas = document.createElement('canvas');
        canvas.width = Math.max(1, Math.ceil((right - left) * dpr));
        canvas.height = Math.max(1, Math.ceil((bottom - top) * dpr));
        const ctx = canvas.getContext('2d');
        for (const t of tokens) {
            const shrink = Math.abs(Math.cos(t.rotation)) + Math.abs(Math.sin(t.rotation));
            const w = t.rect.width / shrink;
            const h = t.rect.height / shrink;
            ctx.save();
            ctx.translate((t.rect.left - left + t.rect.width / 2) * dpr, (t.rect.top - top + t.rect.height / 2) * dpr);
            ctx.rotate(t.rotation);
            ctx.drawImage(t.img, -w / 2 * dpr, -h / 2 * dpr, w * dpr, h * dpr);
            ctx.restore();
        }
        return { canvas, left, top, width: right - left, height: bottom - top };
    }

    // Opaque pixel blocks of a snapshot, in snapshot and viewport coordinates.
    function blocksOf(shot, block) {
        const dpr = window.devicePixelRatio || 1;
        let data;
        try {
            data = shot.canvas.getContext('2d').getImageData(0, 0, shot.canvas.width, shot.canvas.height).data;
        } catch {
            return [];
        }
        const blocks = [];
        for (let y = 0; y < shot.canvas.height; y += block) {
            for (let x = 0; x < shot.canvas.width; x += block) {
                const cx = Math.min(shot.canvas.width - 1, x + (block >> 1));
                const cy = Math.min(shot.canvas.height - 1, y + (block >> 1));
                if (data[(cy * shot.canvas.width + cx) * 4 + 3] < 40) continue;
                blocks.push({ sx: x, sy: y, x: shot.left * dpr + x, y: shot.top * dpr + y, u: x / shot.canvas.width, v: y / shot.canvas.height });
            }
        }
        return blocks;
    }

    function blockSize(shot) {
        const dpr = window.devicePixelRatio || 1;
        return Math.max(2, Math.round(Math.max(1.5, Math.min(shot.width, shot.height) / 11) * dpr));
    }

    const factionOfSource = key => (/^force-.+-([A-Za-z]+)-[fs]$/.exec(key) || [])[1] || null;
    const factionOfTank = key => (/^tank-([A-Za-z]+)-[fs]-/.exec(key) || [])[1] || null;

    // Killed stacks wait here briefly (still drawn where they stood) until their tanks token shows up to beam to.
    const BEAM_WAIT_MS = 1100;
    const pending = [];

    function drawHold(ctx, effect, now) {
        const dpr = window.devicePixelRatio || 1;
        const shot = effect.shot;
        const pulse = 0.5 + 0.5 * Math.sin((now - effect.start) / 55);
        ctx.globalAlpha = 1;
        ctx.drawImage(shot.canvas, shot.left * dpr, shot.top * dpr, shot.width * dpr, shot.height * dpr);
        ctx.globalCompositeOperation = 'lighter';
        ctx.globalAlpha = 0.18 + 0.22 * pulse;
        ctx.drawImage(shot.canvas, shot.left * dpr, shot.top * dpr, shot.width * dpr, shot.height * dpr);
        ctx.globalCompositeOperation = 'source-over';
    }

    function bezier(a, c, b, t) {
        const u = 1 - t;
        return u * u * a + 2 * u * t * c + t * t * b;
    }

    // Each source pixel block lifts off its token, streaks along an arc and lands on its place in the tanks token.
    function drawBeam(ctx, effect, elapsed) {
        const dpr = window.devicePixelRatio || 1;
        const src = effect.shot;
        const dst = effect.target;
        const travel = 0.42;

        if (elapsed < 0.4) drawFlash(ctx, { shot: src }, elapsed * 1.4);

        for (const p of effect.particles) {
            const t = (elapsed - p.delay) / travel;
            if (t < 0) {
                // Not yet departed: still part of the dissolving token, flickering just before lift-off.
                const flicker = t > -0.25 ? 0.55 + 0.45 * Math.random() : 1;
                ctx.globalAlpha = flicker;
                ctx.drawImage(src.canvas, p.src.sx, p.src.sy, p.ss, p.ss, p.src.x, p.src.y, p.ss, p.ss);
                continue;
            }
            if (t >= 1) {
                ctx.globalAlpha = 1;
                ctx.drawImage(dst.canvas, p.dst.sx, p.dst.sy, p.ds, p.ds, p.dst.x, p.dst.y, p.ds, p.ds);
                continue;
            }

            const e = t < 0.5 ? 2 * t * t : 1 - Math.pow(-2 * t + 2, 2) / 2;
            const x = bezier(p.src.x, p.cx, p.dst.x, e);
            const y = bezier(p.src.y, p.cy, p.dst.y, e);
            const size = Math.max(1, (p.ss + (p.ds - p.ss) * e) * (1 - 0.45 * Math.sin(Math.PI * t)));

            // Light streak behind the travelling pixel.
            const back = Math.max(0, e - 0.08);
            const tx = bezier(p.src.x, p.cx, p.dst.x, back);
            const ty = bezier(p.src.y, p.cy, p.dst.y, back);
            ctx.globalCompositeOperation = 'lighter';
            ctx.globalAlpha = 0.5 * Math.sin(Math.PI * t);
            ctx.strokeStyle = 'rgba(255, 190, 110, 1)';
            ctx.lineWidth = Math.max(1, size * 0.6);
            ctx.beginPath();
            ctx.moveTo(tx + size / 2, ty + size / 2);
            ctx.lineTo(x + size / 2, y + size / 2);
            ctx.stroke();
            ctx.globalCompositeOperation = 'source-over';

            ctx.globalAlpha = 1 - e;
            ctx.drawImage(src.canvas, p.src.sx, p.src.sy, p.ss, p.ss, x, y, size, size);
            ctx.globalAlpha = e;
            ctx.drawImage(dst.canvas, p.dst.sx, p.dst.sy, p.ds, p.ds, x, y, size, size);
        }

        // Soft glow where the token materialises.
        if (elapsed > 0.35) {
            const t = Math.min(1, (elapsed - 0.35) / 0.65);
            const gx = (dst.left + dst.width / 2) * dpr;
            const gy = (dst.top + dst.height / 2) * dpr;
            const r = Math.max(dst.width, dst.height) * (0.6 + 0.6 * t) * dpr;
            const glow = ctx.createRadialGradient(gx, gy, 0, gx, gy, r);
            const a = 0.45 * Math.sin(Math.PI * t);
            glow.addColorStop(0, `rgba(255, 210, 140, ${a})`);
            glow.addColorStop(1, 'rgba(240, 130, 50, 0)');
            ctx.globalCompositeOperation = 'lighter';
            ctx.globalAlpha = 1;
            ctx.fillStyle = glow;
            ctx.beginPath();
            ctx.arc(gx, gy, r, 0, Math.PI * 2);
            ctx.fill();
            ctx.globalCompositeOperation = 'source-over';
        }
    }

    function beam(item, target, done) {
        const dpr = window.devicePixelRatio || 1;
        const src = item.shot;
        const srcBlocks = blocksOf(src, item.block);
        const ds = blockSize(target);
        const dstBlocks = blocksOf(target, ds);
        if (srcBlocks.length === 0 || dstBlocks.length === 0) return false;

        // Pair source and destination blocks by relative position so the token keeps its shape in transit.
        const order = list => [...list].sort((a, b) => (a.v - b.v) || (a.u - b.u));
        const s = order(srcBlocks);
        const d = order(dstBlocks);
        const n = Math.max(s.length, d.length);
        const sx = (src.left + src.width / 2) * dpr, sy = (src.top + src.height / 2) * dpr;
        const dx = (target.left + target.width / 2) * dpr, dy = (target.top + target.height / 2) * dpr;
        const lift = Math.min(220, Math.hypot(dx - sx, dy - sy) * 0.35) * dpr;
        const particles = [];
        for (let k = 0; k < n; k++) {
            const a = s[Math.floor(k * s.length / n)];
            const b = d[Math.floor(k * d.length / n)];
            const jitter = (Math.random() - 0.5) * 60 * dpr;
            particles.push({
                src: a, dst: b, ss: item.block, ds,
                cx: (a.x + b.x) / 2 + jitter, cy: Math.min(a.y, b.y) - lift + jitter * 0.5,
                // Dissolve sweeps from the top of the source token down, with some noise.
                delay: 0.06 + a.v * 0.3 + Math.random() * 0.18
            });
        }

        item.effect.kind = 'beam';
        item.effect.target = target;
        item.effect.particles = particles;
        item.effect.start = performance.now();
        item.effect.duration = 1900;
        item.effect.done = done;
        return true;
    }

    // Unclaimed stacks (no tanks token appeared) simply blow apart where they stood.
    function scatter(item) {
        item.effect.kind = 'scatter';
        item.effect.particles = particlesFor(item.shot, false);
        item.effect.start = performance.now();
        item.effect.flash = true;
        item.effect.duration = 1700;
    }

    // Called when forces were killed: each item names a map stack and how many tokens it had before and has now.
    window.FxDisintegrate = function (items) {
        if (reducedMotion.matches) return;
        remember();
        for (const item of items || []) {
            killedKeys.add(item.key);
            const history = memory.get(item.key) || [];
            const before = [...history].reverse().find(h => h.tokens.length > item.to);
            if (!before) continue;
            const lost = before.tokens.slice(Math.min(item.to, before.tokens.length - 1));
            const shot = shotOf(lost);
            const now = performance.now();
            const effect = { kind: 'hold', shot, particles: [], start: now, duration: BEAM_WAIT_MS + 400 };
            const entry = { faction: factionOfSource(item.key), shot, block: blockSize(shot), effect, time: now };
            pending.push(entry);
            run(effect);
            setTimeout(() => {
                const i = pending.indexOf(entry);
                if (i >= 0) {
                    pending.splice(i, 1);
                    scatter(entry);
                }
            }, BEAM_WAIT_MS);
            memory.delete(item.key);
        }
    };

    ensureMemory();
    // Called after new tokens appear in the tanks: they are beamed in from the killed stacks, or assemble from scattered pixels.
    window.FxIntegrate = function (keys, delay) {
        if (reducedMotion.matches) return;
        const wait = Math.max(0, delay || 0);
        const byFaction = new Map();
        for (const key of keys || []) {
            const el = byKey(key);
            if (!el) continue;
            el.style.visibility = 'hidden';
            const faction = factionOfTank(key);
            if (faction && pending.some(p => p.faction === faction)) {
                if (!byFaction.has(faction)) byFaction.set(faction, []);
                byFaction.get(faction).push(el);
            } else {
                setTimeout(() => assemble(el), wait);
            }
        }

        for (const [faction, els] of byFaction) {
            const sources = pending.filter(p => p.faction === faction);
            sources.forEach(s => pending.splice(pending.indexOf(s), 1));
            beamWhenReady(sources, els, 0);
        }
    };

    function assemble(el) {
        const shot = snapshot(el);
        if (!shot) {
            el.style.visibility = '';
            return;
        }
        run({
            kind: 'scatter', shot, particles: particlesFor(shot, true), inward: true, start: performance.now(),
            done: () => { el.style.visibility = ''; }
        });
    }

    // Tank tokens are freshly rendered; their images may still be loading.
    function beamWhenReady(sources, els, attempt) {
        const tokens = els.flatMap(el => [...el.querySelectorAll('img'), ...(el.tagName === 'IMG' ? [el] : [])])
            .map(img => ({ img, rect: img.getBoundingClientRect(), rotation: rotationOf(img) }))
            .filter(t => t.rect.width > 0);
        const ready = tokens.length > 0 && tokens.every(t => t.img.complete && t.img.naturalWidth > 0);
        if (!ready && attempt < 12) {
            setTimeout(() => beamWhenReady(sources, els, attempt + 1), 60);
            return;
        }

        const show = () => els.forEach(el => { el.style.visibility = ''; });
        const target = ready ? shotOf(tokens) : null;
        let started = false;
        for (const source of sources) {
            if (target && beam(source, target, started ? null : show)) started = true;
            else scatter(source);
        }
        if (!started) show();
    }})();

window.ScrollToEdge = function (element, toEnd) {
    if (element) element.scrollTop = toEnd ? element.scrollHeight : 0;
};

// Lets the user resize the (bottom-left anchored) report panel by dragging its top edge, right edge or top-right corner.
window.InitReportResize = function (panel) {
    const dock = panel && panel.closest('.map-report-dock');
    if (!dock || dock.dataset.resizeInit) return;
    dock.dataset.resizeInit = '1';

    const KEY = 'treachery.reportSize';
    const MIN_W = 180, MIN_H = 60, MARGIN = 14;

    const bounds = () => {
        const parent = dock.offsetParent || document.body;
        return { w: parent.clientWidth - dock.offsetLeft - MARGIN, h: dock.offsetTop + dock.offsetHeight - MARGIN };
    };

    const apply = (size) => {
        if (size && size.w) dock.style.width = size.w + 'px'; else dock.style.removeProperty('width');
        if (size && size.h) { dock.style.setProperty('--report-h', size.h + 'px'); dock.dataset.sized = '1'; }
        else { dock.style.removeProperty('--report-h'); delete dock.dataset.sized; }
    };

    try { apply(JSON.parse(localStorage.getItem(KEY))); } catch { }

    dock.addEventListener('pointerdown', (e) => {
        const handle = e.target.closest('.map-report-resize');
        if (!handle || e.button !== 0) return;
        e.preventDefault();
        const mode = handle.dataset.resize;
        const content = dock.querySelector('.map-report-panel');
        const startX = e.clientX, startY = e.clientY;
        const startW = dock.offsetWidth, startH = content ? content.offsetHeight : dock.offsetHeight;
        const max = bounds();
        const size = {};
        try { Object.assign(size, JSON.parse(localStorage.getItem(KEY)) || {}); } catch { }
        handle.setPointerCapture(e.pointerId);
        dock.classList.add('is-resizing');

        const move = (ev) => {
            if (mode !== 'y') size.w = Math.round(Math.max(MIN_W, Math.min(max.w, startW + ev.clientX - startX)));
            if (mode !== 'x') size.h = Math.round(Math.max(MIN_H, Math.min(max.h, startH - (ev.clientY - startY))));
            apply(size);
        };
        const end = () => {
            handle.removeEventListener('pointermove', move);
            handle.removeEventListener('pointerup', end);
            handle.removeEventListener('pointercancel', end);
            dock.classList.remove('is-resizing');
            try { localStorage.setItem(KEY, JSON.stringify(size)); } catch { }
        };
        handle.addEventListener('pointermove', move);
        handle.addEventListener('pointerup', end);
        handle.addEventListener('pointercancel', end);
    });

    dock.addEventListener('dblclick', (e) => {
        if (!e.target.closest('.map-report-resize')) return;
        apply(null);
        try { localStorage.removeItem(KEY); } catch { }
    });
};

// Top-down spice blow: cinnamon dust erupts from the sand, rains back down and settles, then fades away.
(function () {
    'use strict';

    const DURATION = 5800;
    const COLORS = [[196, 98, 45], [172, 80, 36], [214, 136, 72], [229, 171, 112], [146, 64, 30], [205, 118, 58]];
    let audio = null;

    const clamp = v => Math.max(0, Math.min(1, v));
    const smooth = v => { v = clamp(v); return v * v * (3 - 2 * v); };
    const easeOut = v => 1 - Math.pow(1 - clamp(v), 3);
    const pick = list => list[Math.floor(Math.random() * list.length)];
    const rgba = (c, a) => `rgba(${c[0]},${c[1]},${c[2]},${Math.max(0, a).toFixed(3)})`;

    // cx, cy and size are in the coordinate space of host; scale converts that space to screen pixels.
    function blow(host, cx, cy, size, scale) {
        const dpr = (window.devicePixelRatio || 1) * scale;
        const reach = Math.max(34 / scale, size * 2.6);
        const half = reach * 1.7;
        const canvas = document.createElement('canvas');
        canvas.setAttribute('aria-hidden', 'true');
        canvas.style.cssText = host === document.body
            ? `position:fixed;left:${cx - half}px;top:${cy - half}px;width:${half * 2}px;height:${half * 2}px;pointer-events:none;z-index:1540;`
            : `position:absolute;left:${cx - half}px;top:${cy - half}px;width:${half * 2}px;height:${half * 2}px;pointer-events:none;`;
        canvas.width = Math.round(half * 2 * dpr);
        canvas.height = Math.round(half * 2 * dpr);
        host.appendChild(canvas);
        const ctx = canvas.getContext('2d');
        const o = half * dpr;
        const u = dpr;

        const puffs = Array.from({ length: 20 }, (_, i) => ({
            angle: (i / 20) * Math.PI * 2 + (Math.random() - 0.5) * 0.5,
            dist: reach * (0.3 + Math.random() * 0.65),
            r0: size * (0.18 + Math.random() * 0.18),
            r1: size * (0.55 + Math.random() * 0.6),
            delay: Math.random() * 0.12,
            alpha: 0.45 + Math.random() * 0.3,
            color: pick(COLORS)
        }));

        const grains = Array.from({ length: 130 }, () => ({
            angle: Math.random() * Math.PI * 2,
            dist: reach * (0.15 + Math.pow(Math.random(), 0.7) * 1.05),
            r: 0.8 + Math.random() * 2.1,
            lift: 0.4 + Math.random() * 0.9,
            flight: 0.3 + Math.random() * 0.3,
            delay: Math.random() * 0.1,
            color: pick(COLORS)
        }));

        const start = performance.now();
        function frame(now) {
            const t = (now - start) / DURATION;
            if (t >= 1 || !canvas.isConnected) { canvas.remove(); return; }
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            const fadeAll = 1 - smooth((t - 0.72) / 0.28);

            // Spice settling on the sand
            const stainR = reach * (0.25 + 0.75 * easeOut(t / 0.55)) * u;
            const stain = ctx.createRadialGradient(o, o, 0, o, o, stainR);
            stain.addColorStop(0, rgba([168, 74, 32], 0.42 * fadeAll));
            stain.addColorStop(0.65, rgba([196, 98, 45], 0.22 * fadeAll));
            stain.addColorStop(1, rgba([196, 98, 45], 0));
            ctx.fillStyle = stain;
            ctx.beginPath(); ctx.arc(o, o, stainR, 0, Math.PI * 2); ctx.fill();

            // Shock ring across the sand
            if (t < 0.32) {
                const k = t / 0.32;
                ctx.strokeStyle = rgba([255, 214, 160], 0.55 * (1 - k));
                ctx.lineWidth = (5 - 4 * k) * u;
                ctx.beginPath(); ctx.arc(o, o, reach * (0.2 + 1.1 * easeOut(k)) * u, 0, Math.PI * 2); ctx.stroke();
            }

            // Grains thrown up towards the viewer, then raining back down
            for (const g of grains) {
                const k = clamp((t - g.delay) / g.flight);
                if (k <= 0) continue;
                const d = g.dist * easeOut(k) * u;
                const h = 4 * k * (1 - k) * g.lift;
                const x = o + Math.cos(g.angle) * d;
                const y = o + Math.sin(g.angle) * d;
                const r = g.r * (1 + h * 1.6) * u;
                const a = (k < 1 ? 1 : 0.85) * fadeAll;
                if (h > 0.02) {
                    ctx.fillStyle = rgba([40, 18, 6], 0.22 * a * (1 - h * 0.4));
                    ctx.beginPath(); ctx.arc(x + h * 7 * u, y + h * 9 * u, g.r * u, 0, Math.PI * 2); ctx.fill();
                }
                ctx.fillStyle = rgba(g.color, a);
                ctx.beginPath(); ctx.arc(x, y, r, 0, Math.PI * 2); ctx.fill();
            }

            // Billowing dust clouds, lit from the top-left
            for (const p of puffs) {
                const k = clamp((t - p.delay) / 0.8);
                if (k <= 0) continue;
                const d = p.dist * easeOut(k) * u;
                const r = (p.r0 + (p.r1 - p.r0) * easeOut(k)) * u;
                const a = p.alpha * smooth(k / 0.12) * (1 - smooth((t - 0.35) / 0.55));
                if (a <= 0.002) continue;
                const x = o + Math.cos(p.angle) * d;
                const y = o + Math.sin(p.angle) * d;
                const body = ctx.createRadialGradient(x, y, 0, x, y, r);
                body.addColorStop(0, rgba(p.color, a));
                body.addColorStop(0.55, rgba(p.color, a * 0.6));
                body.addColorStop(1, rgba(p.color, 0));
                ctx.fillStyle = body;
                ctx.beginPath(); ctx.arc(x, y, r, 0, Math.PI * 2); ctx.fill();
                const hx = x - r * 0.25, hy = y - r * 0.28;
                const light = ctx.createRadialGradient(hx, hy, 0, hx, hy, r * 0.6);
                light.addColorStop(0, rgba([246, 206, 152], a * 0.45));
                light.addColorStop(1, rgba([246, 206, 152], 0));
                ctx.fillStyle = light;
                ctx.beginPath(); ctx.arc(hx, hy, r * 0.6, 0, Math.PI * 2); ctx.fill();
            }

            // Hot core of the eruption
            if (t < 0.4) {
                const k = t / 0.4;
                const cr = size * (0.6 + 1.2 * easeOut(k)) * u;
                const core = ctx.createRadialGradient(o, o, 0, o, o, cr);
                core.addColorStop(0, rgba([255, 226, 170], 0.85 * (1 - k)));
                core.addColorStop(0.4, rgba([226, 140, 70], 0.6 * (1 - k)));
                core.addColorStop(1, rgba([196, 98, 45], 0));
                ctx.fillStyle = core;
                ctx.beginPath(); ctx.arc(o, o, cr, 0, Math.PI * 2); ctx.fill();
            }

            requestAnimationFrame(frame);
        }
        requestAnimationFrame(frame);
    }

    function noise(ac, seconds) {
        const buffer = ac.createBuffer(1, Math.ceil(ac.sampleRate * seconds), ac.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < data.length; i++) data[i] = Math.random() * 2 - 1;
        const src = ac.createBufferSource();
        src.buffer = buffer;
        return src;
    }

    function eruptionSound(volume) {
        try {
            const AC = window.AudioContext || window.webkitAudioContext;
            if (!AC) return;
            audio = audio || new AC();
            if (audio.state === 'suspended') audio.resume();
            const ac = audio;
            const now = ac.currentTime;
            const master = ac.createGain();
            master.gain.value = Math.min(1, volume) * 0.8;
            master.connect(ac.destination);

            // Low rumble from below the sand
            const rumble = ac.createOscillator();
            rumble.type = 'sine';
            rumble.frequency.setValueAtTime(70, now);
            rumble.frequency.exponentialRampToValueAtTime(32, now + 1.4);
            const rumbleGain = ac.createGain();
            rumbleGain.gain.setValueAtTime(0.0001, now);
            rumbleGain.gain.exponentialRampToValueAtTime(0.7, now + 0.05);
            rumbleGain.gain.exponentialRampToValueAtTime(0.0001, now + 1.5);
            rumble.connect(rumbleGain).connect(master);
            rumble.start(now);
            rumble.stop(now + 1.6);

            // Whoosh of the spice column
            const blast = noise(ac, 2.2);
            const band = ac.createBiquadFilter();
            band.type = 'bandpass';
            band.Q.value = 0.8;
            band.frequency.setValueAtTime(260, now);
            band.frequency.exponentialRampToValueAtTime(1500, now + 0.18);
            band.frequency.exponentialRampToValueAtTime(320, now + 1.9);
            const blastGain = ac.createGain();
            blastGain.gain.setValueAtTime(0.0001, now);
            blastGain.gain.exponentialRampToValueAtTime(0.9, now + 0.06);
            blastGain.gain.exponentialRampToValueAtTime(0.0001, now + 2.1);
            blast.connect(band).connect(blastGain).connect(master);
            blast.start(now);

            // Sand pattering back down
            const patter = noise(ac, 1.6);
            const high = ac.createBiquadFilter();
            high.type = 'highpass';
            high.frequency.value = 2600;
            const patterGain = ac.createGain();
            patterGain.gain.setValueAtTime(0.0001, now + 0.5);
            patterGain.gain.exponentialRampToValueAtTime(0.14, now + 0.85);
            patterGain.gain.exponentialRampToValueAtTime(0.0001, now + 2.0);
            patter.connect(high).connect(patterGain).connect(master);
            patter.start(now + 0.5);
        } catch (e) {
            console.debug('Spice blow sound failed', e);
        }
    }

    window.FxSpiceBlow = function (keys, volume) {
        const reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        let shown = false;
        for (const key of keys || []) {
            const el = document.querySelector(`[data-fx-key="${CSS.escape(key)}"]`);
            if (!el) continue;
            const rect = el.getBoundingClientRect();
            if (rect.width <= 0 || rect.height <= 0) continue;
            shown = true;
            if (reduceMotion) continue;
            // Draw inside the map so the effect is clipped by the map and stays below panels on top of it.
            const layer = el.closest('#planetmap_inner')?.querySelector(':scope > .map-fx-layer');
            const layerRect = layer?.getBoundingClientRect();
            if (layer && layer.offsetWidth > 0 && layerRect.width > 0) {
                const scale = layerRect.width / layer.offsetWidth;
                const cx = (rect.left + rect.width / 2 - layerRect.left) / scale;
                const cy = (rect.top + rect.height / 2 - layerRect.top) / scale;
                blow(layer, cx, cy, Math.max(rect.width, rect.height) / scale, scale);
            } else {
                blow(document.body, rect.left + rect.width / 2, rect.top + rect.height / 2, Math.max(rect.width, rect.height), 1);
            }
        }
        if (shown && volume > 0) eruptionSound(volume);
    };
})();
