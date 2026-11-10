let generation = 0;

/** Which login this is. Anything started under an older one must not touch the app's state. */
export const sessionGeneration = () => generation;

export function endSessionGeneration(): void {
    generation += 1;
}
