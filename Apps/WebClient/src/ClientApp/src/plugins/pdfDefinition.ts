/**
 * Shallow representation to be used by html2pdf. Does not define the object complety, but sufficiently to avoid using any.
 * To be expanded as needed.
 */
export default interface PDFDefinition {
    internal: {
        getNumberOfPages(): number;
        pageSize: { getWidth(): number; getHeight(): number };
    };
    setPage(page: number): void;
    setFontSize(size: number): void;
    setTextColor(color: number): void;
    text(text: string, width: number, height: number): void;
}
