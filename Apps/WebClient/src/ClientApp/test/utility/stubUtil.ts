export const stubbedPromise = <T = void>(): Promise<T> => {
    return new Promise(() => {
        // Stubbed Promise
    });
};

export const stubbedVoid = (): void => {
    // Stubbed Void
};
