/** v-tooltip location "end center", offset 10: right of the activator, centred, flipped left / clamped to fit. */
export function tooltipPosition(
    anchor: { x: number; y: number; width: number; height: number },
    bubble: { width: number; height: number },
    windowWidth: number,
    windowHeight: number
): { left: number; top: number } {
    const offset = 10;
    let left = anchor.x + anchor.width + offset;
    if (left + bubble.width > windowWidth - 8)
        left = anchor.x - offset - bubble.width;
    left = Math.max(8, Math.min(left, windowWidth - bubble.width - 8));
    const top = Math.max(
        8,
        Math.min(
            anchor.y + anchor.height / 2 - bubble.height / 2,
            windowHeight - bubble.height - 8
        )
    );
    return { left, top };
}
