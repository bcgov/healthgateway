import { Identifier } from "@/ioc/identifier";

class Container {
    private mappings = new Map<Identifier, (c: Container) => any>();
    private cachedMappings = new Map<Identifier, any>();

    public set<T>(
        key: Identifier,
        factory: (c: Container) => T,
        singleton = true
    ): void {
        this.mappings.set(key, factory);
        if (singleton) {
            this.cachedMappings.set(key, factory(this));
        }
    }

    public get<T>(key: Identifier): T {
        const cachedResult = this.cachedMappings.get(key);
        return cachedResult ?? this.mappings.get(key)?.(this);
    }
}

export const container = new Container();
