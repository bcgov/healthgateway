import html2pdf from "html2pdf.js";

import PDFDefinition from "@/plugins/pdfDefinition";

export default abstract class PDFUtil {
    public static async generatePdf(
        fileName: string,
        element: HTMLElement,
        footerText?: string
    ): Promise<void> {
        // Fixes a possible bug with html2canvas where if the page is scrolled it could cut off the image.
        const scrollX = window.scrollX;
        const scrollY = window.scrollY;
        window.scrollTo(0, 0);
        const opt = {
            margin: [25, 15],
            filename: fileName,
            image: { type: "jpeg", quality: 1 },
            html2canvas: { dpi: 96, scale: 2, letterRendering: true },
            jsPDF: { unit: "pt", format: "letter", orientation: "portrait" },
            pagebreak: { mode: ["avoid-all", "css", "legacy"] },
        };
        return html2pdf()
            .set(opt)
            .from(element)
            .toPdf()
            .get("pdf")
            .then((pdf: PDFDefinition) => {
                // Add footer with page numbers
                const totalPages = pdf.internal.getNumberOfPages();
                for (let i = 1; i <= totalPages; i++) {
                    pdf.setPage(i);
                    pdf.setFontSize(10);
                    pdf.setTextColor(150);
                    pdf.text(
                        `Page ${i} of ${totalPages}`,
                        pdf.internal.pageSize.getWidth() / 2 - 55,
                        pdf.internal.pageSize.getHeight() - 10
                    );
                    if (footerText) {
                        pdf.text(
                            footerText,
                            pdf.internal.pageSize.getWidth() / 2 + 55,
                            pdf.internal.pageSize.getHeight() - 10
                        );
                    }
                }
            })
            .save()
            .output("bloburl")
            .then((pdfBlobUrl: RequestInfo) => {
                fetch(pdfBlobUrl).then((res) => {
                    res.blob().then(() => {
                        window.scrollTo(scrollX, scrollY);
                    });
                });
            });
    }
}
