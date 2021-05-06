export const voidPromise = <T = void>(): Promise<T> => {
    return new Promise(voidMethod);
};

export const voidMethod = (): void => {
    // Stubbed Void
};
