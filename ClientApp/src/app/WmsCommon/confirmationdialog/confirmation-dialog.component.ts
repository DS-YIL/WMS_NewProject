import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-confirmation-dialog',
  templateUrl: './confirmation-dialog.component.html',
  //styleUrls: ['./confirmation-dialog.component.scss']
})
export class ConfirmationDialogComponent  {

  constructor( public dialogRef: MatDialogRef<ConfirmationDialogComponent>,
               @Inject(MAT_DIALOG_DATA) public message: string) { }

    onNoClick(): void {
      this.dialogRef.close();
    }

    public animateCSS(formId, animatepostion) {
      const element = document.getElementById(formId);
      element.classList.add('animated', animatepostion);
      element.addEventListener('animationend', function () {
        element.classList.remove('animated', animatepostion);
      })
    }
  

}
