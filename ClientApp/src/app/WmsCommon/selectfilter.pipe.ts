import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'selectfilter',
  pure: false
})
export class SelectfilterPipe implements PipeTransform {

  transform(items: any[], filter: string): any {
    if (!items || !filter) {
        return items;
    }
   
    // filter items array, items which match and return true will be
    // kept, false will be filtered out
    return items.filter(item => item.AccessGroupMaster.GroupName.indexOf(filter) !== -1);
}




}