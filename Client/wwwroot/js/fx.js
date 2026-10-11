// Pixel disintegration / integration effects for tokens that are killed on the map and arrive in the tanks.
(function () {
    'use strict';

    const DURATION = 1100;
    const active = new Set();
    let overlay = null;
    let frame = 0;

    function ensureOverlay() {
        if (overlay && overlay.isConnected) return overlay;
        overlay = document.createElement('canvas');
        overlay.className = 'fx-overlay';
        overlay.setAttribute('aria-hidden', 'true');
        document.body.appendChild(overlay);
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
        const block = Math.max(2, Math.round(Math.min(shot.width, shot.height) / 13)) * dpr;
        const ctx = shot.canvas.getContext('2d');
        let data;
        try {
            data = ctx.getImageData(0, 0, shot.canvas.width, shot.canvas.height).data;
        } catch {
            return [];
        }

        const particles = [];
        const angle = -Math.PI / 4 + (Math.random() - 0.5) * 0.6;
        for (let y = 0; y < shot.canvas.height; y += block) {
            for (let x = 0; x < shot.canvas.width; x += block) {
                const cx = Math.min(shot.canvas.width - 1, x + (block >> 1));
                const cy = Math.min(shot.canvas.height - 1, y + (block >> 1));
                if (data[(cy * shot.canvas.width + cx) * 4 + 3] < 40) continue;

                const sweep = (x / shot.canvas.width) * 0.45 + (1 - y / shot.canvas.height) * 0.15;
                const distance = (18 + Math.random() * 46) * dpr;
                const spread = angle + (Math.random() - 0.5) * 1.2;
                particles.push({
                    sx: x, sy: y,
                    x: shot.left * dpr + x, y: shot.top * dpr + y,
                    dx: Math.cos(spread) * distance, dy: Math.sin(spread) * distance - 10 * dpr * Math.random(),
                    delay: inward ? (1 - sweep) * 0.45 + Math.random() * 0.12 : sweep + Math.random() * 0.12,
                    spin: (Math.random() - 0.5) * 2
                });
            }
        }

        return particles.map(p => ({ ...p, size: block }));
    }

    function tick(now) {
        const canvas = ensureOverlay();
        resizeOverlay();
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        for (const effect of [...active]) {
            const elapsed = (now - effect.start) / DURATION;
            if (elapsed < 0) continue;

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

    // Called synchronously before the killed tokens are removed from the DOM.
    window.FxDisintegrate = function (keys) {
        for (const key of keys || []) {
            const el = byKey(key);
            const shot = el && snapshot(el);
            if (!shot) continue;
            run({ shot, particles: particlesFor(shot, false), inward: false, start: performance.now() });
        }
    };

    // Called after new tokens appear in the tanks: they assemble from scattered pixels.
    window.FxIntegrate = function (keys, delay) {
        const wait = Math.max(0, delay || 0);
        for (const key of keys || []) {
            const el = byKey(key);
            if (!el) continue;
            el.style.visibility = 'hidden';
            const begin = () => {
                const shot = snapshot(el);
                if (!shot) {
                    el.style.visibility = '';
                    return;
                }
                run({
                    shot, particles: particlesFor(shot, true), inward: true, start: performance.now(),
                    done: () => { el.style.visibility = ''; }
                });
            };
            setTimeout(begin, wait);
        }
    };
})();

window.ScrollToEdge = function (element, toEnd) {
    if (element) element.scrollTop = toEnd ? element.scrollHeight : 0;
};
