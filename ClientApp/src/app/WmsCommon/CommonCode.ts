import { Injectable, Inject } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class commonComponent {
  constructor() { }

  //animattion
  public animateCSS(formId, animatepostion) {
    const element = document.getElementById(formId);
    element.classList.add('animated', animatepostion);
    element.addEventListener('animationend', function () {
      element.classList.remove('animated', animatepostion);
    })
  }

  //export pdf
  exportPdf(exportColumns: any, List: any, fileName: any) {
    import("jspdf").then(jsPDF => {
      import("jspdf-autotable").then(x => {
        const doc = new jsPDF.default(0, 0);
        doc.autoTable(exportColumns, List);
        doc.save(fileName);
      })
    })
  }

  exportExcel(List: any, fileName: any) {
    import("xlsx").then(xlsx => {
      const worksheet = xlsx.utils.json_to_sheet(List);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, fileName);
    });
  }

  saveAsExcelFile(buffer: any, fileName: string): void {
    import("file-saver").then(FileSaver => {
      let EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
      let EXCEL_EXTENSION = '.xlsx';
      const data: Blob = new Blob([buffer], {
        type: EXCEL_TYPE
      });
      FileSaver.saveAs(data, fileName + '_export_' + new Date().getTime() + EXCEL_EXTENSION);
    });
  }
}



