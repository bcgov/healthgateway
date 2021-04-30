export const stubbedPromise = <T = void>(): Promise<T> => {
    return new Promise(voidMethod);
};

export const voidMethod = (): void => {
    // Stubbed Void
};
