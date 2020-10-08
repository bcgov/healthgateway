export class FragmentedStorage implements Storage {
    private internalStorage: Storage;
    private maxSize: number;
    private readonly fragmentIndicator = "split~";
    private readonly fragmentSeparator = "&~&";
    private readonly keySeparator = ".";

    constructor(storageImplementation: Storage, maxSize: number) {
        this.internalStorage = storageImplementation;
        this.maxSize = maxSize;
    }

    get length(): number {
        return this.internalStorage.length;
    }

    public key(index: number): string | null {
        return this.internalStorage.key(index);
    }

    public clear(): void {
        this.internalStorage.clear();
    }

    public setItem(key: string, data: string): void {
        this.removeItem(key);
        if (data.length < this.maxSize) {
            this.internalStorage.setItem(key, data);
        } else {
            const fragmented = this.chunkString(
                data,
                this.maxSize
            ) as RegExpMatchArray;
            const fragmentKeys: string[] = [];
            for (let i = 0; i < fragmented.length; i++) {
                const fragmentKey = key + this.keySeparator + (i + 1);
                fragmentKeys.push(fragmentKey);
                this.internalStorage.setItem(fragmentKey, fragmented[i]);
            }

            this.internalStorage.setItem(
                key,
                this.fragmentIndicator +
                    fragmentKeys.join(this.fragmentSeparator)
            );
        }
    }

    public getItem(key: string): string | null {
        let storedItem = this.internalStorage.getItem(key);
        if (this.isFragment(storedItem)) {
            storedItem = storedItem as string;
            const fragmentSection = this.getFragmentSection(storedItem);

            const fragmentNames = this.getFragmentNames(fragmentSection);
            const item = this.assembleFragments(fragmentNames);
            return item;
        }

        return storedItem;
    }

    public removeItem(key: string): void {
        let storedItem = this.internalStorage.getItem(key);
        if (this.isFragment(storedItem)) {
            storedItem = storedItem as string;
            const fragmentSection = this.getFragmentSection(storedItem);
            const fragmentNames = this.getFragmentNames(fragmentSection);

            for (let i = 0; i < fragmentNames.length; i++) {
                const fragment = fragmentNames[i];
                this.internalStorage.removeItem(fragment);
            }
        }

        this.internalStorage.removeItem(key);
    }

    private isFragment(storedItem: string | null): boolean {
        return storedItem !== null
            ? storedItem.startsWith(this.fragmentIndicator)
            : false;
    }

    private getFragmentSection(fragment: string): string {
        const fragmentSection = fragment.substr(
            this.fragmentIndicator.length,
            fragment?.length
        );
        return fragmentSection;
    }

    private getFragmentNames(fragmentSection: string): string[] {
        return fragmentSection.split(this.fragmentSeparator).sort((a, b) => {
            const aIndex = a.lastIndexOf(this.keySeparator);
            const bIndex = b.lastIndexOf(this.keySeparator);
            const aNum = Number(a.substr(aIndex + 1, a.length));
            const bNum = Number(b.substr(bIndex + 1, b.length));
            return aNum > bNum ? 1 : aNum < bNum ? -1 : 0;
        });
    }

    private assembleFragments(fragmentNames: string[]): string {
        let reconstitutedItem = "";
        for (let i = 0; i < fragmentNames.length; i++) {
            reconstitutedItem += this.internalStorage.getItem(fragmentNames[i]);
        }
        return reconstitutedItem;
    }

    private chunkString(str: string, length: number): RegExpMatchArray | null {
        return str.match(new RegExp(".{1," + length + "}", "g"));
    }
}
