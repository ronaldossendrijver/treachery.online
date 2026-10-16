/*
 * Lightweight UI feedback: synthesized interface sounds (Web Audio, no assets) and press ripples.
 * Volume follows the "Sound effects volume" setting via SetUiSoundVolume; 0 mutes.
 */
(function () {
    "use strict";

    const interactiveSelector = [
        "button", "a[href]", "summary", "select",
        "input[type=checkbox]", "input[type=radio]",
        "[role=button]", "[role=tab]", "[role=menuitem]", "[role=option]",
        "img.image-hover.pointer", "img.pointer"
    ].join(",");
    const rippleSelector = ".ui-btn, .rail-pile-card, .map-faction-tab, .sheet-handle";
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

    let volume = 0;
    let context = null;
    let master = null;
    let noiseBuffer = null;
    let lastPlayed = 0;

    window.SetUiSoundVolume = function (value) {
        volume = Math.max(0, Math.min(1, Number(value) || 0));
        if (master) master.gain.value = masterGain();
    };

    function masterGain() {
        return Math.sqrt(volume) * 0.4;
    }

    function ensureContext() {
        if (context || volume <= 0) return context;
        const AudioContextClass = window.AudioContext || window.webkitAudioContext;
        if (!AudioContextClass) return null;
        context = new AudioContextClass();
        master = context.createGain();
        master.gain.value = masterGain();
        master.connect(context.destination);
        noiseBuffer = context.createBuffer(1, Math.floor(context.sampleRate * 0.08), context.sampleRate);
        const data = noiseBuffer.getChannelData(0);
        for (let i = 0; i < data.length; i++) data[i] = (Math.random() * 2 - 1) * Math.pow(1 - i / data.length, 2);
        return context;
    }

    function tone(type, from, to, start, duration, gain) {
        const osc = context.createOscillator();
        const amp = context.createGain();
        osc.type = type;
        osc.frequency.setValueAtTime(from, start);
        if (to !== from) osc.frequency.exponentialRampToValueAtTime(to, start + duration);
        amp.gain.setValueAtTime(0.0001, start);
        amp.gain.exponentialRampToValueAtTime(gain, start + 0.006);
        amp.gain.exponentialRampToValueAtTime(0.0001, start + duration);
        osc.connect(amp).connect(master);
        osc.start(start);
        osc.stop(start + duration + 0.02);
    }

    function noise(frequency, q, start, duration, gain) {
        const src = context.createBufferSource();
        const filter = context.createBiquadFilter();
        const amp = context.createGain();
        src.buffer = noiseBuffer;
        filter.type = "bandpass";
        filter.frequency.value = frequency;
        filter.Q.value = q;
        amp.gain.setValueAtTime(gain, start);
        amp.gain.exponentialRampToValueAtTime(0.0001, start + duration);
        src.connect(filter).connect(amp).connect(master);
        src.start(start);
        src.stop(start + duration + 0.02);
    }

    const sounds = {
        click(t) {
            noise(2600, 1.4, t, 0.035, 0.55);
            tone("sine", 210, 120, t, 0.05, 0.35);
        },
        tab(t) {
            noise(3400, 2, t, 0.025, 0.4);
            tone("triangle", 880, 760, t, 0.05, 0.12);
        },
        select(t) {
            tone("triangle", 640, 660, t, 0.08, 0.2);
            tone("triangle", 960, 980, t + 0.045, 0.11, 0.17);
            noise(3000, 2, t, 0.02, 0.25);
        },
        open(t) {
            tone("sine", 360, 620, t, 0.11, 0.2);
            noise(1800, 0.8, t, 0.06, 0.18);
        },
        close(t) {
            tone("sine", 600, 340, t, 0.1, 0.18);
            noise(1500, 0.8, t, 0.05, 0.15);
        },
        confirm(t) {
            noise(2400, 1.2, t, 0.03, 0.45);
            tone("triangle", 523.25, 523.25, t + 0.01, 0.2, 0.17);
            tone("triangle", 783.99, 783.99, t + 0.06, 0.24, 0.14);
            tone("sine", 1046.5, 1046.5, t + 0.06, 0.18, 0.05);
        },
        token(t) {
            noise(1250, 1.6, t, 0.045, 0.7);
            tone("sine", 320, 190, t, 0.06, 0.3);
            noise(1900, 2.2, t + 0.035, 0.03, 0.3);
        },
        denied(t) {
            tone("square", 150, 135, t, 0.06, 0.07);
            tone("square", 120, 105, t + 0.075, 0.08, 0.07);
            noise(500, 1, t, 0.03, 0.15);
        }
    };

    function play(name) {
        if (volume <= 0 || document.hidden) return;
        const now = performance.now();
        if (now - lastPlayed < 40) return;
        lastPlayed = now;
        if (!ensureContext()) return;
        if (context.state === "suspended") context.resume();
        sounds[name](context.currentTime + 0.005);
    }

    function isDisabled(el) {
        return el.disabled || el.getAttribute("aria-disabled") === "true" || el.closest("fieldset:disabled") != null;
    }

    function findTarget(start) {
        const direct = start.closest(interactiveSelector);
        if (direct) return direct;
        for (let el = start, depth = 0; el && el !== document.body && depth < 4; el = el.parentElement, depth++) {
            if (el.tagName === "LABEL" || el instanceof HTMLInputElement) return null;
            if (getComputedStyle(el).cursor === "pointer") return el;
        }
        return null;
    }

    function classify(el) {
        if (el.closest(".tanks-pile, #planetmap")) return "token";
        if (el.matches("input[type=checkbox], input[type=radio], img.image-hover, [role=option], [aria-pressed], [aria-selected=false]")) return "select";
        if (el.matches("[role=tab]")) return "tab";
        if (el.matches("summary")) return el.parentElement && el.parentElement.open ? "close" : "open";
        if (el.matches("[aria-expanded]")) return el.getAttribute("aria-expanded") === "true" ? "close" : "open";
        if (el.matches("[aria-haspopup]")) return "open";
        if (el.matches(".ui-btn-primary, .ui-btn-success, button[type=submit]")) return "confirm";
        return "click";
    }

    function pop(el) {
        if (reducedMotion.matches || !el.animate) return;
        el.animate(
            [{ transform: "scale(1)" }, { transform: "scale(1.12)" }, { transform: "scale(0.98)" }, { transform: "scale(1)" }],
            { duration: 280, easing: "cubic-bezier(0.2, 0.8, 0.3, 1.2)" });
    }

    function ripple(el, event) {
        if (reducedMotion.matches) return;
        const rect = el.getBoundingClientRect();
        if (rect.width === 0) return;
        if (getComputedStyle(el).position === "static") el.style.position = "relative";
        const host = document.createElement("span");
        host.className = "ui-ripple-host";
        host.setAttribute("aria-hidden", "true");
        const wave = document.createElement("span");
        wave.className = "ui-ripple";
        const size = Math.max(rect.width, rect.height) * 2.2;
        wave.style.setProperty("--ripple-size", size + "px");
        wave.style.left = (event.clientX - rect.left) + "px";
        wave.style.top = (event.clientY - rect.top) + "px";
        host.appendChild(wave);
        el.appendChild(host);
        setTimeout(() => host.remove(), 560);
    }

    document.addEventListener("pointerdown", event => {
        if (event.button !== 0 || !(event.target instanceof Element)) return;
        const el = event.target.closest(rippleSelector);
        if (el && !isDisabled(el)) ripple(el, event);
    }, { capture: true, passive: true });

    // Disabled targets get a distinct "denied" sound. Natively disabled controls don't receive click events,
    // so this is detected on pointerdown; the click handler covers browsers that don't dispatch pointer events to them.
    let lastDenied = null;
    let lastDeniedAt = 0;

    function playDenied(el) {
        if (el === lastDenied && performance.now() - lastDeniedAt < 1000) return;
        lastDenied = el;
        lastDeniedAt = performance.now();
        play("denied");
    }

    document.addEventListener("pointerdown", event => {
        if (event.button !== 0 || !(event.target instanceof Element)) return;
        const el = findTarget(event.target);
        if (el && isDisabled(el)) playDenied(el);
    }, { capture: true, passive: true });

    document.addEventListener("click", event => {
        if (!(event.target instanceof Element)) return;
        const el = findTarget(event.target);
        if (!el || el.matches("select")) return;
        if (isDisabled(el)) {
            playDenied(el);
            return;
        }
        const kind = classify(el);
        play(kind);
        if (kind === "select" && el.tagName === "IMG" && !el.closest("#planetmap")) pop(el);
    }, { capture: true, passive: true });

    document.addEventListener("change", event => {
        const el = event.target;
        if (el instanceof HTMLSelectElement || (el instanceof HTMLInputElement && el.type === "range")) play("select");
    }, { capture: true, passive: true });

    // Busy state for buttons whose click starts a server request (driven by Client via UiBusyBegin/UiBusyEnd).
    const busySelector = "button, .ui-btn, [role=button], input[type=submit], input[type=button]";
    const busyOps = new Set();
    let lastPressed = null;
    let lastPressedAt = 0;
    let busyElement = null;
    let busySeq = 0;
    let busyReleaseTimer = 0;

    function rememberPress(event) {
        if (!(event.target instanceof Element)) return;
        const el = event.target.closest(busySelector);
        if (!el || el.closest("#planetmap, .tanks-pile")) return;
        lastPressed = el;
        lastPressedAt = performance.now();
    }

    document.addEventListener("click", rememberPress, { capture: true, passive: true });
    document.addEventListener("submit", event => {
        const submitter = event.submitter;
        if (submitter instanceof Element) {
            lastPressed = submitter;
            lastPressedAt = performance.now();
        }
    }, { capture: true, passive: true });

    window.addEventListener("click", event => {
        if (event.target instanceof Element && event.target.closest("[aria-busy=true]")) {
            event.preventDefault();
            event.stopImmediatePropagation();
        }
    }, { capture: true });

    window.addEventListener("submit", event => {
        if (event.target instanceof Element && event.target.querySelector("[aria-busy=true]")) {
            event.preventDefault();
            event.stopImmediatePropagation();
        }
    }, { capture: true });

    window.UiBusyBegin = function () {
        const token = ++busySeq;
        if (!busyElement && lastPressed && lastPressed.isConnected && performance.now() - lastPressedAt < 800) {
            busyElement = lastPressed;
            busyElement.setAttribute("aria-busy", "true");
            busyElement.classList.add("is-busy");
            lastPressed = null;
        }
        if (busyElement) {
            clearTimeout(busyReleaseTimer);
            busyOps.add(token);
        }
        return token;
    };

    window.UiBusyEnd = function (token) {
        if (!busyOps.delete(token) || busyOps.size > 0) return;
        clearTimeout(busyReleaseTimer);
        // Short grace period so chained requests (e.g. login followed by status refresh) keep one busy state.
        busyReleaseTimer = setTimeout(() => {
            if (busyOps.size > 0 || !busyElement) return;
            busyElement.removeAttribute("aria-busy");
            busyElement.classList.remove("is-busy");
            busyElement = null;
        }, 120);
    };
})();
