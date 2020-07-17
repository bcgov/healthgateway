export default class PageError {
    public code: string;
    public name: string;
    public message: string;

    constructor(code: string, name: string, message: string) {
        this.code = code;
        this.name = name;
        this.message = message;
    }
}
