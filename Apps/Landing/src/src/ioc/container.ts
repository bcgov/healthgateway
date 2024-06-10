import { Identifier } from "@/ioc/identifier";

interface IContainer {
    set<T>(key: Identifier, factory: (c: Container) => T): void;
    get<T>(key: Identifier): T;
}

class Container implements IContainer {
    private mappings = new Map<Identifier, (c: Container) => any>();
    private cachedMappings = new Map<Identifier, any>();

    public set<T>(key: Identifier, factory: (c: Container) => T): void {
        this.mappings.set(key, factory);
    }

    public get<T>(key: Identifier): T {
        let result = this.cachedMappings.get(key);
        if (!result) {
            result = this.mappings.get(key)?.(this);
            this.cachedMappings.set(key, result);
        }
        return result;
    }
}

export const container: IContainer = new Container();
