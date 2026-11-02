function FindDealMapFit(width, height, reserveRight, visualRadius, markers, obstacles, alignTop = false) {
    const margin = 8;
    const availableWidth = Math.max(0, width - 2 * margin - reserveRight);
    const availableHeight = Math.max(0, height - 2 * margin);
    const maxScale = Math.min(availableWidth, availableHeight) / (2 * visualRadius);
    if (!Number.isFinite(maxScale) || maxScale <= 0) return null;

    const preferredX = (width - reserveRight) / 2;
    const preferredY = alignTop ? margin + visualRadius * maxScale : height / 2;
    const paddedObstacles = obstacles.map(rect => ({
        left: rect.left - 2,
        top: rect.top - 2,
        right: rect.right + 2,
        bottom: rect.bottom + 2
    }));

    function countObscured(scale, centerX, centerY) {
        let count = 0;
        for (const marker of markers) {
            const left = centerX + marker.left * scale;
            const top = centerY + marker.top * scale;
            const right = centerX + marker.right * scale;
            const bottom = centerY + marker.bottom * scale;
            if (paddedObstacles.some(rect =>
                left < rect.right && right > rect.left && top < rect.bottom && bottom > rect.top)) {
                count++;
            }
        }
        return count;
    }

    const initialObscured = countObscured(maxScale, preferredX, preferredY);
    const initial = { scale: maxScale, centerX: preferredX, centerY: preferredY, obscured: initialObscured };
    if (initialObscured === 0) return initial;

    // Prefer a larger map, then the position closest to its usual center.
    let best = initial;
    for (let step = 0; step <= 20; step++) {
        const scale = maxScale * (1 - 0.04 * step);
        const minX = margin + visualRadius * scale;
        const maxX = width - margin - reserveRight - visualRadius * scale;
        const minY = margin + visualRadius * scale;
        const maxY = height - margin - visualRadius * scale;
        const xSteps = maxX - minX < 0.01 ? 0 : 8;
        const ySteps = maxY - minY < 0.01 ? 0 : 8;
        const candidates = [];

        for (let x = 0; x <= xSteps; x++) {
            for (let y = 0; y <= ySteps; y++) {
                const centerX = xSteps ? minX + (maxX - minX) * x / xSteps : (minX + maxX) / 2;
                const centerY = ySteps ? minY + (maxY - minY) * y / ySteps : (minY + maxY) / 2;
                candidates.push({
                    centerX, centerY,
                    distance: (centerX - preferredX) ** 2 + (centerY - preferredY) ** 2
                });
            }
        }
        candidates.sort((a, b) => a.distance - b.distance);

        for (const candidate of candidates) {
            const obscured = countObscured(scale, candidate.centerX, candidate.centerY);
            if (obscured === 0) return { scale, centerX: candidate.centerX, centerY: candidate.centerY, obscured };
            if (obscured < best.obscured) best = { scale, centerX: candidate.centerX, centerY: candidate.centerY, obscured };
        }
    }

    return best;
}

if (typeof module !== 'undefined') module.exports = { FindDealMapFit };
