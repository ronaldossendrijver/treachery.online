const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');
const { FindDealMapFit } = require(path.join(__dirname, '..', 'Client', 'wwwroot', 'js', 'map-deal-fit.js'));

function isClear(fit, marker, obstacle) {
    const left = fit.centerX + marker.left * fit.scale;
    const right = fit.centerX + marker.right * fit.scale;
    const top = fit.centerY + marker.top * fit.scale;
    const bottom = fit.centerY + marker.bottom * fit.scale;
    return right <= obstacle.left || left >= obstacle.right || bottom <= obstacle.top || top >= obstacle.bottom;
}

function assertFits(fit, width, height, reserveRight, radius, markers, obstacles) {
    assert.ok(fit);
    assert.equal(fit.obscured, 0);
    assert.ok(fit.centerX - radius * fit.scale >= 8 - 1e-8);
    assert.ok(fit.centerX + radius * fit.scale <= width - 8 - reserveRight + 1e-8);
    assert.ok(fit.centerY - radius * fit.scale >= 8 - 1e-8);
    assert.ok(fit.centerY + radius * fit.scale <= height - 8 + 1e-8);
    for (const marker of markers) {
        for (const obstacle of obstacles) assert.ok(isClear(fit, marker, obstacle));
    }
}

test('preserves the original map fit when deal indicators are unobstructed', () => {
    const fit = FindDealMapFit(900, 700, 100, 300, [], []);
    assert.deepEqual(fit, { scale: 684 / 600, centerX: 400, centerY: 350, obscured: 0 });

    const marker = { left: -10, top: -320, right: 10, bottom: -300 };
    const unobstructed = FindDealMapFit(900, 700, 0, 330, [marker], [
        { left: 10, top: 500, right: 200, bottom: 690 }
    ]);
    assert.equal(unobstructed.scale, 684 / 660);
    assert.equal(unobstructed.centerX, 450);
    assert.equal(unobstructed.centerY, 350);
});

test('moves the map before reducing its scale when a log covers an indicator', () => {
    const marker = { left: -260, top: 190, right: -240, bottom: 210 };
    const obstacles = [{ left: 100, top: 450, right: 250, bottom: 650 }];
    const fit = FindDealMapFit(900, 700, 0, 300, [marker], obstacles);
    assert.equal(fit.scale, 684 / 600);
    assert.ok(fit.centerX > 450);
    assertFits(fit, 900, 700, 0, 300, [marker], obstacles);
});

test('keeps a crowded twelve-player layout reachable within a small board', () => {
    const radius = 380;
    const markers = [];
    for (let seat = 0; seat < 12; seat++) {
        for (let row = 0; row < 2; row++) {
            for (let column = -1; column <= 1; column++) {
                const angle = seat * Math.PI / 6 + column * 0.1;
                const x = Math.cos(angle) * (320 + row * 30);
                const y = Math.sin(angle) * (320 + row * 30);
                markers.push({ left: x - 10, right: x + 10, top: y - 10, bottom: y + 10 });
            }
        }
    }
    const obstacles = [
        { left: 0, top: 0, right: 320, bottom: 48 },
        { left: 6, top: 52, right: 314, bottom: 87 },
        { left: 142, top: 93, right: 314, bottom: 117 },
        { left: 8, top: 379, right: 228, bottom: 496 },
        { left: 268, top: 408, right: 312, bottom: 496 },
        { left: 0, top: 510, right: 320, bottom: 568 }
    ];
    const fit = FindDealMapFit(320, 568, 0, radius, markers, obstacles);
    assertFits(fit, 320, 568, 0, radius, markers, obstacles);
    assert.ok(fit.scale >= 0.8 * (304 / (2 * radius)));
});

test('reports when no usable placement exists', () => {
    const marker = { left: -10, top: -10, right: 10, bottom: 10 };
    const fit = FindDealMapFit(320, 568, 0, 100, [marker], [
        { left: 0, top: 0, right: 320, bottom: 568 }
    ]);
    assert.equal(fit.obscured, 1);
    assert.equal(FindDealMapFit(0, 0, 0, 100, [marker], []), null);
});

test('phone fit puts the planet just below the menu, leaving spare height for the log', () => {
    for (const [width, height] of [[360, 640], [412, 840], [760, 1200]]) {
        const top = 96, bottom = 53, radius = 380;
        const availableHeight = height - top - bottom;
        const fit = FindDealMapFit(width, availableHeight, 0, radius, [], [], true);
        assertFits(fit, width, availableHeight, 0, radius, [], []);
        assert.ok(Math.abs(fit.centerY - radius * fit.scale - 8) < 1e-8);
        const centered = FindDealMapFit(width, availableHeight, 0, radius, [], []);
        assert.equal(fit.scale, centered.scale);
        assert.ok(fit.centerY <= centered.centerY);
    }
});

test('top-aligned phone fit still avoids deal indicator obstacles', () => {
    const marker = { left: -10, top: -360, right: 10, bottom: -340 };
    const obstacle = { left: 140, top: 0, right: 220, bottom: 45 };
    const fit = FindDealMapFit(360, 560, 0, 380, [marker], [obstacle], true);
    assertFits(fit, 360, 560, 0, 380, [marker], [obstacle]);
});
